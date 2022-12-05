using Gameplay.PuzzleRefactor.Fields;
using Gameplay.Puzzles.Data;
using Sirenix.Utilities;
using UnityEngine;

namespace Gameplay.PuzzleRefactor.PuzzleStates
{
    public class CompletedIncorrectlyState : APuzzleState

    {
        #region Constructors

        public CompletedIncorrectlyState(PuzzleRef stateMachine) : base(stateMachine)
        {
        }

        #endregion

        #region Public Methods

        public override void OnEnter()
        {
            stateMachine.EventsHandler.Invoke(PuzzleEventType.CompletedIncorrectly);
            base.OnEnter();
        }

        #endregion

        #region Protected Methods

        protected override void AssignEvents()
        {
            stateMachine.Fields.ForEach(field => field.OnPlayerExited += PlayerExitedField);
        }

        protected override void UnassignEvents()
        {
            stateMachine.Fields.ForEach(field => field.OnPlayerExited -= PlayerExitedField);
        }

        #endregion

        #region Private Methods

        private void PlayerExitedField(StandardFieldRef field, Transform fieldBase)
        {
            if (stateMachine.CurrentBase != fieldBase)
            {
                return;
            }

            stateMachine.DeactivatePuzzle();
        }

        #endregion
    }
}
