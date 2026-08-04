using System;

namespace ShadowTileEscape
{
    public sealed class LightSolver
    {
        int[] litCount = Array.Empty<int>();
        bool[] visited = Array.Empty<bool>();

        public int[] Solve(LevelState state)
        {
            var cellCount = state.width * state.height;
            if (litCount.Length != cellCount) litCount = new int[cellCount];
            else Array.Clear(litCount, 0, litCount.Length);
            if (visited.Length != cellCount * 4) visited = new bool[cellCount * 4];

            for (var sourceIndex = 0; sourceIndex < state.lights.Length; sourceIndex++)
            {
                var source = state.lights[sourceIndex];
                if (!source.active) continue;
                Array.Clear(visited, 0, visited.Length);
                Trace(state, source.position, source.direction, Math.Max(0, source.range));
            }
            for (var guardIndex = 0; guardIndex < state.guards.Length; guardIndex++)
            {
                var guard = state.guards[guardIndex];
                if (guard.lightRange <= 0) continue;
                Array.Clear(visited, 0, visited.Length);
                Trace(state, guard.position, guard.facing, guard.lightRange);
            }
            for (var movingIndex = 0; movingIndex < state.movingLights.Length; movingIndex++)
            {
                var moving = state.movingLights[movingIndex];
                if (!moving.active || moving.path.Length == 0) continue;
                Array.Clear(visited, 0, visited.Length);
                Trace(state, moving.Position, moving.Direction, Math.Max(0, moving.range));
            }
            return litCount;
        }

        void Trace(LevelState state, GridCoord origin, Direction direction, int range)
        {
            var current = origin;
            for (var distance = 0; distance < range; distance++)
            {
                var next = current + GridDirections.Offset(direction);
                if (!state.Contains(next) || IsOpaque(state, next)) return;

                var stateIndex = state.Index(next) * 4 + (int)direction;
                if (visited[stateIndex]) return;
                visited[stateIndex] = true;
                litCount[state.Index(next)]++;
                current = next;

                for (var i = 0; i < state.mirrors.Length; i++)
                {
                    if (state.mirrors[i].position != current) continue;
                    direction = Reflect(direction, state.mirrors[i].kind);
                    break;
                }
            }
        }

        static bool IsOpaque(LevelState state, GridCoord c)
        {
            if (state.HasCellFlag(c, CellFlags.Wall | CellFlags.Void)) return true;
            for (var i = 0; i < state.boxes.Length; i++) if (state.boxes[i] == c) return true;
            for (var i = 0; i < state.curtains.Length; i++)
                if (state.curtains[i].position == c && !state.curtains[i].open) return true;
            return false;
        }

        static Direction Reflect(Direction incoming, MirrorKind kind)
        {
            if (kind == MirrorKind.Slash)
            {
                switch (incoming)
                {
                    case Direction.North: return Direction.East;
                    case Direction.East: return Direction.North;
                    case Direction.South: return Direction.West;
                    default: return Direction.South;
                }
            }
            switch (incoming)
            {
                case Direction.North: return Direction.West;
                case Direction.West: return Direction.North;
                case Direction.South: return Direction.East;
                default: return Direction.South;
            }
        }
    }
}
