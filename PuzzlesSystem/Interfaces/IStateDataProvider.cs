using System;
using System.Collections.Generic;
using Gameplay.PuzzleRefactor.ScriptableObjects;

namespace Gameplay.PuzzleRefactor.Interfaces
{
    public interface IStateDataProvider
    {
        #region Public Properties

        public Dictionary<Type, FieldStateData> StateDatas { get; }

        #endregion

        #region Public Methods

        public FieldStateData GetData(Type fieldType);

        #endregion
    }
}
