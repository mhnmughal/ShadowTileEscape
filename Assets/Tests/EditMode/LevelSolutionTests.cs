using NUnit.Framework;
using UnityEditor;

namespace ShadowTileEscape.Tests
{
    public sealed class LevelSolutionTests
    {
        [TestCaseSource(nameof(LevelNumbers))]
        public void SerializedSolutionCompletesLevel(int levelNumber)
        {
            var definition = AssetDatabase.LoadAssetAtPath<LevelDefinition>($"Assets/Data/Levels/Level_{levelNumber:00}.asset");
            Assert.That(definition, Is.Not.Null);
            var engine = new TurnEngine(definition.CreateState());
            var tokens = definition.verifiedSolution.Split(',');
            for (var turn = 0; turn < tokens.Length; turn++)
            {
                var result = engine.TryExecute(Parse(tokens[turn]));
                Assert.That(result.Accepted, Is.True, $"Level {levelNumber:00} rejected turn {turn + 1} ({tokens[turn]}): {result.reason}");
                Assert.That(engine.State.failed, Is.False, $"Level {levelNumber:00} failed on turn {turn + 1} ({tokens[turn]}): {result.reason}");
            }
            Assert.That(engine.State.completed, Is.True, $"Level {levelNumber:00} solution ended without completing objectives.");
            Assert.That(engine.State.moveCount, Is.EqualTo(tokens.Length));
            Assert.That(tokens.Length, Is.LessThanOrEqualTo(definition.par));
        }

        static int[] LevelNumbers()
        {
            var values = new int[15];
            for (var i = 0; i < values.Length; i++) values[i] = i + 1;
            return values;
        }

        static PlayerCommand Parse(string token)
        {
            switch (token.Trim())
            {
                case "N": return PlayerCommand.Move(Direction.North);
                case "E": return PlayerCommand.Move(Direction.East);
                case "S": return PlayerCommand.Move(Direction.South);
                case "W": return PlayerCommand.Move(Direction.West);
                case "I": return PlayerCommand.Interact();
                default: throw new AssertionException($"Unknown solution token '{token}'.");
            }
        }
    }
}
