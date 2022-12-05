using System;
using Cinemachine;
using SavingSystem;
using UnityEngine;

namespace Gameplay.PuzzleRefactor.Other
{
    public class TagTriggerDetector : MonoBehaviour, ISave
    {
        #region Events

        public event Action OnObjectEntered;
        public event Action OnObjectExited;

        #endregion

        #region Serialized Fields

        [SerializeField, TagField]
        private string tagToDetect = "Player";
        
        public string ListName => SavingSystemUtils.PuzzleIdList;

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            GetComponent<Collider>().isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag(tagToDetect))
            {
                return;
            }

            OnObjectEntered?.Invoke();
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag(tagToDetect))
            {
                return;
            }

            OnObjectExited?.Invoke();
        }

        #endregion
    }
}
