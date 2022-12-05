using Common.StateMachine.Abstract;

namespace Gameplay.PuzzleRefactor.PuzzleStates
{
    public abstract class APuzzleState : AState<PuzzleRef, APuzzleState>
    {
        #region Constructors

        protected APuzzleState(PuzzleRef stateMachine) : base(stateMachine)
        {
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

        protected abstract void AssignEvents();
        protected abstract void UnassignEvents();

        #endregion
    }
}
