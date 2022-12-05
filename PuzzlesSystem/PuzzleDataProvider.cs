using Gameplay.PuzzleRefactor.Interfaces;
using Gameplay.PuzzleRefactor.ScriptableObjects;
using UnityEngine;

namespace Gameplay.PuzzleRefactor
{
    public static class PuzzleDataProvider
    {
        #region Private Fields

        private static PuzzleRefData puzzleRefData;
        private static IStateDataProvider fieldStateData;
        private static AudioDataHolder audioDataHolder;
        private static RuntimePuzzleData runtimePuzzleData;

        #endregion

        #region Public Properties

        public static PuzzleRefData DefaultAnimationData => puzzleRefData ??= GetDataFromResources<PuzzleRefData>("Puzzle/Refactor/PuzzleRefData/DefaultPuzzleData");
        public static IStateDataProvider DefaultStateData => fieldStateData ??= GetDataFromResources<FieldStateDataProvider>("Puzzle/Refactor/FieldStateDataProvider");
        public static AudioDataHolder DefaultAudioData => audioDataHolder ??= GetDataFromResources<AudioDataHolder>("Puzzle/Refactor/SFX/DefaultPuzzleAudioData");
        public static RuntimePuzzleData RuntimePuzzleData => runtimePuzzleData ??= GetDataFromResources<RuntimePuzzleData>("Puzzle/Refactor/CurrentPuzzleData");

        #endregion

        #region Private Methods

        private static T GetDataFromResources<T>(string path) where T : Object
        {
            T data = Resources.Load<T>(path);
            
            if (data == null)
            {
                Debug.LogError($"Couldn't find default data for {typeof(T).Name} in {path}");
            }

            return data;
        }

        #endregion
    }
}
