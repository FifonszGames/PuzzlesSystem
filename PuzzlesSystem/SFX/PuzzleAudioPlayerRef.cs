using Extensions;
using Gameplay.Puzzles.Data;
using UnityEngine;

namespace Gameplay.PuzzleRefactor.SFX
{
    [RequireComponent(typeof(AudioPlayerRef))]
    public class PuzzleAudioPlayerRef : MonoBehaviour
    {
        #region Private Fields

        private PuzzleEventsHandler puzzleEventsHandler;
        private AudioPlayerRef audioPlayer;

        #endregion

        #region Constants

        private const string JingleClip = "Jingle";

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            GetReferences();
        }

        private void OnEnable()
        {
            AssignCallback();
        }

        private void OnDisable()
        {
            UnassignCallback();
        }

        #endregion

        #region Private Methods

        private void GetReferences()
        {
            puzzleEventsHandler = GetComponentInParent<PuzzleRef>().EventsHandler;
            audioPlayer = GetComponent<AudioPlayerRef>();
        }

        private void AssignCallback()
        {
            foreach ((PuzzleEventType key, string value) in PuzzleDataProvider.DefaultAudioData.AudioEvents)
            {
                puzzleEventsHandler.AddListener(key, () => Play(value));
            }

            puzzleEventsHandler.AddListener(PuzzleEventType.Completed, PlayJingle);
        }

        private void UnassignCallback()
        {
            foreach ((PuzzleEventType key, string value) in PuzzleDataProvider.DefaultAudioData.AudioEvents)
            {
                puzzleEventsHandler.RemoveListener(key);
            }

            puzzleEventsHandler.RemoveListener(PuzzleEventType.Completed, PlayJingle);
        }

        private void PlayJingle() => Play(JingleClip);
        private void Play(string path)
        {
            if (audioPlayer != null)
            {
                audioPlayer.PlayFromData(path);
            }
        }

        #endregion
    }
}
