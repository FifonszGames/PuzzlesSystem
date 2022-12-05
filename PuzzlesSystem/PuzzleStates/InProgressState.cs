using System.Linq;
using Gameplay.PuzzleRefactor.Fields;
using Gameplay.PuzzleRefactor.Interfaces;
using Gameplay.PuzzleRefactor.Other;
using Gameplay.PuzzleRefactor.Utils;
using Gameplay.Puzzles.Data;
using Sirenix.Utilities;
using UnityEngine;

namespace Gameplay.PuzzleRefactor.PuzzleStates
{
    public class InProgressState : APuzzleState
    {
        #region Private Fields

        private IPuzzleValidator[] puzzleValidators;
        private TagTriggerDetector playerDetector;
        private bool endImmediately;
        private bool wasMoveReverted;

        private IPuzzleValidator[] PuzzleValidators => puzzleValidators ??= stateMachine.GetComponents<IPuzzleValidator>();

        #endregion

        #region Constructors

        public InProgressState(PuzzleRef stateMachine, TagTriggerDetector playerDetector) : base(stateMachine)
        {
            if (!stateMachine)
            {
                return;
            }

            this.playerDetector = playerDetector;
        }

        #endregion

        #region Public Methods

        public override void OnEnter()
        {
            stateMachine.EventsHandler.Invoke(PuzzleEventType.PuzzleStarted);
            endImmediately = ShouldEndImmediately(out int activationFields);

            if (endImmediately)
            {
                stateMachine.SwitchTo(stateMachine.CompletedState);

                return;
            }

            base.OnEnter();
        }

        public override void OnExit()
        {
            if (endImmediately)
            {
                return;
            }

            base.OnExit();
        }

        #endregion

        #region Protected Methods

        protected override void AssignEvents()
        {
            stateMachine.Fields.ForEach(AssignFieldEvents);
            stateMachine.EventsHandler.AddListener(PuzzleEventType.MoveCanceled, MoveReverted);
            playerDetector.OnObjectExited += PlayerExitedResetRange;
        }

        protected override void UnassignEvents()
        {
            stateMachine.Fields.ForEach(UnassignFieldEvents);
            stateMachine.EventsHandler.RemoveListener(PuzzleEventType.MoveCanceled, MoveReverted);
            playerDetector.OnObjectExited -= PlayerExitedResetRange;
        }

        #endregion

        #region Private Methods

        private void AssignFieldEvents(StandardFieldRef field)
        {
            field.OnPlayerEntered += PlayerEnteredField;
            field.OnPlayerExited += PlayerExitedField;
            field.OnCompletionChanged += FieldCompletionChanged;
            field.OnRegisterStep += RegisterStep;
        }

        private void UnassignFieldEvents(StandardFieldRef field)
        {
            field.OnPlayerEntered -= PlayerEnteredField;
            field.OnPlayerExited -= PlayerExitedField;
            field.OnCompletionChanged -= FieldCompletionChanged;
            field.OnRegisterStep -= RegisterStep;
        }

        private void PlayerExitedResetRange() => stateMachine.DeactivatePuzzle();

        private void MoveReverted() => wasMoveReverted = true;

        private void FieldCompletionChanged(StandardFieldRef field, Transform enteredBase)
        {
            if (!field.IsCompleted)
            {
                stateMachine.CompletedFields.Remove(field);

                if (stateMachine.CompletedFields.Count < 1)
                {
                    stateMachine.SwitchTo(stateMachine.InactiveState);
                }
            }
            else
            {
                if (stateMachine.CompletedFields.Contains(field))
                {
                    return;
                }

                stateMachine.AddCompleted(field);

                if (field is ActivationFieldRef)
                {
                    CheckCompletion();
                }
            }
        }

        private void CheckCompletion()
        {
            bool isFinished = GetCompletionStatus(out int completedActivationFields);

            if (!isFinished)
            {
                if (completedActivationFields == PuzzleRef.ActivationFieldsRequiredAmount)
                {
                    stateMachine.SwitchTo(stateMachine.CompletedIncorrectlyState);
                }

                return;
            }

            if (PuzzleValidators.Any(validator => !validator.WasCorrect()))
            {
                stateMachine.SwitchTo(stateMachine.CompletedIncorrectlyState);

                return;
            }

            stateMachine.SwitchTo(stateMachine.CompletedState);
        }

        private bool GetCompletionStatus(out int completedActivationFields)
        {
            if (ShouldEndImmediately(out completedActivationFields))
            {
                return true;
            }

            bool enoughActivationFieldsCompleted = completedActivationFields >= PuzzleRef.ActivationFieldsRequiredAmount;

            if (enoughActivationFieldsCompleted && !stateMachine.Fields.AllStandardAreCompleted())
            {
                return false;
            }

            return enoughActivationFieldsCompleted;
        }

        private bool ShouldEndImmediately(out int completedActivationFields)
        {
            completedActivationFields = stateMachine.Fields.ActivationCompletedCount();

            return stateMachine.ActivationFieldsCount < PuzzleRef.ActivationFieldsRequiredAmount && completedActivationFields == stateMachine.ActivationFieldsCount;
        }

        private void PlayerExitedField(StandardFieldRef field, Transform fieldBase)
        {
            if (stateMachine.CurrentBase != fieldBase)
            {
                return;
            }

            stateMachine.SetCurrentField(null);
            stateMachine.SetCurrentBase(null);
        }

        private void PlayerEnteredField(StandardFieldRef field, Transform fieldBase)
        {
            stateMachine.SetCurrentField(field);
            stateMachine.SetCurrentBase(fieldBase);
            stateMachine.FieldEntered(field, fieldBase);
        }

        private void RegisterStep(StandardFieldRef field, Transform fieldBase)
        {
            if (wasMoveReverted)
            {
                wasMoveReverted = false;

                return;
            }

            stateMachine.AddStep(new PuzzleStep(stateMachine.CurrentField, field, fieldBase, !field.IsCompleted));
        }

        #endregion
    }
}
