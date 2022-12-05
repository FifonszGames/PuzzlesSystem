using DG.Tweening;
using Extensions;
using Gameplay.PuzzleRefactor.Fields;
using Gameplay.PuzzleRefactor.FieldStates;
using Gameplay.PuzzleRefactor.Interfaces;
using Gameplay.PuzzleRefactor.ScriptableObjects;
using Sirenix.Utilities;
using UnityEngine;
using Utility.Data;

namespace Gameplay.PuzzleRefactor.Highlighting
{
    [RequireComponent(typeof(Collider))]
    public class BarrierHighlighterRef : MonoBehaviour, IAssignable<PuzzleRefData>, IAssignable<IStateDataProvider>
    {
        #region Serialized Fields

        [SerializeField]
        private Vector3Variable playerPosition;

        #endregion

        #region Private Fields

        private IStateDataProvider stateData;
        private PuzzleRefData highlightData;
        private StandardFieldRef field;
        private Material material;
        private Tween visibilityTween;

        private static readonly int ProgressVisibilityParameter = Shader.PropertyToID("_ProgressVisibility");
        private static readonly int PlayerPositionProperty = Shader.PropertyToID("_PlayerPosition");

        #endregion

        #region Constants

        private const float DefaultDuration = 0.5f;

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            GetReferences();
            GetComponents<Collider>().ForEach(collider => collider.isTrigger = false);
            field.OnStateChanged += FieldStateChanged;
            gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            AssignCallback();
            AnimateVisibility();
        }

        private void OnDisable()
        {
            TryKillTween();
            UnAssignCallback();
        }

        private void OnDestroy()
        {
            field.OnStateChanged -= FieldStateChanged;
        }

        #endregion

        #region Public Methods

        public void SetData(PuzzleRefData data)
        {
            highlightData = data;
        }

        public void SetData(IStateDataProvider data)
        {
            stateData = data;
        }

        #endregion

        #region Private Methods

        private void GetReferences()
        {
            material = GetComponentInChildren<MeshRenderer>().material;
            field = GetComponentInParent<StandardFieldRef>();
        }

        private void AssignCallback()
        {
            playerPosition.OnValueChanged += PlayerPositionChanged;
        }

        private void UnAssignCallback()
        {
            playerPosition.OnValueChanged -= PlayerPositionChanged;
        }

        private void FieldStateChanged(AFieldState newState)
        {
            FieldStateData fieldStateData = stateData.GetData(newState.GetType());

            if (fieldStateData.HasActiveBarrier && !gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }
            else if (!fieldStateData.HasActiveBarrier && gameObject.activeSelf)
            {
                gameObject.SetActive(false);
            }
        }

        private void AnimateVisibility()
        {
            visibilityTween = material.AnimateMaterialProperty(ProgressVisibilityParameter, 0, 0);

            bool isNull = highlightData.BarrierAnimation.IsNull();
            visibilityTween = material.AnimateMaterialProperty(ProgressVisibilityParameter, 1, isNull ? DefaultDuration : highlightData.BarrierAnimation.Duration);

            if (!isNull)
            {
                visibilityTween.SetEase(highlightData.BarrierAnimation.Curve);
            }
        }

        private void TryKillTween()
        {
            if (visibilityTween != null && visibilityTween.IsPlaying())
            {
                visibilityTween.Kill();
            }
        }

        private void PlayerPositionChanged()
        {
            material.SetVector(PlayerPositionProperty, playerPosition.Value);
        }

        #endregion
    }
}
