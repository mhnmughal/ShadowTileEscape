using System;
using UnityEngine;

namespace ShadowTileEscape
{
    [CreateAssetMenu(menuName = "Shadow Tile Escape/Level Definition")]
    public sealed class LevelDefinition : ScriptableObject
    {
        public int levelNumber = 1;
        public string displayName = "Silent Steps";
        public int width = 7;
        public int height = 5;
        public int par = 8;
        public int requiredShards;
        public CellFlags[] cells = Array.Empty<CellFlags>();
        public GridCoord playerStart;
        public Direction playerFacing = Direction.East;
        public GridCoord exit;
        public LightSourceState[] lights = Array.Empty<LightSourceState>();
        public MirrorState[] mirrors = Array.Empty<MirrorState>();
        public GridCoord[] boxes = Array.Empty<GridCoord>();
        public CurtainState[] curtains = Array.Empty<CurtainState>();
        public GuardState[] guards = Array.Empty<GuardState>();
        public GridCoord[] shards = Array.Empty<GridCoord>();

        public LevelState CreateState()
        {
            var state = new LevelState
            {
                width = width,
                height = height,
                cells = cells.Length == width * height ? (CellFlags[])cells.Clone() : new CellFlags[width * height],
                player = playerStart,
                playerFacing = playerFacing,
                exit = exit,
                requiredShards = requiredShards,
                lights = (LightSourceState[])lights.Clone(),
                mirrors = (MirrorState[])mirrors.Clone(),
                boxes = (GridCoord[])boxes.Clone(),
                curtains = (CurtainState[])curtains.Clone(),
                guards = new GuardState[guards.Length],
                shards = (GridCoord[])shards.Clone(),
                collectedShards = new bool[shards.Length]
            };
            for (var i = 0; i < guards.Length; i++) state.guards[i] = guards[i].Copy();
            return state;
        }
    }
}
