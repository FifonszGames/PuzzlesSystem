using Gameplay.PuzzleRefactor.Fields;
using Gameplay.PuzzleRefactor.Utils;
using UnityEngine;

namespace Gameplay.PuzzleRefactor.FieldStates
{
    public class FieldWalkableState : AFieldState
    {
        #region Private Fields

        protected readonly PuzzleRef puzzle;

        #endregion

        #region Constructors

        public FieldWalkableState(StandardFieldRef stateMachine, PuzzleRef puzzle) : base(stateMachine, puzzle.EventsHandler)
        {
            this.puzzle = puzzle;
        }

        #endregion

        #region Protected Methods

        protected override void AssignEvents()
        {
            base.AssignEvents();
            stateMachine.OnPlayerEntered += OnPlayerEntered;
            puzzle.OnFieldEntered += OtherFieldEntered;
        }

        protected override void UnassignEvents()
        {
            base.UnassignEvents();
            stateMachine.OnPlayerEntered -= OnPlayerEntered;
            puzzle.OnFieldEntered -= OtherFieldEntered;
        }

        protected virtual void OnPlayerEntered(StandardFieldRef fieldRef, Transform fieldBase)
        {
            stateMachine.SwitchTo(stateMachine.CompletedState);
        }

        #endregion

        #region Private Methods

        private void OtherFieldEntered(StandardFieldRef field, Transform fieldBase)
        {
            if (field == stateMachine || field.IsCompleted || !field.IsActive)
            {
                return;
            }

            bool isNeighbour = stateMachine.IsNeighbourOf(field);

            if (!isNeighbour)
            {
                stateMachine.SwitchTo(stateMachine.BlockedState);
            }
        }

        #endregion
    }
}
