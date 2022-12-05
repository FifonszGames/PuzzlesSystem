using System;
using Gameplay.PuzzleRefactor.Fields;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.PuzzleRefactor.Other
{
    [Serializable]
    public class PuzzleStep
    {
        #region Public Properties

        [ShowInInspector, ReadOnly]
        public StandardFieldRef FromField { get; private set; }
        [ShowInInspector, ReadOnly]
        public Transform ToBase { get; private set; }
        [ShowInInspector, ReadOnly]
        public StandardFieldRef ToField { get; private set; }
        [ShowInInspector, ReadOnly]
        public bool WasCompletion { get; private set; }

        #endregion

        #region Constructors

        public PuzzleStep(StandardFieldRef fromField, StandardFieldRef toField, Transform toBase, bool wasCompletion)
        {
            FromField = fromField;
            ToField = toField;
            ToBase = toBase;
            WasCompletion = wasCompletion;
        }

        #endregion
    }
}
