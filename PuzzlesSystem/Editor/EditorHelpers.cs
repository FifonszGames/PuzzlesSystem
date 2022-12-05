using UnityEditor;
using UnityEngine;

namespace Gameplay.PuzzleRefactor.Editor
{
    public static class EditorHelpers
    {
        #region Public Methods

        public static void SaveAsset<T>(this T self) where T : Object
        {
            EditorUtility.SetDirty(self);
            AssetDatabase.SaveAssetIfDirty(self);
        }

        #endregion
    }
}
