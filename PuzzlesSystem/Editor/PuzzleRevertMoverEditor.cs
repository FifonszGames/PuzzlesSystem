using Gameplay.PuzzleRefactor.PuzzleMovers;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

namespace Gameplay.PuzzleRefactor.Editor
{
    [CustomEditor(typeof(PuzzleRevertMover))]
    public class PuzzleRevertMoverEditor : OdinEditor
    {
        #region Private Fields

        private PuzzleRevertMover revertMover;
        private Transform transform;
        private Color gizmosColor = Color.red;
        private Vector3 playerPosition;

        #endregion

        #region Constants

        private const float DiscHandleRadius = 0.5f;
        private const int LineThickness = 5;

        #endregion

        #region Unity Callbacks

        protected override void OnEnable()
        {
            revertMover = target as PuzzleRevertMover;
            transform = revertMover.transform;
        }

        #endregion

        #region Public Methods

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.Space(10f);
            GUILayout.Label("Editor Only", EditorStyles.boldLabel);
            gizmosColor = EditorGUILayout.ColorField("Gizmos Color", gizmosColor);
        }

        #endregion

        #region Private Methods

        private void OnSceneGUI()
        {
            if (ShouldPaint())
            {
                DrawDestinationGizmos();
            }
            else
            {
                playerPosition = Vector3.zero;
            }
        }

        private void DrawDestinationGizmos()
        {
            playerPosition = GetPlayerPosition();

            Handles.color = gizmosColor;
            Handles.DrawSolidDisc(playerPosition, Vector3.up, DiscHandleRadius);
            Handles.DrawLine(playerPosition, revertMover.Destinations[0], LineThickness);

            for (int i = 0; i < revertMover.Destinations.Count - 1; i++)
            {
                Vector3 current = revertMover.Destinations[i];
                Vector3 next = revertMover.Destinations[i + 1];

                if (i == 0)
                {
                    Handles.DrawSolidDisc(current, Vector3.up, DiscHandleRadius);
                }

                Handles.DrawLine(current, next, LineThickness);
                Handles.DrawSolidDisc(next, Vector3.up, DiscHandleRadius);
            }

            Handles.color = Color.white;
        }

        private Vector3 GetPlayerPosition()
        {
            return playerPosition == Vector3.zero ? transform.position : playerPosition;
        }

        private bool ShouldPaint()
        {
            return Application.isPlaying && revertMover != null && !revertMover.Destinations.IsNullOrEmpty() && revertMover.Destinations.Count > 0;
        }

        #endregion
    }
}
