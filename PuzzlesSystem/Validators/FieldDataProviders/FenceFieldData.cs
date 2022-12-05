using System.Collections.Generic;
using System.Linq;
using Gameplay.PuzzleRefactor.Fields;
using Sirenix.Utilities;
using UnityEngine;

namespace Gameplay.PuzzleRefactor.Validators.FieldDataProviders
{
    [RequireComponent(typeof(StandardFieldRef)), DisallowMultipleComponent]
    public class FenceFieldData : MonoBehaviour
    {
        #region Public Properties

        [field: SerializeField]
        public List<StandardFieldRef> NeighboursWithFail { get; private set; }

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            if (NeighboursWithFail.IsNullOrEmpty())
            {
                return;
            }
            if (NeighboursWithFail.Any(field => field == null))
            {
                NeighboursWithFail.RemoveAll(field => field == null);
                Debug.LogWarning($"{name} has null fields in list, delete them!", this);
            }
        }

        private void OnValidate()
        {
            if (NeighboursWithFail.IsNullOrEmpty())
            {
                return;
            }

            StandardFieldRef self = GetComponent<StandardFieldRef>();

            if (NeighboursWithFail.Contains(self))
            {
                NeighboursWithFail.RemoveAll(neighbour => neighbour.Equals(self));
            }

            foreach (StandardFieldRef otherField in NeighboursWithFail)
            {
                if (otherField.TryGetComponent(out FenceFieldData otherValidator))
                {
                    if (!otherValidator.NeighboursWithFail.Contains(self))
                    {
                        otherValidator.NeighboursWithFail.Add(self);
                    }
                }
            }

            if (NeighboursWithFail.Count > 1)
            {
                NeighboursWithFail = NeighboursWithFail.Distinct().ToList();
            }
        }

        #endregion
    }
}
