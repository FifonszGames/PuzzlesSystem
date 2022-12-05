using System;
using System.Collections.Generic;
using System.Linq;
using Extensions;
using Gameplay.PuzzleRefactor.Fields;
using Gameplay.PuzzleRefactor.ScriptableObjects;
using UnityEngine;

namespace Gameplay.PuzzleRefactor.Utils
{
    public static class AFieldExtensions
    {
        #region Public Methods

        public static List<StandardFieldRef> GetAllNeighbours(this StandardFieldRef self)
        {
            List<StandardFieldRef> neighbours = new List<StandardFieldRef>();

            foreach (Transform baseChild in self.transform.GetChildrenWithTag(AFieldUtils.FieldBaseTag))
            {
                Vector3[] directions = baseChild.GetChildLookupDirections();

                foreach (Vector3 direction in directions)
                {
                    if (!self.HasNeighbour(baseChild.GetPositionAboveSelf(), direction, out StandardFieldRef neighbour))
                    {
                        continue;
                    }

                    neighbours.Add(neighbour);
                }
            }

            return neighbours.Distinct().ToList();
        }

        public static bool IsNeighbourOf(this StandardFieldRef self, StandardFieldRef other)
        {
            return self.Neighbours.Contains(other);
        }

        public static bool HasNeighbourBeneath(this StandardFieldRef self, Vector3 positionAboveDirection, out StandardFieldRef neighbour)
        {
            if (self.gameObject.scene.GetPhysicsScene().Raycast(positionAboveDirection, -self.transform.up, out RaycastHit hitInfo, AFieldUtils.ChildCheckLength, AFieldUtils.AllButBarrierLayer))
            {
                if (hitInfo.transform.TryGetComponentInParent(out neighbour))
                {
                    return self != neighbour;
                }
            }

            neighbour = null;

            return false;
        }

        public static Vector3 GetPositionAboveSelf(this Transform self)
        {
            return self.position + self.up * AFieldUtils.AboveSelfHeight;
        }

        public static IEnumerable<StandardFieldRef> WithoutNeighboursOf(this IEnumerable<StandardFieldRef> self, StandardFieldRef other, StandardFieldRef excludee = null)
        {
            List<StandardFieldRef> result = self.Where(field => other.Neighbours.All(field2 => field2 != field)).ToList();

            return excludee == null ? result : result.Without(excludee);
        }

        public static int ActivationCompletedCount(this IEnumerable<StandardFieldRef> self)
        {
            return self.Count(field => field is ActivationFieldRef && field.IsCompleted);
        }

        public static bool AllStandardAreCompleted(this IEnumerable<StandardFieldRef> self)
        {
            return self.Where(field => !(field is ActivationFieldRef)).All(field => field.IsCompleted);
        }

        public static FieldStateData GetData(this Dictionary<Type, FieldStateData> self, Type fieldType)
        {
            if (self == null)
            {
                return null;
            }

            FieldStateData derivedValue = null;

            foreach ((Type key, FieldStateData value) in self)
            {
                if (fieldType == key)
                {
                    return value;
                }

                if (key.IsAssignableFrom(fieldType))
                {
                    derivedValue = value;
                }
            }

            return derivedValue;
        }

        public static bool HasNeighbour(this StandardFieldRef self, Vector3 positionAboveSelf, Vector3 direction, out StandardFieldRef neighbour)
        {
            Vector3 positionAboveDirection = AFieldUtils.GetPositionAboveDirection(direction, positionAboveSelf);

            return self.HasNeighbourBeneath(positionAboveDirection, out neighbour);
        }

        #endregion
    }
}
