using Gameplay.Puzzles.Data;
using Sirenix.Utilities;

namespace Gameplay.PuzzleRefactor.PuzzleStates
{
    public class CompletedState : APuzzleState
    {
        #region Constructors

        public CompletedState(PuzzleRef stateMachine) : base(stateMachine)
        {
        }

        #endregion

        #region Public Methods

        public override void OnEnter()
        {
            stateMachine.Fields.ForEach(field => field.SwitchTo(field.CompletedState));
            stateMachine.EventsHandler.Invoke(PuzzleEventType.Completed);

            if (!stateMachine.IsLoading)
            {
                stateMachine.InvokeOnCompleted();
            }
        }

        #endregion

        #region Protected Methods

        protected override void AssignEvents()
        {
        }

        protected override void UnassignEvents()
        {
        }

        #endregion
    }
}
