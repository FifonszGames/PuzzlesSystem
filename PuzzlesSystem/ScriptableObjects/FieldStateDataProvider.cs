using System;
using System.Collections.Generic;
using Gameplay.PuzzleRefactor.Interfaces;
using Gameplay.PuzzleRefactor.Utils;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace Gameplay.PuzzleRefactor.ScriptableObjects
{
	[CreateAssetMenu(fileName = nameof(FieldStateDataProvider), menuName = "ScriptableObjects/" + nameof(Gameplay) + "/" + nameof(PuzzleRefactor) + "/" + nameof(FieldStateDataProvider))]
    public sealed class FieldStateDataProvider : SerializedScriptableObject, IStateDataProvider
    {
        #region Serialized Fields

        [SerializeField]
        private FieldStateData defaultData;

        #endregion

        #region Public Properties

        [field: OdinSerialize, DictionaryDrawerSettings(KeyLabel = "State Type", ValueLabel = "State Data"), ReadOnly, Space(5f)]
        public Dictionary<Type, FieldStateData> StateDatas { get; private set; } = new Dictionary<Type, FieldStateData>();

        #endregion

        #region Public Methods

        public FieldStateData GetData(Type fieldType)
        {
            FieldStateData stateData = StateDatas.GetData(fieldType);

            if (!stateData)
            {
                Debug.LogWarning($"Couldn't find state data for {fieldType.Name}. Check if {name} has appropriate data, returning default", this);

                return defaultData;
            }

            return stateData;
        }

        #endregion
    }
}
