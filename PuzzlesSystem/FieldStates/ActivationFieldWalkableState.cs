using System.Collections.Generic;
using Extensions;
using Gameplay.PuzzleRefactor.Fields;
using Gameplay.PuzzleRefactor.Utils;
using Gameplay.Puzzles.Data;
using UnityEngine;

namespace Gameplay.PuzzleRefactor.FieldStates
{
    public class ActivationFieldWalkableState : FieldWalkableState
    {
        #region Private Fields

        private bool invoked;

        #endregion

        #region Constructors

        public ActivationFieldWalkableState(StandardFieldRef stateMachine, PuzzleRef puzzle) : base(stateMachine, puzzle)
        {
        }

        #endregion

        #region Public Methods

        public override void OnEnter()
        {
            base.OnEnter();
            invoked = false;
        }

        public override void OnExit()
        {
            base.OnExit();

            if (invoked && puzzle.IsCompletedIncorrectly)
            {
                stateMachine.InvokeCompletionChanged(false);
            }
        }

        #endregion

        #region Protected Methods

        protected override void AssignEvents()
        {
            base.AssignEvents();
            eventsHandler.AddListener(PuzzleEventType.PuzzleStarted, PuzzleStarted);
        }

        protected override void UnassignEvents()
        {
            base.UnassignEvents();
            eventsHandler.RemoveListener(PuzzleEventType.PuzzleStarted, PuzzleStarted);
        }

        protected override void OnPlayerEntered(StandardFieldRef fieldRef, Transform fieldBase)
        {
            if (puzzle.ActivationFieldsCount < PuzzleRef.ActivationFieldsRequiredAmount)
            {
                base.OnPlayerEntered(fieldRef, fieldBase);

                return;
            }

            IReadOnlyCollection<StandardFieldRef> puzzleFields = puzzle.Fields;
            int count = puzzleFields.ActivationCompletedCount() + 1;

            if (count < PuzzleRef.ActivationFieldsRequiredAmount)
            {
                base.OnPlayerEntered(fieldRef, fieldBase);
            }
            else
            {
                invoked = true;
                stateMachine.InvokeCompletionChanged(true);
            }
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
