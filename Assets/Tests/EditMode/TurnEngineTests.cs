using NUnit.Framework;

namespace ShadowTileEscape.Tests
{
    public sealed class TurnEngineTests
    {
        static LevelState Empty(int width = 5, int height = 5)
        {
            return new LevelState
            {
                width = width,
                height = height,
                cells = new CellFlags[width * height],
                player = new GridCoord(1, 1),
                playerFacing = Direction.East,
                exit = new GridCoord(width - 1, height - 1)
            };
        }

        [Test]
        public void LightIncludesTraversedCellsButNotSource()
        {
            var state = Empty();
            state.lights = new[] { new LightSourceState { position = new GridCoord(1, 2), direction = Direction.East, range = 2, active = true } };
            var lit = new LightSolver().Solve(state);
            Assert.That(lit[state.Index(new GridCoord(1, 2))], Is.Zero);
            Assert.That(lit[state.Index(new GridCoord(2, 2))], Is.EqualTo(1));
            Assert.That(lit[state.Index(new GridCoord(3, 2))], Is.EqualTo(1));
            Assert.That(lit[state.Index(new GridCoord(4, 2))], Is.Zero);
        }

        [Test]
        public void OpaqueBoxStopsBeforeItsCell()
        {
            var state = Empty();
            state.boxes = new[] { new GridCoord(3, 2) };
            state.lights = new[] { new LightSourceState { position = new GridCoord(0, 2), direction = Direction.East, range = 5, active = true } };
            var lit = new LightSolver().Solve(state);
            Assert.That(lit[state.Index(new GridCoord(2, 2))], Is.EqualTo(1));
            Assert.That(lit[state.Index(new GridCoord(3, 2))], Is.Zero);
            Assert.That(lit[state.Index(new GridCoord(4, 2))], Is.Zero);
        }

        [Test]
        public void SlashMirrorReflectsEastToNorth()
        {
            var state = Empty();
            state.mirrors = new[] { new MirrorState { position = new GridCoord(2, 1), kind = MirrorKind.Slash } };
            state.lights = new[] { new LightSourceState { position = new GridCoord(0, 1), direction = Direction.East, range = 4, active = true } };
            var lit = new LightSolver().Solve(state);
            Assert.That(lit[state.Index(new GridCoord(2, 1))], Is.EqualTo(1));
            Assert.That(lit[state.Index(new GridCoord(2, 2))], Is.EqualTo(1));
            Assert.That(lit[state.Index(new GridCoord(2, 3))], Is.EqualTo(1));
        }

        [TestCase(MirrorKind.Slash, Direction.East, Direction.North)]
        [TestCase(MirrorKind.Slash, Direction.North, Direction.East)]
        [TestCase(MirrorKind.Slash, Direction.West, Direction.South)]
        [TestCase(MirrorKind.Slash, Direction.South, Direction.West)]
        [TestCase(MirrorKind.Backslash, Direction.East, Direction.South)]
        [TestCase(MirrorKind.Backslash, Direction.South, Direction.East)]
        [TestCase(MirrorKind.Backslash, Direction.West, Direction.North)]
        [TestCase(MirrorKind.Backslash, Direction.North, Direction.West)]
        public void MirrorReflectionCoversEveryIncomingDirection(MirrorKind kind, Direction incoming, Direction outgoing)
        {
            var state = Empty();
            var mirror = new GridCoord(2, 2);
            var incomingOffset = GridDirections.Offset(incoming);
            var source = new GridCoord(mirror.x - incomingOffset.x, mirror.y - incomingOffset.y);
            state.mirrors = new[] { new MirrorState { position = mirror, kind = kind } };
            state.lights = new[] { new LightSourceState { position = source, direction = incoming, range = 3, active = true } };
            var lit = new LightSolver().Solve(state);
            Assert.That(lit[state.Index(mirror + GridDirections.Offset(outgoing))], Is.EqualTo(1));
        }

        [Test]
        public void InvalidMoveDoesNotMutateOrCreateHistory()
        {
            var state = Empty();
            state.player = new GridCoord(0, 0);
            var engine = new TurnEngine(state);
            var result = engine.TryExecute(PlayerCommand.Move(Direction.West));
            Assert.That(result.outcome, Is.EqualTo(TurnOutcome.Invalid));
            Assert.That(engine.State.player, Is.EqualTo(new GridCoord(0, 0)));
            Assert.That(engine.HistoryCount, Is.Zero);
        }

