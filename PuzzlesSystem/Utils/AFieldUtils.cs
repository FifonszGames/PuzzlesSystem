using UnityEngine;

namespace Gameplay.PuzzleRefactor.Utils
{
    public static class AFieldUtils
    {
        #region Private Fields

        private static int fenceLayer = -1;
        private static int allButBarrierLayer = -1;

        #endregion

        #region Constants

        public const float ChildCheckLength = 4f;
        public const float FenceCheckLength = 3f;
        public const int AboveSelfHeight = 2;
        public const string FieldBaseTag = "FieldBase";
        public const string FieldDirectionSymbolTag = "FieldDirectionSymbol";
        public const float NeighbourBaseDistance = 4.5f;

        #endregion

        #region Public Properties

        public static int FenceLayer {
            get
            {
                if (fenceLayer == -1)
                {
                    fenceLayer = LayerMask.NameToLayer("FenceCheck");
                    fenceLayer = 1 << fenceLayer;
                }

                return fenceLayer;
            }
        }
        public static int AllButBarrierLayer {
            get
            {
                if (allButBarrierLayer == -1)
                {
                    allButBarrierLayer = LayerMask.NameToLayer("FieldBarrier");
                    allButBarrierLayer = 1 << allButBarrierLayer;
                    allButBarrierLayer = ~allButBarrierLayer;
                }

                return allButBarrierLayer;
            }
        }

        #endregion

        #region Public Methods

        public static Vector3 GetPositionAboveDirection(Vector3 direction, Vector3 positionAboveSelf)
        {
            return positionAboveSelf + direction * ChildCheckLength;
        }

        #endregion
    }
}
