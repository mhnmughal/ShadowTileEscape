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
            if (IsPlayerUnsafe(next) || HasGuardAt(next, next.player))
            {
                next.failed = true;
                State = next;
                return new TurnResult(TurnOutcome.Failed, "A guardian found Noor.");
            }

            CollectShards(next);
            if (next.player == next.exit && next.ShardsCollected >= next.requiredShards)
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
                item.direction = (Direction)(((int)item.direction + 1) % 4);
                state.lights[i] = item;
                return true;
            }
            for (var i = 0; i < state.mirrors.Length; i++)
            {
                if (state.mirrors[i].position != target || !state.mirrors[i].rotatable) continue;
                var item = state.mirrors[i];
                item.kind = item.kind == MirrorKind.Slash ? MirrorKind.Backslash : MirrorKind.Slash;
                state.mirrors[i] = item;
                return true;
            }
            for (var i = 0; i < state.curtains.Length; i++)
            {
                if (state.curtains[i].position != target) continue;
                var item = state.curtains[i];
                item.open = !item.open;
                state.curtains[i] = item;
                return true;
            }
            for (var i = 0; i < state.boxes.Length; i++)
            {
                if (state.boxes[i] != target) continue;
                var beyond = target + GridDirections.Offset(state.playerFacing);
                if (!IsWalkable(state, beyond)) return false;
                state.boxes[i] = beyond;
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
            for (var i = 0; i < count; i++)
            {
                if (!moves[i]) continue;
                for (var j = i + 1; j < count; j++)
                {
                    var contested = targets[i] == targets[j];
                    var swap = targets[i] == state.guards[j].position && targets[j] == state.guards[i].position;
                    if (contested || swap) { moves[i] = false; moves[j] = false; }
                }
            }
            for (var i = 0; i < count; i++)
            {
                if (!moves[i]) continue;
                var guard = state.guards[i];
                guard.position = targets[i];
                guard.patrolIndex = (guard.patrolIndex + 1) % guard.patrol.Length;
            }
        }

        static void CollectShards(LevelState state)
        {
            for (var i = 0; i < state.shards.Length; i++)
                if (!state.collectedShards[i] && state.shards[i] == state.player) state.collectedShards[i] = true;
        }

        void PushHistory(LevelState snapshot)
        {
            if (history.Count == historyLimit) history.RemoveAt(0);
            history.Add(snapshot);
        }
    }
}
