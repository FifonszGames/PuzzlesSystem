using Gameplay.PuzzleRefactor.Fields;
using Gameplay.PuzzleRefactor.Validators.FieldDataProviders;
using Gameplay.Puzzles.Musical.Data;
using UnityEngine;

namespace Gameplay.PuzzleRefactor.Validators.PuzzleValidators
{
    [DisallowMultipleComponent]
    public class MusicalPuzzleValidatorRef : APuzzleValidator
    {
        #region Serialized Fields

        [SerializeField]
        private MusicalSequence musicalSequence;

        #endregion

        #region Public Methods

        public override bool WasCorrect()
        {
            int signalIndex = 0;

            for (int i = 1; i < puzzle.CompletedFields.Count - 1; i++)
            {
                StandardFieldRef currentField = puzzle.CompletedFields[i];

                if (currentField.TryGetComponent(out MusicalFieldData signalProvider))
                {
                    if (musicalSequence.GetValidateSignal(signalIndex++) != signalProvider.SignalType)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        #endregion
    }
}
