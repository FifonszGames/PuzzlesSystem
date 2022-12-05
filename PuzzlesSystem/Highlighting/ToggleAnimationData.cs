using System;
using UnityEngine;

namespace Gameplay.PuzzleRefactor.Highlighting
{
    [Serializable]
    public struct ToggleAnimationData
    {
        #region Public Properties

        [field: SerializeField]
        public AnimationData OnData { get; private set; }
        [field: SerializeField]
        public AnimationData OffData { get; private set; }

        #endregion
    }
}
