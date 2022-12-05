using System.Collections.Generic;
using Animation.Triggers;
using FMOD.Studio;
using FMODUnity;
using Gameplay.PuzzleRefactor.ScriptableObjects;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.PuzzleRefactor.SFX
{
    public class AudioPlayerRef : MonoBehaviour
    {
        #region Serialized Fields

        [SerializeField, Header("Overrides")]
        private bool overrideDefaults;
        [SerializeField, ShowIf("overrideDefaults")]
        private AudioDataOverride overrides;

        #endregion

        #region Private Fields

        private List<EventInstance> currentInstances = new List<EventInstance>();

        #endregion

        #region Unity Callbacks

        private void OnDestroy()
        {
            foreach (EventInstance eventInstance in currentInstances)
            {
                eventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            }

            currentInstances = null;
        }
        
        #endregion

        #region Public Methods

        public void PlayFromData(string id)
        {
            AudioPlayer.AudioData data = GetData(id);

            if (data != null)
            {
                EventInstance eventInstance = RuntimeManager.CreateInstance(data.Clip);
                eventInstance.set3DAttributes(RuntimeUtils.To3DAttributes(data.Position != null ? data.Position.gameObject : gameObject));
                eventInstance.start();
                eventInstance.release();
                currentInstances.Add(eventInstance);
            }
            else
            {
                Debug.LogError($"Could not found audio data with id: {id}");
            }
        }

        #endregion

        #region Private Methods

        private AudioPlayer.AudioData GetData(string id)
        {
            if (overrideDefaults && overrides != null)
            {
                return overrides.GetDataFromId(id);
            }

            return PuzzleDataProvider.DefaultAudioData.GetDataFromId(id);
        }

        #endregion
    }
}
