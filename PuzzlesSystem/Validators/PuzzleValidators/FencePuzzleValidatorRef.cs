using System.Collections.Generic;
using Gameplay.PuzzleRefactor.Other;
using Gameplay.PuzzleRefactor.Validators.FieldDataProviders;
using Sirenix.Utilities;
using UnityEngine;

namespace Gameplay.PuzzleRefactor.Validators.PuzzleValidators
{
    [DisallowMultipleComponent]
    public class FencePuzzleValidatorRef : APuzzleValidator
    {
        #region Public Methods

        public override bool WasCorrect()
        {
            IReadOnlyList<PuzzleStep> steps = puzzle.PuzzleSteps;

            for (int i = 1; i < steps.Count - 1; i++)
            {
                PuzzleStep currentStep = steps[i];

                if (currentStep.ToField.TryGetComponent(out FenceFieldData data))
                {
                    if (WalkedThroughFence(data, currentStep))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        #endregion

        #region Private Methods

        private bool WalkedThroughFence(FenceFieldData data, PuzzleStep currentStep)
        {
            if (data.NeighboursWithFail.IsNullOrEmpty() || currentStep.FromField == null)
            {
                return false;
            }

            return data.NeighboursWithFail.Contains(currentStep.FromField);
        }

        #endregion
    }
}
