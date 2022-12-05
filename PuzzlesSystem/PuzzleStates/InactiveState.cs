using System.Collections.Generic;
using System.Linq;
using Extensions;
using Gameplay.PuzzleRefactor.Fields;
using Gameplay.PuzzleRefactor.Other;
using Sirenix.Utilities;
using UnityEngine;

namespace Gameplay.PuzzleRefactor.PuzzleStates
{
    public class InactiveState : APuzzleState
    {
        private List<ActivationFieldRef> ActivationFields => stateMachine.Fields.FilterCast<ActivationFieldRef>().ToList();

        #region Constructors

        public InactiveState(PuzzleRef stateMachine) : base(stateMachine)
        {
        }

        #endregion

        #region Public Methods

        public override void OnEnter()
        {
            if (stateMachine.CompletedFields == null || stateMachine.CompletedFields.Count > 0)
            {
                stateMachine.RefreshCompletionList();
            }

            if (stateMachine.PuzzleSteps == null || stateMachine.PuzzleSteps.Count > 0)
            {
                stateMachine.RefreshStepsList();
            }

            stateMachine.Fields.ForEach(field => field.SwitchTo(field.InactiveState));
            base.OnEnter();
        }

        #endregion

        #region Protected Methods

        protected override void AssignEvents()
        {
            ActivationFields.ForEach(activationField => activationField.OnCompletionChanged += ActivationFieldCompletionChanged);
        }

        protected override void UnassignEvents()
        {
            ActivationFields.ForEach(activationField => activationField.OnCompletionChanged -= ActivationFieldCompletionChanged);
        }

        #endregion

        #region Private Methods

        private void ActivationFieldCompletionChanged(StandardFieldRef field, Transform enteredBase)
        {
            if (stateMachine.IsLoading || !field.IsCompleted)
            {
                return;
            }

            stateMachine.AddCompleted(field);
            stateMachine.CurrentResetPoint = enteredBase.FindClosest(stateMachine.ResetPoints);
            stateMachine.AddStep(new PuzzleStep(stateMachine.CurrentField, field, enteredBase, true));
            stateMachine.SwitchTo(stateMachine.InProgressState);
        }

        #endregion
    }
}
