using System;
using Cinemachine;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.PuzzleRefactor.Other
{
    [RequireComponent(typeof(Collider))]
    public class BaseTriggerDetector : MonoBehaviour
    {
        #region Events

        public event Action<Transform> OnObjectEntered;
        public event Action<Transform> OnObjectExited;

        #endregion

        #region Serialized Fields

        [SerializeField, TagField]
        private string tagToDetect = "Player";
        [SerializeField, Required]
        private Transform fieldBase;

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

            OnObjectEntered?.Invoke(fieldBase);
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag(tagToDetect))
            {
                return;
            }

            OnObjectExited?.Invoke(fieldBase);
        }

        #endregion
    }
}
