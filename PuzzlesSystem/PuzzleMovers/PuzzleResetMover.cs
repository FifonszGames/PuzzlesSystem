using DG.Tweening;
using Extensions;
using UnityEngine;

namespace Gameplay.PuzzleRefactor.PuzzleMovers
{
    [DisallowMultipleComponent]
    public class PuzzleResetMover : APuzzleMover
    {
        #region Serialized Fields

        [SerializeField, Range(0.01f, 1f)]
        private float resetTime = 0.5f;
        [SerializeField]
        private Ease easeType = Ease.Linear;
        [SerializeField]
        private bool useTimePerDistance;

        #endregion

        #region Constants

        private const float DistancePerPuzzle = 4f;

        #endregion

        #region Protected Methods

        protected override void PerformMovement(PuzzleRef puzzleRef)
        {
            if (!puzzleRef.PlayerIsOnField)
            {
                puzzleRef.DeactivatePuzzle();

                return;
            }

            MovementToggled(true);
            Vector3 endPosition = GetResetPosition(puzzleRef);
            float time = useTimePerDistance ? GetPerDistanceTime(endPosition) : resetTime;

            playerTransform.DOMove(endPosition, time).SetEase(easeType).OnComplete(() => MovementFinish(puzzleRef));
        }

        protected override bool CanPerformMovement(PuzzleRef puzzleRef) => base.CanPerformMovement(puzzleRef) && !isActive;

        #endregion

        #region Private Methods

        private float GetPerDistanceTime(Vector3 endPosition)
        {
            float distanceToEnd = playerTransform.position.DistanceTo(endPosition);
            float rest = distanceToEnd % DistancePerPuzzle;
            int times = (int) Mathf.Floor(distanceToEnd / DistancePerPuzzle);
            float time = times * resetTime + Mathf.Lerp(0, resetTime, rest / DistancePerPuzzle);

            return time;
        }

        private void MovementFinish(PuzzleRef puzzleRef)
        {
            puzzleRef.DeactivatePuzzle();
            MovementToggled(false);
        }

        private Vector3 GetResetPosition(PuzzleRef puzzleRef)
        {
            return puzzleRef.CurrentResetPoint.WithY(playerTransform.position.y);
        }

        #endregion
    }
}