        [Test]
        public void FailedLightTurnCanBeUndone()
        {
            var state = Empty();
            state.player = new GridCoord(1, 1);
            state.lights = new[] { new LightSourceState { position = new GridCoord(0, 2), direction = Direction.East, range = 5, active = true } };
            var engine = new TurnEngine(state);
            Assert.That(engine.TryExecute(PlayerCommand.Move(Direction.North)).outcome, Is.EqualTo(TurnOutcome.Failed));
            Assert.That(engine.Undo(), Is.True);
            Assert.That(engine.State.player, Is.EqualTo(new GridCoord(1, 1)));
            Assert.That(engine.State.failed, Is.False);
        }

        [Test]
        public void LampInteractionRotatesClockwiseAndCountsTurn()
        {
            var state = Empty();
            state.lights = new[] { new LightSourceState { position = new GridCoord(2, 1), direction = Direction.North, range = 2, active = true } };
            var engine = new TurnEngine(state);
            Assert.That(engine.TryExecute(PlayerCommand.Interact()).Accepted, Is.True);
            Assert.That(engine.State.lights[0].direction, Is.EqualTo(Direction.East));
            Assert.That(engine.State.moveCount, Is.EqualTo(1));
        }

        [Test]
        public void SimultaneousGuardsWaitOnContestedDestination()
        {
            var state = Empty();
            state.player = new GridCoord(0, 0);
            state.guards = new[]
            {
                new GuardState { position = new GridCoord(1, 3), patrol = new[] { new GridCoord(1, 3), new GridCoord(2, 3) } },
                new GuardState { position = new GridCoord(3, 3), patrol = new[] { new GridCoord(3, 3), new GridCoord(2, 3) } }
            };
            var engine = new TurnEngine(state);
            engine.TryExecute(PlayerCommand.Move(Direction.East));
            Assert.That(engine.State.guards[0].position, Is.EqualTo(new GridCoord(1, 3)));
            Assert.That(engine.State.guards[1].position, Is.EqualTo(new GridCoord(3, 3)));
        }

        [Test]
        public void ThreeGuardsAllWaitOnOneContestedDestination()
        {
            var state = Empty();
            state.player = new GridCoord(0, 0);
            state.guards = new[]
            {
                new GuardState { position = new GridCoord(1, 3), patrol = new[] { new GridCoord(1, 3), new GridCoord(2, 3) } },
                new GuardState { position = new GridCoord(3, 3), patrol = new[] { new GridCoord(3, 3), new GridCoord(2, 3) } },
                new GuardState { position = new GridCoord(2, 4), patrol = new[] { new GridCoord(2, 4), new GridCoord(2, 3) } }
            };
            var engine = new TurnEngine(state);
            engine.TryExecute(PlayerCommand.Move(Direction.East));
            Assert.That(engine.State.guards[0].position, Is.EqualTo(new GridCoord(1, 3)));
            Assert.That(engine.State.guards[1].position, Is.EqualTo(new GridCoord(3, 3)));
            Assert.That(engine.State.guards[2].position, Is.EqualTo(new GridCoord(2, 4)));
        }

        [Test]
        public void ExitRequiresConfiguredShardCount()
        {
            var state = Empty(3, 3);
            state.player = new GridCoord(0, 0);
            state.exit = new GridCoord(1, 0);
            state.requiredShards = 1;
            state.shards = new[] { new GridCoord(0, 1) };
            state.collectedShards = new bool[1];
            var engine = new TurnEngine(state);
            Assert.That(engine.TryExecute(PlayerCommand.Move(Direction.East)).outcome, Is.EqualTo(TurnOutcome.Advanced));
            Assert.That(engine.State.completed, Is.False);
        }

        [Test]
        public void CurtainInteractionOpensLightPath()
        {
            var state = Empty();
            state.player = new GridCoord(2, 1);
            state.playerFacing = Direction.North;
            state.curtains = new[] { new CurtainState { position = new GridCoord(2, 2), open = false } };
            state.lights = new[] { new LightSourceState { position = new GridCoord(0, 2), direction = Direction.East, range = 5, active = true } };
            var before = new LightSolver().Solve(state);
            Assert.That(before[state.Index(new GridCoord(3, 2))], Is.Zero);
            var engine = new TurnEngine(state);
            Assert.That(engine.TryExecute(PlayerCommand.Interact()).Accepted, Is.True);
            Assert.That(engine.State.curtains[0].open, Is.True);
            Assert.That(new LightSolver().Solve(engine.State)[state.Index(new GridCoord(3, 2))], Is.EqualTo(1));
        }

