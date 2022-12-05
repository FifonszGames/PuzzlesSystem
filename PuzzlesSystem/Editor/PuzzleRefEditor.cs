using System;
using System.Collections.Generic;
using System.Linq;
using Effects;
using Extensions;
using Gameplay.PuzzleRefactor.Fields;
using Gameplay.PuzzleRefactor.Other;
using Gameplay.PuzzleRefactor.Utils;
using Gameplay.PuzzleRefactor.Validators.FieldDataProviders;
using Gameplay.PuzzleRefactor.Validators.PuzzleValidators;
using Gameplay.Puzzles.Data;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

namespace Gameplay.PuzzleRefactor.Editor
{
    [CustomEditor(typeof(PuzzleRef))]
    public class PuzzleRefEditor : OdinEditor
    {
        #region Private Fields

        private PuzzleRef self;
        private Vector3 centerPoint = Vector3.zero;
        private List<Connection> connections;
        private List<Connection> emptyConnections;
        private List<ToggleAction> toggleActions = new List<ToggleAction>();

        private static readonly Color FromColor = Color.cyan;
        private static readonly Color ToColor = Color.blue;
        private static readonly Dictionary<Type, Type> FieldDataToPuzzleValidators = new Dictionary<Type, Type>
        {
            { typeof(DirectionFieldData), typeof(DirectionPuzzleValidatorRef) },
            { typeof(FenceFieldData), typeof(FencePuzzleValidatorRef) },
            { typeof(MusicalFieldData), typeof(MusicalPuzzleValidatorRef) }
        };

        #endregion

        #region Constants

        private const string ResetPointsParentTag = "PuzzleResetPointsParent";
        private const string ResetPointsFieldName = "resetPoints";
        private const float DiscRadius = 0.2f;
        private const string EventHandlersPath = "Assets/Resources/Puzzle/Refactor/EventHandlers/";
        private const string EventHandlerName = "EventHandler";
        private const string AssetExtenstion = ".asset";

        #endregion

        #region Unity Callbacks

        protected override void OnEnable()
        {
            base.OnEnable();
            self = target as PuzzleRef;
            SetupEditorData();
            PopulateToggleActions();
        }

        #endregion

        #region Public Methods

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (!Application.isPlaying)
            {
                DrawEditorLayout();
            }
            else
            {
                DrawRuntimeLayout();
            }
        }

        #endregion

        #region Private Methods

        private void SetupEditorData()
        {
            List<StandardFieldRef> fields = GetCurrentFields();
            SetConnections(fields);
            SetCenterPoint(fields);
        }

        private void OnSceneGUI()
        {
            Handles.DrawWireCube(centerPoint, Vector3.one);
            DrawConnections();
        }

        private void DrawConnections()
        {
            if (!emptyConnections.IsNullOrEmpty())
            {
                emptyConnections.ForEach(connection => connection.Draw(Color.red));
            }

            if (connections.IsNullOrEmpty())
            {
                return;
            }

            if (connections.Any(connection => connection.From == null))
            {
                SetupEditorData();

                return;
            }

            for (int i = 0; i < connections.Count; i++)
            {
                float t = (float) i / (connections.Count - 1);
                connections[i].Draw(Color.Lerp(FromColor, ToColor, t));
            }
        }

        private void DrawEditorLayout()
        {
            GUILayout.Space(15f);

            DrawToggleAllButton();

            GUILayout.Space(5f);

            DrawToggles();

            GUILayout.Space(5f);

            DrawDoEverythingButton();
        }

        private void DrawRuntimeLayout()
        {
            GUIStyle labelStyle = GUI.skin.label;
            labelStyle.alignment = TextAnchor.MiddleCenter;
            EditorGUILayout.LabelField("Runtime Buttons", labelStyle);

            using (new EditorGUILayout.HorizontalScope(labelStyle))
            {
                if (GUILayout.Button("Refresh Highlight Data"))
                {
                    self.CallPrivateMethod("AssignHighlightData");
                }

                if (GUILayout.Button("Refresh State Data"))
                {
                    self.CallPrivateMethod("AssignStateData");
                }
            }
        }

