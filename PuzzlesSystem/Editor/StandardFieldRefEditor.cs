using System.Collections.Generic;
using System.Linq;
using Extensions;
using Gameplay.PuzzleRefactor.Fields;
using Gameplay.PuzzleRefactor.Utils;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using UnityEditor;
using UnityEditor.Experimental.SceneManagement;
using UnityEngine;

namespace Gameplay.PuzzleRefactor.Editor
{
    [CustomEditor(typeof(StandardFieldRef), true)]
    public class StandardFieldRefEditor : OdinEditor
    {
        #region Private Fields

        private List<Transform> currentBaseChildren;
        private StandardFieldRef currentField;

        #endregion

        #region Constants

        private const float DiscRadius = 0.2f;
        private const float LineThickness = 3f;

        #endregion

        #region Unity Callbacks

        protected override void OnEnable()
        {
            base.OnEnable();
            currentField = target as StandardFieldRef;
            TryInitialize();
        }

        #endregion

        #region Private Methods

        private void OnSceneGUI()
        {
            if (EditorApplication.isPlaying || PrefabStageUtility.GetCurrentPrefabStage() == null)
            {
                return;
            }

            DrawNeighbourGizmos();
        }

        private void TryInitialize()
        {
            if (currentField == null)
            {
                return;
            }

            currentBaseChildren = currentField.transform.GetChildrenWithTag(AFieldUtils.FieldBaseTag).ToList();
        }

        private void DrawNeighbourGizmos()
        {
            if (currentBaseChildren.IsNullOrEmpty())
            {
                Debug.LogWarning($"{name},couldn't find any base children. Make sure that field bases have {AFieldUtils.FieldBaseTag} tag");

                return;
            }

            foreach (Transform currentBaseChild in currentBaseChildren)
            {
                Vector3[] directions = currentBaseChild.GetChildLookupDirections();

                foreach (Vector3 direction in directions)
                {
                    DrawHandles(currentBaseChild.GetPositionAboveSelf(), direction);
                }
            }
        }

        private void DrawHandles(Vector3 positionAboveSelf, Vector3 direction)
        {
            Vector3 positionAboveNeighbour = AFieldUtils.GetPositionAboveDirection(direction, positionAboveSelf);

            bool hasNeighbour = currentField.HasNeighbourBeneath(positionAboveNeighbour, out StandardFieldRef neighbour);
            bool drawHandles = neighbour == null || neighbour != currentField;

            if (!drawHandles)
            {
                return;
            }

            Handles.color = Color.cyan;
            Handles.DrawSolidDisc(positionAboveNeighbour, currentField.transform.up, DiscRadius);
            Handles.color = hasNeighbour ? Color.green : Color.red;
            Handles.DrawLine(positionAboveNeighbour, positionAboveNeighbour + -currentField.transform.up * AFieldUtils.ChildCheckLength, LineThickness);
        }

        #endregion
    }
}
