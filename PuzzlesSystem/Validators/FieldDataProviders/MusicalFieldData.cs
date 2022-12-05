using Gameplay.PuzzleRefactor.Fields;
using Gameplay.Puzzles.Musical.Data;
using UnityEngine;

namespace Gameplay.PuzzleRefactor.Validators.FieldDataProviders
{
    [RequireComponent(typeof(StandardFieldRef)), DisallowMultipleComponent]
    public class MusicalFieldData : MonoBehaviour
    {
        #region Public Properties

        [field: SerializeField]
        public SignalType SignalType { get; private set; }

        #endregion

        #region Unity Callbacks

        private void OnValidate()
        {
            if (SignalType == SignalType.Break)
            {
                SignalType = SignalType.One;
            }
        }

        #endregion
    }
}
