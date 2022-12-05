using Gameplay.PuzzleRefactor.Fields;
using Gameplay.PuzzleRefactor.Utils;

namespace Gameplay.PuzzleRefactor.FieldStates
{
    public class FieldBlockedState : AFieldState
    {
        #region Private Fields

        private PuzzleRef puzzle;

        #endregion

        #region Constructors

        public FieldBlockedState(StandardFieldRef stateMachine, PuzzleRef puzzle) : base(stateMachine, puzzle.EventsHandler)
        {
            this.puzzle = puzzle;
        }

        #endregion

        #region Protected Methods

        protected override void AssignEvents()
        {
            base.AssignEvents();
            puzzle.OnCompletedFieldAdded += CompletedFieldsOnItemAdded;
        }

        protected override void UnassignEvents()
        {
            base.UnassignEvents();
            puzzle.OnCompletedFieldAdded -= CompletedFieldsOnItemAdded;
        }

        #endregion

        #region Private Methods

        private void CompletedFieldsOnItemAdded(StandardFieldRef item)
        {
            if (item == stateMachine || !stateMachine.IsNeighbourOf(item))
            {
                return;
            }

            stateMachine.SwitchTo(stateMachine.WalkableState);
        }

        #endregion
    }
}