        private void DrawDoEverythingButton()
        {
            if (GUILayout.Button("DO EVERYTHING ABOVE", GUILayout.Height(50f)))
            {
                if (!toggleActions.Any(action => action.IsToggled))
                {
                    Debug.LogWarning("None of the actions were selected, skipping initialization");
                }
                else if (EditorUtility.DisplayDialog("Initialize Puzzle", "This will override all existing references, are you sure?", "Yes", "No"))
                {
                    Initialize();
                    Debug.Log($"Congratulations, you have initialized {self.name}, now go check if it's working properly :D");
                }
            }
        }

        private void DrawToggles()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUILayout.VerticalScope())
                {
                    ShowToggles(0, toggleActions.Count / 2);
                }

                using (new EditorGUILayout.VerticalScope())
                {
                    ShowToggles(toggleActions.Count / 2, toggleActions.Count);
                }
            }
        }

        private void DrawToggleAllButton()
        {
            GUIStyle labelStyle = GUI.skin.box;

            if (GUILayout.Button("Toggle All", labelStyle))
            {
                bool toggle = !toggleActions[0].IsToggled;
                toggleActions.ForEach(toggleAction => toggleAction.IsToggled = toggle);
            }
        }

        private void ShowToggles(int from, int to)
        {
            for (int i = from; i < to; i++)
            {
                ToggleAction action = toggleActions[i];
                GUIContent content = new GUIContent(action.ToggleName, action.Tooltip);
                action.IsToggled = GUILayout.Toggle(action.IsToggled, content);
            }
        }

        private void PopulateToggleActions()
        {
            toggleActions.Add(new ToggleAction("Center Fields", "Move fields parent so that fields are in the center of a puzzle", () => PerformAction(CenterFields)));
            toggleActions.Add(new ToggleAction("Center Puzzle Components", "Move puzzle components so that they are in the center of a puzzle", CenterPuzzleComponents));
            toggleActions.Add(new ToggleAction("Remove Empty Neighbours", "Search for neighbours in each field and delete all empty data", RemoveNullNeighbours));
            toggleActions.Add(new ToggleAction("Set Reset Points", "Find all reset points", SetResetPoints));
            toggleActions.Add(new ToggleAction("Get Field References", "Find all fields", () => PerformAction(SetFields)));
            toggleActions.Add(new ToggleAction("Set Puzzle References in Fields", "Set reference to this puzzle in each field", () => PerformAction(SetPuzzleReferenceInFields)));
            toggleActions.Add(new ToggleAction("Set Fields Neighbours", "Set neighbours for each field", () => PerformAction(SetFieldsNeighbours)));
            toggleActions.Add(new ToggleAction("Set Detector Bases", "For each field look for their player detectors and set their closest bases ", () => PerformAction(SetDetectorBases)));
            toggleActions.Add(new ToggleAction("Add Puzzle Validators", "Go through each field and check if this puzzle needs any special validators", () => PerformAction(TryAddValidators)));
            toggleActions.Add(new ToggleAction("Initialize Fence Fields",
                                               "For each fence field check if there is a barrier in any direction, if there is add neighbour from that direction to that fields list of neighbours with fail",
                                               () => PerformAction(TrySetFenceFields)));
            toggleActions.Add(new ToggleAction("Create Event Handler", "Create new event handler for the puzzle", TryCreateEventHandler));
            toggleActions.Add(new ToggleAction("Adjust Event Handler Name", $"Set name of event handler to match PrefabName_{EventHandlerName}", AdjustEventHandlerName));
        }

        private void TryCreateEventHandler()
        {
            PuzzleEventsHandler eventsHandler = self.EventsHandler;

            string fileName = GetRequiredEventHandlerName();
            string path = EventHandlersPath + fileName + AssetExtenstion;

            if (eventsHandler == null)
            {
                CreateNewHandler(path);
            }
            else if (EditorUtility.DisplayDialog("Puzzle Event Handler", "This puzzle already has it's event handler, are you sure you want to create new one?", "Yes", "No"))
            {
                if (eventsHandler.name.ToLower().Equals(fileName))
                {
                    Debug.LogWarning("Puzzle and Event's Handler names are equal, did not create a new one");

                    return;
                }

                CreateNewHandler(path);
            }
        }

        private string GetRequiredEventHandlerName() => $"{self.name}_{EventHandlerName}";

        private void CreateNewHandler(string path)
        {
            PuzzleEventsHandler eventsHandler = CreateInstance<PuzzleEventsHandler>();
            AssetDatabase.CreateAsset(eventsHandler, path);
            SaveAndRefreshAssetDatabase();
            self.SetPropertyValue(eventsHandler, nameof(self.EventsHandler), false);
        }

        private void AdjustEventHandlerName()
        {
            PuzzleEventsHandler eventsHandler = self.EventsHandler;

            if (eventsHandler == null)
            {
                Debug.LogWarning("Couldn't adjust name of handler cuz it was empty");

                return;
            }

            string fileName = GetRequiredEventHandlerName();

            if (eventsHandler.name.ToLower().Equals(fileName))
            {
                Debug.LogWarning("Puzzle and Event's Handler name is already the same, not changing");

                return;
            }

            AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(eventsHandler), fileName);
            SaveAndRefreshAssetDatabase();
        }

        private void SaveAndRefreshAssetDatabase()
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private void CenterFields(List<StandardFieldRef> fields)
        {
            if (!GetParentOf<StandardFieldRef>(out Transform fieldsParent))
            {
                Debug.LogWarning("Couldn't find any parent that has fields, no centering done");

                return;
            }

            SetCenterPoint(fields);
            Vector3 centerToMid = self.transform.position - centerPoint;
            centerPoint += centerToMid;
            fieldsParent.position += centerToMid;
            SetConnections(fields);
        }

        private void CenterPuzzleComponents()
        {
            if (!GetParentOf<PuzzleCompletedVFX>(out Transform componentsParent))
            {
                Debug.LogWarning($"Did not find components parent.\nMake sure that there is a {nameof(PuzzleCompletedVFX)} on any child of object that holds puzzle components");
            }

            componentsParent.localPosition = Vector3.zero;

            foreach (Transform child in componentsParent)
            {
                child.localPosition = Vector3.zero;
            }
        }

        private void SetConnections(List<StandardFieldRef> fields)
        {
            connections = new List<Connection>();
            emptyConnections = new List<Connection>();

            foreach (StandardFieldRef field in fields)
            {
                if (field.Neighbours.IsNullOrEmpty())
                {
                    emptyConnections.Add(new Connection(field.transform));

                    continue;
                }

                foreach (StandardFieldRef neighbour in field.Neighbours.Where(neighbour => neighbour != null))
                {
                    Connection connection = new Connection(field.transform, neighbour.transform);

                    if (connections.All(con => con != connection))
                    {
                        connections.Add(connection);
                    }
                }
            }

            connections = connections.OrderBy(connection => connection.From.position.x).ThenBy(connection => connection.To.position.z).ToList();
        }

        private List<StandardFieldRef> GetCurrentFields()
        {
            if (GetParentOf<StandardFieldRef>(out Transform fieldsParent))
            {
                return fieldsParent.GetComponentsInChildren<StandardFieldRef>(true).ToList();
            }

            return new List<StandardFieldRef>();
        }

        private bool GetParentOf<T>(out Transform fieldsParent) where T : Component
        {
            fieldsParent = null;

            foreach (Transform child in self.transform)
            {
                if (child.TryGetComponentInChildren(out T component))
                {
                    fieldsParent = child;

                    return true;
                }
            }

            return false;
        }

        private void SetCenterPoint(List<StandardFieldRef> fields)
        {
            centerPoint = fields.Select(field => field.transform.position).ToList().GetCenter();
        }

        private void Initialize()
        {
            EditorUtility.SetDirty(self);

            toggleActions.Where(toggleAction => toggleAction.IsToggled).ForEach(toggleAction => toggleAction.Action.Invoke());
            AssetDatabase.SaveAssetIfDirty(self);
            serializedObject.ApplyModifiedProperties();
        }

        private void RemoveNullNeighbours()
        {
            foreach (StandardFieldRef field in GetCurrentFields())
            {
                if (field.Neighbours.Any(neighbour => neighbour == null))
                {
                    field.Neighbours.RemoveAll(neighbour => neighbour == null);
                }
            }
        }

        private void PerformAction(Action<List<StandardFieldRef>> action, List<StandardFieldRef> fields = null)
        {
            if (fields.IsNullOrEmpty())
            {
                fields = GetCurrentFields();
            }

            action.Invoke(fields);
        }

        private void TryAddValidators(List<StandardFieldRef> fields)
        {
            List<Type> validatorsNeeded = GetRequiredValidators(fields);

            Component[] selfComponents = self.GetComponents<Component>();

            foreach (Type validatorType in FieldDataToPuzzleValidators.Values)
            {
                bool hasValidator = selfComponents.Any(component => component.GetType().IsAssignableFrom(validatorType));

                if (validatorsNeeded.Contains(validatorType))
                {
                    if (hasValidator)
                    {
                        continue;
                    }

                    self.gameObject.AddComponent(validatorType);
                }
                else if (hasValidator)
                {
                    Component component = selfComponents.FirstOrDefault(component => component.GetType().IsAssignableFrom(validatorType));

                    if (!component)
                    {
                        continue;
                    }

                    DestroyImmediate(component);
                }
            }
        }

        private List<Type> GetRequiredValidators(List<StandardFieldRef> fields)
        {
            List<Type> validatorsNeeded = new List<Type>();

            foreach (StandardFieldRef field in fields)
            {
                Component[] components = field.GetComponents<Component>();

                foreach (Component component in components)
                {
                    foreach ((Type fieldDataType, Type validatorType) in FieldDataToPuzzleValidators)
                    {
                        if (component.GetType().IsAssignableFrom(fieldDataType))
                        {
                            validatorsNeeded.Add(validatorType);
                        }
                    }
                }
            }

            return validatorsNeeded.Distinct().ToList();
        }

        private void TrySetFenceFields(List<StandardFieldRef> fields)
        {
            foreach (StandardFieldRef field in fields)
            {
                if (!field.TryGetComponent(out FenceFieldData fenceFieldData))
                {
                    continue;
                }

                IEnumerable<Transform> bases = field.transform.GetChildrenWithTag(AFieldUtils.FieldBaseTag);
                List<StandardFieldRef> neighboursWithFail = new List<StandardFieldRef>();

                foreach (Transform fieldBase in bases)
                {
                    Vector3[] directions = fieldBase.GetChildLookupDirections();
                    Vector3 aboveSelf = fieldBase.GetPositionAboveSelf();

                    foreach (Vector3 direction in directions)
                    {
                        if (!fieldBase.gameObject.scene.GetPhysicsScene().Raycast(aboveSelf, direction, AFieldUtils.FenceCheckLength, AFieldUtils.FenceLayer))
                        {
                            continue;
                        }

                        if (!field.HasNeighbour(aboveSelf, direction, out StandardFieldRef neighbour))
                        {
                            continue;
                        }

                        if (!neighboursWithFail.Contains(neighbour))
                        {
                            neighboursWithFail.Add(neighbour);
                        }
                    }
                }

                fenceFieldData.SetPropertyValue(neighboursWithFail, nameof(fenceFieldData.NeighboursWithFail), false);
            }
        }

        private void SetDetectorBases(List<StandardFieldRef> fields)
        {
            foreach (StandardFieldRef field in fields)
            {
                BaseTriggerDetector[] fieldDetectors = field.transform.GetComponentsInChildren<BaseTriggerDetector>();
                List<Transform> fieldBases = field.transform.GetChildrenWithTag(AFieldUtils.FieldBaseTag).ToList();

                foreach (BaseTriggerDetector fieldDetector in fieldDetectors)
                {
                    Transform closestBase = fieldDetector.transform.position.GetClosestTransform(fieldBases);
                    fieldDetector.SetFieldValue(closestBase, "fieldBase");
                }
            }
        }

        private void SetPuzzleReferenceInFields(List<StandardFieldRef> fields)
        {
            foreach (StandardFieldRef field in fields)
            {
                field.SetFieldValue(self, "puzzle");
            }
        }

        private void SetResetPoints()
        {
            Transform resetPointsParent = self.GetChildWithTag(ResetPointsParentTag);

            if (resetPointsParent == null)
            {
                Debug.LogWarning($"Could not find Reset Points parent for {self.name}, make sure its tag is {ResetPointsParentTag}");

                return;
            }

            List<Transform> resetPoints = resetPointsParent.GetComponentsInChildren<Transform>().ToList();
            resetPoints.Remove(resetPointsParent);
            self.SetFieldValue(resetPoints, ResetPointsFieldName);
        }

        private void SetFieldsNeighbours(List<StandardFieldRef> fields)
        {
            foreach (StandardFieldRef field in fields)
            {
                SetFieldNeighbours(field);
            }
        }

        private void SetFieldNeighbours(StandardFieldRef field)
        {
            List<StandardFieldRef> neighbours = field.GetAllNeighbours();
            field.SetPropertyValue(neighbours, nameof(field.Neighbours), false);
        }

        private void SetFields(List<StandardFieldRef> fields)
        {
            self.SetFieldValue(fields, "fields");
        }

        #endregion

        private class ToggleAction
        {
            public readonly Action Action;
            public readonly string ToggleName;
            public readonly string Tooltip;

            public bool IsToggled;

            #region Constructors

            public ToggleAction(string toggleName, string tooltip, Action action)
            {
                ToggleName = toggleName;
                Tooltip = tooltip;
                Action = action;
                IsToggled = true;
            }

            #endregion
        }

        private class Connection
        {
            public readonly Transform From;
            public readonly Transform To;

            #region Constructors

            public Connection(Transform from, Transform to)
            {
                From = from;
                To = to;
            }

            public Connection(Transform from)
            {
                From = from;
                To = null;
            }

            #endregion

            #region Public Methods

            public void Draw(Color color = default)
            {
                Transform fromTransform = From.transform;
                Vector3 firstUp = fromTransform.up;
                Vector3 from = fromTransform.position + firstUp;

                if (!HasConnection())
                {
                    Handles.color = Color.red;
                    Handles.DrawSolidDisc(from, firstUp, DiscRadius);

                    return;
                }

                Transform secondTransform = To.transform;
                Vector3 secondUp = secondTransform.up;
                Vector3 to = secondTransform.position + secondUp;

                Handles.color = color;
                Handles.DrawLine(from, to);
                Handles.DrawSolidDisc(from, firstUp, DiscRadius);
                Handles.DrawSolidDisc(to, secondUp, DiscRadius);
            }

            public static bool operator ==(Connection a, Connection b)
            {
                return (a.From == b.From || a.From == b.To) && (a.To == b.From || a.To == b.To);
            }

            public static bool operator !=(Connection a, Connection b)
            {
                return !(a == b);
            }

            #endregion

            #region Private Methods

            private bool HasConnection() => To != null;

            #endregion
        }
    }
}
