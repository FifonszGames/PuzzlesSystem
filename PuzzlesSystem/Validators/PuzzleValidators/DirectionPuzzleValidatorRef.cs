using System.Collections.Generic;
using Extensions;
using Gameplay.PuzzleRefactor.Other;
using Gameplay.PuzzleRefactor.Validators.FieldDataProviders;
using UnityEngine;

namespace Gameplay.PuzzleRefactor.Validators.PuzzleValidators
{
    [DisallowMultipleComponent]
    public class DirectionPuzzleValidatorRef : APuzzleValidator
    {
        #region Public Methods

        public override bool WasCorrect() => CheckStepsCompletion();

        #endregion

        #region Private Methods

        private bool CheckStepsCompletion()
        {
            IReadOnlyList<PuzzleStep> steps = puzzle.PuzzleSteps;

            for (int i = 1; i < steps.Count - 1; i++)
            {
                if (!IsStepValid(steps[i], steps, i))
                {
                    return false;
                }
            }

            return true;
        }

        private bool IsStepValid(PuzzleStep currentStep, IReadOnlyList<PuzzleStep> steps, int currentIndex)
        {
            if (!currentStep.WasCompletion)
            {
                return true;
            }

            if (!currentStep.ToField.TryGetComponent(out DirectionFieldData data))
            {
                return true;
            }

            if (!FindNextCompletionStep(steps, currentIndex + 1, out PuzzleStep nextCompletedStep))
            {
                return true;
            }

            return PlayerLeftCorrectly(currentStep.ToBase, nextCompletedStep.ToBase, data.RequiredExitDirection);
        }

        private bool PlayerLeftCorrectly(Transform fromBase, Transform toBase, Vector3 requiredExitDirection)
        {
            Vector3 exitDirection = fromBase.position.DirectionToSkipY(toBase.position);

            return exitDirection.IsFacing(requiredExitDirection);
        }

        private bool FindNextCompletionStep(IReadOnlyList<PuzzleStep> steps, int startIndex, out PuzzleStep nextCompletedStep)
        {
            nextCompletedStep = null;

            for (int j = startIndex; j < steps.Count - 1; j++)
            {
                if (steps[j].WasCompletion)
                {
                    nextCompletedStep = steps[j];

                    return true;
                }
            }

            return false;
        }

        #endregion
    }
}
