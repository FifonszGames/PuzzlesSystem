using System.Collections.Generic;
using System.Linq;
using Animation.Triggers;
using Gameplay.Puzzles.Data;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace Gameplay.PuzzleRefactor.ScriptableObjects
{
	[CreateAssetMenu(fileName = nameof(AudioDataHolder), menuName = "ScriptableObjects/" + nameof(Gameplay) + "/" + nameof(PuzzleRefactor) + "/" + nameof(AudioDataHolder))]
    public class AudioDataHolder : SerializedScriptableObject
    {
        #region Serialized Fields

        [SerializeField]
        private List<AudioPlayer.AudioData> audioDatas;

        #endregion

        #region Private Fields

        [OdinSerialize]
        private Dictionary<PuzzleEventType, string> audioEvents = new Dictionary<PuzzleEventType, string>();

        #endregion

        #region Public Properties

        public IReadOnlyDictionary<PuzzleEventType, string> AudioEvents => audioEvents;

        #endregion

        #region Public Methods

        public AudioPlayer.AudioData GetDataFromId(string id)
        {
            return audioDatas.FirstOrDefault(data => data.ID.Equals(id));
        }

        #endregion
    }
}
