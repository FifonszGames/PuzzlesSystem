using System.Collections.Generic;
using System.Linq;
using Animation.Triggers;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

namespace Gameplay.PuzzleRefactor.ScriptableObjects
{
	[CreateAssetMenu(fileName = nameof(AudioDataOverride), menuName = "ScriptableObjects/" + nameof(Gameplay) + "/" + nameof(PuzzleRefactor) + "/" + nameof(AudioDataOverride))]
    public class AudioDataOverride : SerializedScriptableObject
    {
        #region Serialized Fields

        [SerializeField]
        private List<AudioPlayer.AudioData> overrides = new List<AudioPlayer.AudioData>();

        #endregion

        #region Public Methods

        public AudioPlayer.AudioData GetDataFromId(string id)
        {
            if (overrides.IsNullOrEmpty())
            {
                return PuzzleDataProvider.DefaultAudioData.GetDataFromId(id);
            }

            AudioPlayer.AudioData data = overrides.FirstOrDefault(data => data.ID.Equals(id));

            return data ?? PuzzleDataProvider.DefaultAudioData.GetDataFromId(id);
        }

        #endregion
    }
}
