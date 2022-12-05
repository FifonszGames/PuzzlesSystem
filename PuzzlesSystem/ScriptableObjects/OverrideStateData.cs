using System;
using System.Collections.Generic;
using Gameplay.PuzzleRefactor.Interfaces;
using Gameplay.PuzzleRefactor.Utils;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace Gameplay.PuzzleRefactor.ScriptableObjects
{
	[CreateAssetMenu(fileName = nameof(OverrideStateData), menuName = "ScriptableObjects/" + nameof(Gameplay) + "/" + nameof(PuzzleRefactor) + "/" + nameof(OverrideStateData))]
    public class OverrideStateData : SerializedScriptableObject, IStateDataProvider
    {
        #region Private Fields

        [OdinSerialize, DictionaryDrawerSettings(KeyLabel = "Override Type", ValueLabel = "Override Data")]
        private Dictionary<Type, FieldStateData> overrides = new Dictionary<Type, FieldStateData>();

        #endregion

        #region Public Properties

        public Dictionary<Type, FieldStateData> StateDatas => overrides;

        #endregion

        #region Public Methods

        public FieldStateData GetData(Type fieldType)
        {
            FieldStateData overriden = overrides.GetData(fieldType);

            return overriden ? overriden : PuzzleDataProvider.DefaultStateData.GetData(fieldType);
        }

        #endregion
    }
}
