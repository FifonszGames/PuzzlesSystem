using System;
using System.Collections.Generic;
using System.Linq;
using Gameplay.PuzzleRefactor.FieldStates;
using Gameplay.PuzzleRefactor.Interfaces;
using Gameplay.PuzzleRefactor.ScriptableObjects;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Gameplay.PuzzleRefactor.Editor
{
    public abstract class AStateDataEditor<T> : OdinEditor where T : Object, IStateDataProvider
    {
        #region Private Fields

        private T self;

        private int currentIndex;
        private FieldStateData stateData;
        private Dictionary<string, Type> stateTypes;
        private string[] stateNames;

        #endregion

        #region Constants

        private const string PopupName = "Apply Changes To";
        private const string StateManagementLabel = "STATE MANAGEMENT";
        private const string DataToSet = "With Data";
        private const string OverrideExistingName = "Override Existing";
        private const string AddNewName = "Add New";

        #endregion

        #region Unity Callbacks

        protected override void OnEnable()
        {
            base.OnEnable();
            self = target as T;
            stateTypes = ReflectionExtensions.GetAllDerivedOf(typeof(AFieldState)).ToDictionary(t => t.Name, t => t);
            stateNames = stateTypes.Select(type => type.Key).ToArray();
        }

        #endregion

        #region Public Methods

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.Space(10f);
            EditorGUILayout.LabelField(StateManagementLabel, GUI.skin.label);
            EditorGUILayout.Space(5f);
            currentIndex = EditorGUILayout.Popup(PopupName, currentIndex, stateNames);

            bool alreadyExists = self.StateDatas?.ContainsKey(stateTypes[stateNames[currentIndex]]) ?? false;
            stateData = EditorGUILayout.ObjectField(DataToSet, stateData, typeof(FieldStateData), false) as FieldStateData;
            ManageAdding(alreadyExists);
        }

        #endregion

        #region Private Methods

        private void ManageAdding(bool alreadyExists)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUI.enabled = !alreadyExists && stateData != null;

                if (GUILayout.Button(AddNewName))
                {
                    AddNewEntry();
                }

                GUI.enabled = alreadyExists && stateData != null;

                if (GUILayout.Button(OverrideExistingName))
                {
                    ModifyExistingEntry();
                }

                GUI.enabled = true;
            }
        }

        private void AddNewEntry()
        {
            if (self.StateDatas == null)
            {
                self.SetPropertyValue(new Dictionary<Type, FieldStateData>(), nameof(self.StateDatas), false);
            }

            string key = stateNames[currentIndex];
            self.StateDatas.Add(stateTypes[key], stateData);
            self.SaveAsset();
        }

        private void ModifyExistingEntry()
        {
            string key = stateNames[currentIndex];
            self.StateDatas[stateTypes[key]] = stateData;
            self.SaveAsset();
        }

        #endregion
    }
}
