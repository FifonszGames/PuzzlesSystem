using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.PuzzleRefactor.ScriptableObjects
{
	[CreateAssetMenu(fileName = nameof(RuntimePuzzleData), menuName = "ScriptableObjects/" + nameof(Gameplay) + "/" + nameof(PuzzleRefactor) + "/" + nameof(RuntimePuzzleData))]
    public class RuntimePuzzleData : ScriptableObject
    {
        #region Private Fields

        [ShowInInspector, ReadOnly]
        public PuzzleRef currentPuzzle;

        #endregion
    }
}