        [Test]
        public void BoxInteractionPushesBlockerAndChangesLight()
        {
            var state = Empty();
            state.player = new GridCoord(1, 1);
            state.playerFacing = Direction.North;
            state.boxes = new[] { new GridCoord(1, 2) };
            state.lights = new[] { new LightSourceState { position = new GridCoord(0, 2), direction = Direction.East, range = 5, active = true } };
            var engine = new TurnEngine(state);
            Assert.That(engine.TryExecute(PlayerCommand.Interact()).Accepted, Is.True);
            Assert.That(engine.State.boxes[0], Is.EqualTo(new GridCoord(1, 3)));
            Assert.That(new LightSolver().Solve(engine.State)[state.Index(new GridCoord(1, 2))], Is.EqualTo(1));
        }

        [Test]
        public void MovingMoonlightAdvancesBeforeFinalSafetyCheck()
        {
            var state = Empty();
            state.player = new GridCoord(1, 1);
            state.movingLights = new[]
            {
                new MovingLightState
                {
                    path = new[] { new GridCoord(0, 3), new GridCoord(0, 1) },
                    directions = new[] { Direction.East, Direction.East },
                    range = 5
                }
            };
            var engine = new TurnEngine(state);
            Assert.That(engine.TryExecute(PlayerCommand.Move(Direction.East)).outcome, Is.EqualTo(TurnOutcome.Failed));
            Assert.That(engine.State.movingLights[0].pathIndex, Is.EqualTo(1));
            Assert.That(engine.Undo(), Is.True);
            Assert.That(engine.State.movingLights[0].pathIndex, Is.Zero);
        }

        [Test]
        public void GuardianLanternUsesResolvedFacingForFinalSafety()
        {
            var state = Empty();
            state.player = new GridCoord(1, 0);
            state.guards = new[]
            {
                new GuardState
                {
                    position = new GridCoord(4, 1),
                    patrol = new[] { new GridCoord(4, 1), new GridCoord(3, 1) },
                    facing = Direction.North,
                    lightRange = 3
                }
            };
            var engine = new TurnEngine(state);
            Assert.That(engine.TryExecute(PlayerCommand.Move(Direction.North)).outcome, Is.EqualTo(TurnOutcome.Failed));
            Assert.That(engine.State.guards[0].position, Is.EqualTo(new GridCoord(3, 1)));
            Assert.That(engine.State.guards[0].facing, Is.EqualTo(Direction.West));
        }

        [Test]
        public void GuardPreviewUsesSameResolverAsExecution()
        {
            var state = Empty();
            state.player = new GridCoord(0, 0);
            state.guards = new[]
            {
                new GuardState { position = new GridCoord(1, 3), patrol = new[] { new GridCoord(1, 3), new GridCoord(2, 3) } }
            };
            var engine = new TurnEngine(state);
            var preview = engine.PreviewGuardPositions();
            engine.TryExecute(PlayerCommand.Move(Direction.East));
            Assert.That(engine.State.guards[0].position, Is.EqualTo(preview[0]));
        }

        [Test]
        public void HeadOnGuardSwapMakesBothWait()
        {
            var state = Empty();
            state.player = new GridCoord(0, 0);
            state.guards = new[]
            {
                new GuardState { position = new GridCoord(1, 3), patrol = new[] { new GridCoord(1, 3), new GridCoord(2, 3) } },
                new GuardState { position = new GridCoord(2, 3), patrol = new[] { new GridCoord(2, 3), new GridCoord(1, 3) } }
            };
            var engine = new TurnEngine(state);
            engine.TryExecute(PlayerCommand.Move(Direction.East));
            Assert.That(engine.State.guards[0].position, Is.EqualTo(new GridCoord(1, 3)));
            Assert.That(engine.State.guards[1].position, Is.EqualTo(new GridCoord(2, 3)));
        }
    }
}
