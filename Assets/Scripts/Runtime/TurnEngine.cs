using System;
using System.Collections.Generic;

namespace ShadowTileEscape
{
    public readonly struct TurnResult
    {
        public readonly TurnOutcome outcome;
        public readonly string reason;
        public bool Accepted => outcome != TurnOutcome.Invalid;

        public TurnResult(TurnOutcome outcome, string reason = "")
        {
            this.outcome = outcome;
            this.reason = reason;
        }
    }

    public sealed class TurnEngine
    {
        readonly LightSolver lightSolver = new LightSolver();
        readonly List<LevelState> history = new List<LevelState>(64);
        readonly int historyLimit;

        public LevelState State { get; private set; }
        public int HistoryCount => history.Count;

        public TurnEngine(LevelState initialState, int historyLimit = 64)
        {
            State = initialState.Copy();
            this.historyLimit = Math.Max(1, historyLimit);
        }

        public TurnResult TryExecute(PlayerCommand command)
        {
            if (State.failed || State.completed) return new TurnResult(TurnOutcome.Invalid, "Level is not accepting commands.");

            var next = State.Copy();
            var valid = command.type == CommandType.Move ? TryMove(next, command.direction) : TryInteract(next);
            if (!valid) return new TurnResult(TurnOutcome.Invalid, "Command cannot change the current state.");

            PushHistory(State.Copy());
            next.moveCount++;

            if (IsPlayerUnsafe(next))
            {
                next.failed = true;
                State = next;
                return new TurnResult(TurnOutcome.Failed, "Noor entered the light.");
            }

            AdvanceGuards(next);
            AdvanceMovingLights(next);
            if (IsPlayerUnsafe(next) || HasGuardAt(next, next.player))
            {
                next.failed = true;
                State = next;
                return new TurnResult(TurnOutcome.Failed, "A guardian found Noor.");
            }

            CollectShards(next);
            if (next.player == next.exit && ObjectivesMet(next))
            {
                next.completed = true;
                State = next;
                return new TurnResult(TurnOutcome.Completed);
            }

            State = next;
            return new TurnResult(TurnOutcome.Advanced);
        }

        public bool Undo()
        {
            if (history.Count == 0) return false;
            var last = history.Count - 1;
            State = history[last];
            history.RemoveAt(last);
            return true;
        }

        public void Restart(LevelState initialState)
        {
            State = initialState.Copy();
            history.Clear();
        }

        public GridCoord[] PreviewGuardPositions()
        {
            var preview = State.Copy();
            AdvanceGuards(preview);
            var positions = new GridCoord[preview.guards.Length];
            for (var i = 0; i < positions.Length; i++) positions[i] = preview.guards[i].position;
            return positions;
        }

        bool TryMove(LevelState state, Direction direction)
        {
            state.playerFacing = direction;
            var target = state.player + GridDirections.Offset(direction);
            if (!IsWalkable(state, target) || HasGuardAt(state, target)) return false;
            state.player = target;
            return true;
        }

        static bool TryInteract(LevelState state)
        {
            var target = state.player + GridDirections.Offset(state.playerFacing);
            for (var i = 0; i < state.lights.Length; i++)
            {
                if (state.lights[i].position != target) continue;
                var item = state.lights[i];
                if (item.fixedDirection) return false;
                var next = item.direction;
                var changed = false;
                for (var step = 1; step <= 4; step++)
                {
                    var candidate = (Direction)(((int)item.direction + step) % 4);
                    if (item.allowedDirectionMask != 0 && (item.allowedDirectionMask & (1 << (int)candidate)) == 0) continue;
                    next = candidate;
                    changed = candidate != item.direction;
                    break;
                }
                if (!changed) return false;
                item.direction = next;
                state.lights[i] = item;
                state.lampRotations++;
                return true;
            }
            for (var i = 0; i < state.mirrors.Length; i++)
            {
                if (state.mirrors[i].position != target || !state.mirrors[i].rotatable) continue;
                var item = state.mirrors[i];
                item.kind = item.kind == MirrorKind.Slash ? MirrorKind.Backslash : MirrorKind.Slash;
                state.mirrors[i] = item;
                state.mirrorRotations++;
                return true;
            }
            for (var i = 0; i < state.curtains.Length; i++)
            {
                if (state.curtains[i].position != target) continue;
                var item = state.curtains[i];
                item.open = !item.open;
                state.curtains[i] = item;
                state.curtainToggles++;
                return true;
            }
            for (var i = 0; i < state.boxes.Length; i++)
            {
                if (state.boxes[i] != target) continue;
                var beyond = target + GridDirections.Offset(state.playerFacing);
                if (!CanBoxEnter(state, beyond)) return false;
                state.boxes[i] = beyond;
                state.boxPushes++;
                return true;
            }
            return false;
        }

