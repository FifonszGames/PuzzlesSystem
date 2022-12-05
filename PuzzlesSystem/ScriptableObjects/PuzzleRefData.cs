using Gameplay.PuzzleRefactor.Highlighting;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.PuzzleRefactor.ScriptableObjects
{
	[CreateAssetMenu(fileName = nameof(PuzzleRefData), menuName = "ScriptableObjects/" + nameof(Gameplay) + "/" + nameof(PuzzleRefactor) + "/" + nameof(PuzzleRefData))]
    public class PuzzleRefData : ScriptableObject
    {
        #region Public Properties

        [field: SerializeField, FoldoutGroup("Highlighting")]
        public ToggleAnimationData InnerFieldAnimation { get; private set; }
        [field: SerializeField, FoldoutGroup("Highlighting")]
        public ToggleAnimationData OuterFieldAnimation { get; private set; }
        [field: SerializeField, FoldoutGroup("Highlighting")]
        public ToggleAnimationData FailProgressAnimation { get; private set; }
        [field: SerializeField, FoldoutGroup("Highlighting")]
        public ToggleAnimationData FailFadingAnimation { get; private set; }
        [field: SerializeField, FoldoutGroup("Highlighting")]
        public AnimationData BarrierAnimation { get; private set; }
        [field: SerializeField, FoldoutGroup("Directionals")]
        public AnimationData DirectionalRotation { get; private set; }

        #endregion
    }
}
