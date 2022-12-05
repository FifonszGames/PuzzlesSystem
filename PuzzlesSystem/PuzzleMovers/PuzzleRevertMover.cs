using System.Collections;
using System.Collections.Generic;
using Extensions;
using Gameplay.PuzzleRefactor.Fields;
using Gameplay.PuzzleRefactor.Utils;
using UnityEngine;

namespace Gameplay.PuzzleRefactor.PuzzleMovers
{
    [DisallowMultipleComponent]
    public class PuzzleRevertMover : APuzzleMover
    {
        #region Serialized Fields

        [SerializeField, Range(0.01f, 2f)]
        private float defaultDuration = 0.5f;
        [SerializeField, Min(1)]
        private int maxSequenceSize = 5;
        [SerializeField]
        private AnimationCurve movementCurve;
        [SerializeField]
        private AnimationCurve durationCurve;

        #endregion

        #region Private Fields

        private StandardFieldRef lastField;
        private float currentMoveDuration;
        private float currentTimer;
        private int currentRevertAmount;

        #endregion

        #region Public Properties

        public List<Vector3> Destinations { get; } = new List<Vector3>();

        #endregion

        #region Protected Methods

        protected override void PerformMovement(PuzzleRef puzzleRef)
        {
            Destinations.Add(GetPosition(puzzleRef));
            ManageTimers();

            if (Destinations.Count == 1)
            {
                StartCoroutine(MoveCoroutine(puzzleRef));
            }
        }

        protected override bool CanPerformMovement(PuzzleRef puzzleRef) => base.CanPerformMovement(puzzleRef) && CanPerformRevert(puzzleRef);

        #endregion

        #region Private Methods

        private void ManageTimers()
        {
            float completionPercentage = currentTimer / currentMoveDuration;
            currentMoveDuration = GetDuration();
            currentTimer = Mathf.Lerp(0, currentMoveDuration, completionPercentage);
        }

        private bool CanPerformRevert(PuzzleRef puzzleRef)
        {
            return puzzleRef.PlayerIsOnField && Destinations.Count <= puzzleRef.CompletedFieldsCount && Destinations.Count < maxSequenceSize;
        }

        private float GetDuration()
        {
            return Destinations.Count > 1 ? defaultDuration * durationCurve.Evaluate((float) Destinations.Count / maxSequenceSize) : defaultDuration;
        }

        private IEnumerator MoveCoroutine(PuzzleRef puzzleRef)
        {
            MovementToggled(true);

            int currentDestinationIndex = 0;

            while (currentDestinationIndex < Destinations.Count)
            {
                Vector3 endPosition = Destinations[currentDestinationIndex];
                Vector3 currentPosition = playerTransform.position;
                currentTimer = 0;

                while (currentTimer <= currentMoveDuration)
                {
                    playerTransform.position = Vector3.Lerp(currentPosition, endPosition, movementCurve.Evaluate(currentTimer / currentMoveDuration));
                    currentTimer += Time.deltaTime;

                    yield return null;
                }

                playerTransform.position = endPosition;
                currentDestinationIndex++;
                puzzleRef.RevertMove();
            }

            lastField = null;
            Destinations.Clear();

            MovementToggled(false);
        }

        private Vector3 GetPosition(PuzzleRef puzzleRef)
        {
            float playerY = playerTransform.transform.position.y;
            int completedFieldsCount = puzzleRef.CompletedFieldsCount;

            if (completedFieldsCount == 1)
            {
                return puzzleRef.CurrentResetPoint.WithY(playerY);
            }

            if (lastField == null)
            {
                lastField = puzzleRef.CompletedFields[completedFieldsCount - 2];

                return playerTransform.FindClosest(lastField.transform.GetChildrenWithTag(AFieldUtils.FieldBaseTag)).WithY(playerY);
            }

            int previouslyCompetedIndex = puzzleRef.CompletedFields.IndexOf(lastField) - 1;

            if (previouslyCompetedIndex < 0)
            {
                return puzzleRef.CurrentResetPoint.WithY(playerY);
            }

            StandardFieldRef nextField = puzzleRef.CompletedFields[previouslyCompetedIndex];
            Vector3 position = Destinations[Destinations.Count - 1].GetClosestPosition(nextField.transform.GetChildrenWithTag(AFieldUtils.FieldBaseTag)).WithY(playerY);
            lastField = nextField;

            return position;
        }

        #endregion
    }
}
