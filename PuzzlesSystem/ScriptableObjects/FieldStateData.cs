using UnityEngine;

namespace Gameplay.PuzzleRefactor.ScriptableObjects
{
	[CreateAssetMenu(fileName = nameof(FieldStateData), menuName = "ScriptableObjects/" + nameof(Gameplay) + "/" + nameof(PuzzleRefactor) + "/" + nameof(FieldStateData))]
    public class FieldStateData : ScriptableObject
    {
        #region Public Properties

        [field: SerializeField]
        public bool HasActiveBarrier { get; private set; } = false;
        [field: SerializeField]
        public bool HighlightsInnerField { get; private set; } = false;
        [field: SerializeField]
        public bool HighlightsOuterField { get; private set; } = false;
        [field: SerializeField]
        public bool HighlightsFail { get; private set; } = false;

        #endregion
    }
}
