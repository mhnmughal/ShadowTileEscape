using System;
using System.Collections.Generic;
using UnityEngine;

namespace ShadowTileEscape
{
    public enum Direction { North, East, South, West }
    public enum CommandType { Move, Interact }
    public enum TurnOutcome { Invalid, Advanced, Failed, Completed }
    public enum MirrorKind { Slash, Backslash }

    [Flags]
    public enum CellFlags { None = 0, Wall = 1, Void = 2 }

    [Serializable]
    public struct GridCoord : IEquatable<GridCoord>
    {
        public int x;
        public int y;

        public GridCoord(int x, int y) { this.x = x; this.y = y; }
        public bool Equals(GridCoord other) => x == other.x && y == other.y;
        public override bool Equals(object obj) => obj is GridCoord other && Equals(other);
        public override int GetHashCode() => (x * 397) ^ y;
        public static bool operator ==(GridCoord a, GridCoord b) => a.Equals(b);
        public static bool operator !=(GridCoord a, GridCoord b) => !a.Equals(b);
        public static GridCoord operator +(GridCoord a, GridCoord b) => new GridCoord(a.x + b.x, a.y + b.y);
        public override string ToString() => $"({x},{y})";
    }

    [Serializable]
    public struct PlayerCommand
    {
        public CommandType type;
        public Direction direction;

        public static PlayerCommand Move(Direction direction) => new PlayerCommand { type = CommandType.Move, direction = direction };
        public static PlayerCommand Interact() => new PlayerCommand { type = CommandType.Interact };
    }

    [Serializable]
    public struct LightSourceState
    {
        public GridCoord position;
        public Direction direction;
        public int range;
        public bool active;
    }

    [Serializable]
    public struct MirrorState
    {
        public GridCoord position;
        public MirrorKind kind;
        public bool rotatable;
    }

    [Serializable]
    public struct CurtainState
    {
        public GridCoord position;
        public bool open;
    }

    [Serializable]
    public sealed class GuardState
    {
        public GridCoord position;
        public GridCoord[] patrol = Array.Empty<GridCoord>();
        public int patrolIndex;

        public GuardState Copy() => new GuardState
        {
            position = position,
            patrol = (GridCoord[])patrol.Clone(),
            patrolIndex = patrolIndex
        };
    }

    [Serializable]
    public sealed class LevelState
    {
        public int width;
        public int height;
        public CellFlags[] cells = Array.Empty<CellFlags>();
        public GridCoord player;
        public Direction playerFacing;
        public GridCoord exit;
        public int requiredShards;
        public LightSourceState[] lights = Array.Empty<LightSourceState>();
        public MirrorState[] mirrors = Array.Empty<MirrorState>();
        public GridCoord[] boxes = Array.Empty<GridCoord>();
        public CurtainState[] curtains = Array.Empty<CurtainState>();
        public GuardState[] guards = Array.Empty<GuardState>();
        public GridCoord[] shards = Array.Empty<GridCoord>();
        public bool[] collectedShards = Array.Empty<bool>();
        public int moveCount;
        public bool failed;
        public bool completed;

        public int Index(GridCoord c) => c.y * width + c.x;
        public bool Contains(GridCoord c) => c.x >= 0 && c.y >= 0 && c.x < width && c.y < height;
        public bool HasCellFlag(GridCoord c, CellFlags flag) => !Contains(c) || (cells[Index(c)] & flag) != 0;

        public LevelState Copy()
        {
            var copy = (LevelState)MemberwiseClone();
            copy.cells = (CellFlags[])cells.Clone();
            copy.lights = (LightSourceState[])lights.Clone();
            copy.mirrors = (MirrorState[])mirrors.Clone();
            copy.boxes = (GridCoord[])boxes.Clone();
            copy.curtains = (CurtainState[])curtains.Clone();
            copy.shards = (GridCoord[])shards.Clone();
            copy.collectedShards = (bool[])collectedShards.Clone();
            copy.guards = new GuardState[guards.Length];
            for (var i = 0; i < guards.Length; i++) copy.guards[i] = guards[i].Copy();
            return copy;
        }

        public int ShardsCollected
        {
            get
            {
                var count = 0;
                for (var i = 0; i < collectedShards.Length; i++) if (collectedShards[i]) count++;
                return count;
            }
        }
    }

    public static class GridDirections
    {
        public static GridCoord Offset(Direction direction)
        {
            switch (direction)
            {
                case Direction.North: return new GridCoord(0, 1);
                case Direction.East: return new GridCoord(1, 0);
                case Direction.South: return new GridCoord(0, -1);
                default: return new GridCoord(-1, 0);
            }
        }
    }
}