        bool IsPlayerUnsafe(LevelState state) => lightSolver.Solve(state)[state.Index(state.player)] > 0;

        static bool IsWalkable(LevelState state, GridCoord c)
        {
            if (!state.Contains(c) || state.HasCellFlag(c, CellFlags.Wall | CellFlags.Void)) return false;
            for (var i = 0; i < state.lights.Length; i++) if (state.lights[i].position == c) return false;
            for (var i = 0; i < state.mirrors.Length; i++) if (state.mirrors[i].position == c) return false;
            for (var i = 0; i < state.boxes.Length; i++) if (state.boxes[i] == c) return false;
            for (var i = 0; i < state.curtains.Length; i++)
                if (state.curtains[i].position == c && !state.curtains[i].open) return false;
            return true;
        }

        static bool HasGuardAt(LevelState state, GridCoord c)
        {
            for (var i = 0; i < state.guards.Length; i++) if (state.guards[i].position == c) return true;
            return false;
        }

        static bool CanBoxEnter(LevelState state, GridCoord c)
        {
            if (!IsWalkable(state, c) || HasGuardAt(state, c) || state.player == c || state.exit == c) return false;
            for (var i = 0; i < state.curtains.Length; i++) if (state.curtains[i].position == c) return false;
            return true;
        }

        static void AdvanceGuards(LevelState state)
        {
            var count = state.guards.Length;
            if (count == 0) return;
            var targets = new GridCoord[count];
            var moves = new bool[count];
            for (var i = 0; i < count; i++)
            {
                var guard = state.guards[i];
                if (guard.patrol.Length == 0) { targets[i] = guard.position; continue; }
                var nextIndex = (guard.patrolIndex + 1) % guard.patrol.Length;
                targets[i] = guard.patrol[nextIndex];
                moves[i] = IsWalkable(state, targets[i]);
            }
            var rejected = new bool[count];
            for (var i = 0; i < count; i++)
            {
                if (!moves[i]) continue;
                for (var j = i + 1; j < count; j++)
                {
                    var contested = targets[i] == targets[j];
                    var swap = targets[i] == state.guards[j].position && targets[j] == state.guards[i].position;
                    if (contested || swap) { rejected[i] = true; rejected[j] = true; }
                }
            }
            for (var i = 0; i < count; i++) if (rejected[i]) moves[i] = false;
            for (var pass = 0; pass < count; pass++)
            {
                var changed = false;
                for (var i = 0; i < count; i++)
                {
                    if (!moves[i]) continue;
                    for (var j = 0; j < count; j++)
                    {
                        if (i == j || targets[i] != state.guards[j].position || moves[j]) continue;
                        moves[i] = false;
                        changed = true;
                        break;
                    }
                }
                if (!changed) break;
            }
            for (var i = 0; i < count; i++)
            {
                if (!moves[i]) continue;
                var guard = state.guards[i];
                guard.facing = DirectionFromDelta(guard.position, targets[i], guard.facing);
                guard.position = targets[i];
                guard.patrolIndex = (guard.patrolIndex + 1) % guard.patrol.Length;
            }
        }

        static void AdvanceMovingLights(LevelState state)
        {
            for (var i = 0; i < state.movingLights.Length; i++)
            {
                var moving = state.movingLights[i];
                if (!moving.active || moving.path.Length == 0) continue;
                moving.pathIndex = (moving.pathIndex + 1) % moving.path.Length;
            }
        }

        static Direction DirectionFromDelta(GridCoord from, GridCoord to, Direction fallback)
        {
            var dx = to.x - from.x;
            var dy = to.y - from.y;
            if (Math.Abs(dx) > Math.Abs(dy)) return dx > 0 ? Direction.East : Direction.West;
            if (dy != 0) return dy > 0 ? Direction.North : Direction.South;
            return fallback;
        }

        static void CollectShards(LevelState state)
        {
            for (var i = 0; i < state.shards.Length; i++)
                if (!state.collectedShards[i] && state.shards[i] == state.player) state.collectedShards[i] = true;
        }

        static bool ObjectivesMet(LevelState state)
        {
            return state.ShardsCollected >= state.requiredShards
                && state.lampRotations >= state.requiredLampRotations
                && state.mirrorRotations >= state.requiredMirrorRotations
                && state.boxPushes >= state.requiredBoxPushes
                && state.curtainToggles >= state.requiredCurtainToggles;
        }

        void PushHistory(LevelState snapshot)
        {
            if (history.Count == historyLimit) history.RemoveAt(0);
            history.Add(snapshot);
        }
    }
}
