using Common.StateMachine.Abstract;
using Gameplay.PuzzleRefactor.Fields;
using Gameplay.Puzzles.Data;

namespace Gameplay.PuzzleRefactor.FieldStates
{
    public abstract class AFieldState : AState<StandardFieldRef, AFieldState>
    {
        #region Private Fields

        protected PuzzleEventsHandler eventsHandler;

        #endregion

        #region Constructors

        protected AFieldState(StandardFieldRef stateMachine, PuzzleEventsHandler eventsHandler) : base(stateMachine)
        {
            this.eventsHandler = eventsHandler;
        }

        #endregion

        #region Public Methods

        public override void OnEnter()
        {
            AssignEvents();
        }

        public override void OnExit()
        {
            UnassignEvents();
        }

        #endregion

        #region Protected Methods

        protected virtual void AssignEvents()
        {
            eventsHandler.AddListener(PuzzleEventType.PuzzleReset, PuzzleReset);
            eventsHandler.AddListener(PuzzleEventType.CompletedIncorrectly, PuzzleCompletedIncorrectly);
        }

        protected virtual void UnassignEvents()
        {
            eventsHandler.RemoveListener(PuzzleEventType.PuzzleReset, PuzzleReset);
            eventsHandler.RemoveListener(PuzzleEventType.CompletedIncorrectly, PuzzleCompletedIncorrectly);
        }

        protected void PuzzleCompletedIncorrectly()
        {
            stateMachine.SwitchTo(stateMachine.CompletedIncorrectlyState);
        }

        #endregion

        #region Private Methods

        private void PuzzleReset()
        {
            stateMachine.SwitchTo(stateMachine.InactiveState);
        }

        #endregion
    }
}
