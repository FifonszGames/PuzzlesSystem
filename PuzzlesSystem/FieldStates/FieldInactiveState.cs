using Extensions;
using Gameplay.PuzzleRefactor.Fields;
using Gameplay.PuzzleRefactor.Utils;
using Gameplay.Puzzles.Data;

namespace Gameplay.PuzzleRefactor.FieldStates
{
    public class FieldInactiveState : AFieldState
    {
        #region Private Fields

        private PuzzleRef puzzle;

        #endregion

        #region Constructors

        public FieldInactiveState(StandardFieldRef stateMachine, PuzzleRef puzzle) : base(stateMachine, puzzle.EventsHandler)
        {
            this.puzzle = puzzle;
        }

        #endregion

        #region Protected Methods

        protected override void AssignEvents()
        {
            eventsHandler.AddListener(PuzzleEventType.PuzzleStarted, PuzzleStarted);
            eventsHandler.AddListener(PuzzleEventType.CompletedIncorrectly, PuzzleCompletedIncorrectly);
        }

        protected override void UnassignEvents()
        {
            eventsHandler.RemoveListener(PuzzleEventType.PuzzleStarted, PuzzleStarted);
            eventsHandler.RemoveListener(PuzzleEventType.CompletedIncorrectly, PuzzleCompletedIncorrectly);
        }

        #endregion

        #region Private Methods

        private void PuzzleStarted()
        {
            StandardFieldRef firstCompleted = puzzle.CompletedFields.First();
            AFieldState nextState = stateMachine.IsNeighbourOf(firstCompleted) ? stateMachine.WalkableState : stateMachine.BlockedState;
            stateMachine.SwitchTo(nextState);
        }

        #endregion
    }
}
