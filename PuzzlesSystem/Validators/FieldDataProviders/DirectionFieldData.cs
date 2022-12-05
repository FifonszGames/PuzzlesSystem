using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Extensions;
using Gameplay.PuzzleRefactor.Fields;
using Gameplay.PuzzleRefactor.FieldStates;
using Gameplay.PuzzleRefactor.Highlighting;
using Gameplay.PuzzleRefactor.Interfaces;
using Gameplay.PuzzleRefactor.ScriptableObjects;
using Gameplay.PuzzleRefactor.Utils;
using Gameplay.Puzzles.Directional;
using SavingSystem;
using UnityEngine;
using Utility.Data;

namespace Gameplay.PuzzleRefactor.Validators.FieldDataProviders
{
    [DisallowMultipleComponent, RequireComponent(typeof(StandardFieldRef))]
    public class DirectionFieldData : MonoBehaviour, IAssignable<PuzzleRefData>, ISave
    {
        #region Serialized Fields

        [SerializeField]
        private Vector3Variable playerPosition;
        [SerializeField]
        private Direction direction;

        #endregion

        #region Private Fields

        private StandardFieldRef field;
        private List<Transform> rotationSymbols;
        private PuzzleRef puzzle;
        private AnimationData animationData;

        private bool alreadyChecked;

        #endregion

        #region Constants

        private const float DotTolerance = 0.85f;
        private static readonly Vector3 DefaultVector = -Vector3.one; 

        #endregion

        #region Public Properties

        public Vector3 RequiredExitDirection { get; private set; } = DefaultVector;

        public string ListName => SavingSystemUtils.FieldIdList;

        #endregion

        private AnimationData AnimationData {
            get
            {
                if (animationData.IsNull())
                {
                    animationData = PuzzleDataProvider.DefaultAnimationData.DirectionalRotation;
                }

                return animationData;
            }
        }

        private List<Transform> RotationSymbols => rotationSymbols ??= transform.GetChildrenWithTag(AFieldUtils.FieldDirectionSymbolTag).ToList();

        #region Unity Callbacks

        private void Awake()
        {
            field = GetComponent<StandardFieldRef>();
            puzzle = GetComponentInParent<PuzzleRef>();
        }

        private void OnEnable()
        {
            field.OnStateChanged += FieldStateChanged;
            SavingManager.Instance.OnGameDataLoaded += OnGameDataLoaded;
        }

        private void OnDisable()
        {
            field.OnStateChanged -= FieldStateChanged;

            if (SavingManager.IsInstanceOnScene())
            {
                SavingManager.Instance.OnGameDataLoaded -= OnGameDataLoaded;
            }
        }

        #endregion

        #region Public Methods

        public void SetData(PuzzleRefData data)
        {
            animationData = data.DirectionalRotation;
        }

        #endregion

        #region Private Methods

        private void OnGameDataLoaded()
        {
            if (RequiredExitDirection == DefaultVector)
            {
                return;
            }

            RotateSymbols(0, null);
        }

        private void FieldStateChanged(AFieldState newState)
        {
            if (newState is FieldWalkableState)
            {
                StandardFieldRef lastCompleted = puzzle.CompletedFields.Last();
                TryRotate(lastCompleted, playerPosition.Value.GetClosestTransform(lastCompleted.transform.GetChildrenWithTag(AFieldUtils.FieldBaseTag)));
                puzzle.OnFieldEntered += CheckRotation;
            }
            else
            {
                puzzle.OnFieldEntered -= CheckRotation;
            }
        }

        private void CheckRotation(StandardFieldRef other, Transform fieldBase)
        {
            if (other == field || !field.IsActive)
            {
                return;
            }

            if (field.IsNeighbourOf(other))
            {
                TryRotate(other, fieldBase);
            }
        }

        private void TryRotate(StandardFieldRef neighbour, Transform currentBase)
        {
            Transform directionalClosestBase = currentBase.position.GetClosestTransform(RotationSymbols);
            Vector3 directionalClosestBasePosition = directionalClosestBase.position;

            if (currentBase.position.DistanceTo(directionalClosestBasePosition) > AFieldUtils.NeighbourBaseDistance)
            {
                return;
            }

            Transform otherClosestBase = directionalClosestBasePosition.GetClosestTransform(neighbour.transform.GetChildrenWithTag(AFieldUtils.FieldBaseTag));
            Vector3 fromBaseToDirectional = otherClosestBase.position.DirectionTo(directionalClosestBasePosition);

            RequiredExitDirection = GetRequiredRotateDirection(fromBaseToDirectional, directionalClosestBase.up);

            RotateSymbols(AnimationData.Duration, AnimationData.Curve);
        }

        private void RotateSymbols(float duration, AnimationCurve curve)
        {
            foreach (Transform rotationSymbol in RotationSymbols)
            {
                if (rotationSymbol.forward.IsFacing(RequiredExitDirection, DotTolerance))
                {
                    return;
                }

                float endAngle = rotationSymbol.localRotation.eulerAngles.y + Vector3.SignedAngle(rotationSymbol.forward, RequiredExitDirection, rotationSymbol.up);

                Tween tween = rotationSymbol.DOLocalRotate(new Vector3(0, endAngle, 0), duration);

                if (curve != null)
                {
                    tween.SetEase(curve);
                }
            }
        }

        private Vector3 GetRequiredRotateDirection(Vector3 fromBaseToDirectional, Vector3 upVector)
        {
            switch (direction)
            {
                case Direction.Forward :
                    return fromBaseToDirectional;
                case Direction.Left :
                    return Vector3.Cross(fromBaseToDirectional, upVector);
                case Direction.Right :
                    return -Vector3.Cross(fromBaseToDirectional, upVector);
                default :
                    return default;
            }
        }

        #endregion
    }
}
