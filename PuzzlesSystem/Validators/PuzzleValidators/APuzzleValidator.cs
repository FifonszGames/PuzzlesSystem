using Gameplay.PuzzleRefactor.Interfaces;
using UnityEngine;

namespace Gameplay.PuzzleRefactor.Validators.PuzzleValidators
{
    [RequireComponent(typeof(PuzzleRef))]
    public abstract class APuzzleValidator : MonoBehaviour, IPuzzleValidator
    {
        #region Private Fields

        protected PuzzleRef puzzle;

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            puzzle = GetComponent<PuzzleRef>();
        }

        #endregion

        #region Public Methods

        public abstract bool WasCorrect();

        #endregion
    }
}
