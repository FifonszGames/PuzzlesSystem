using System;
using UnityEngine;

namespace Gameplay.PuzzleRefactor.Highlighting
{
    [Serializable]
    public struct AnimationData
    {
        #region Public Properties

        [field: SerializeField, Range(0.01f, 1f)]
        public float Duration { get; private set; }
        [field: SerializeField]
        public AnimationCurve Curve { get; private set; }

        #endregion

        #region Public Methods

        public bool IsNull()
        {
            return Duration <= 0 || Curve.length < 1;
        }

        #endregion
    }
}
