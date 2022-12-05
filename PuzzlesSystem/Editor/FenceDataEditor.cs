using System.Collections.Generic;
using Extensions;
using Gameplay.PuzzleRefactor.Utils;
using Gameplay.PuzzleRefactor.Validators.FieldDataProviders;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEditor.Experimental.SceneManagement;
using UnityEngine;

namespace Gameplay.PuzzleRefactor.Editor
{
    [CustomEditor(typeof(FenceFieldData))]
    public class FenceDataEditor : OdinEditor
    {
        #region Private Fields

        private FenceFieldData self;
        private IEnumerable<Transform> bases;
        private static readonly Color FoundColor = Color.green;
        private static readonly Color NotFoundColor = Color.red;
        private static readonly float LineThickness = 3f;

        #endregion

        #region Unity Callbacks

        protected override void OnEnable()
        {
            base.OnEnable();
            self = target as FenceFieldData;

            if (self)
            {
                bases = self.transform.GetChildrenWithTag(AFieldUtils.FieldBaseTag);
            }
        }

        #endregion

        #region Private Methods

        private void OnSceneGUI()
        {
            if (EditorApplication.isPlaying || PrefabStageUtility.GetCurrentPrefabStage() == null)
            {
                return;
            }

            DrawFenceGizmos();
        }

        private void DrawFenceGizmos()
        {
            if (bases == null)
            {
                return;
            }

            foreach (Transform fieldBase in bases)
            {
                Vector3[] directions = fieldBase.GetChildLookupDirections();
                Vector3 aboveSelf = fieldBase.GetPositionAboveSelf();

                foreach (Vector3 direction in directions)
                {
                    if (fieldBase.gameObject.scene.GetPhysicsScene().Raycast(aboveSelf, direction, out RaycastHit hitInfo, AFieldUtils.FenceCheckLength, AFieldUtils.FenceLayer))
                    {
                        Handles.color = FoundColor;
                        Handles.DrawLine(aboveSelf, hitInfo.point, LineThickness);
                    }
                    else
                    {
                        Handles.color = NotFoundColor;
                        Handles.DrawLine(aboveSelf, aboveSelf + direction * AFieldUtils.FenceCheckLength, LineThickness);
                    }
                }
            }
        }

        #endregion
    }
}
