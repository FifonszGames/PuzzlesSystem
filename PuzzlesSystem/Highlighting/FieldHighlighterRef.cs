using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Extensions;
using Gameplay.PuzzleRefactor.Fields;
using Gameplay.PuzzleRefactor.FieldStates;
using Gameplay.PuzzleRefactor.Interfaces;
using Gameplay.PuzzleRefactor.ScriptableObjects;
using UnityEngine;

namespace Gameplay.PuzzleRefactor.Highlighting
{
    [RequireComponent(typeof(StandardFieldRef))]
    public class FieldHighlighterRef : MonoBehaviour, IAssignable<PuzzleRefData>, IAssignable<IStateDataProvider>
    {
        #region Private Fields

        private IStateDataProvider stateData;
        private PuzzleRefData highlightData;
        private FieldStateData lastData;
        private List<Material> materials;
        private StandardFieldRef field;
        private Dictionary<int, Tween> currentTweens = new Dictionary<int, Tween>();

        private static readonly int FailProgressProperty = Shader.PropertyToID("_FailProgress");
        private static readonly int FailFadingProgressProperty = Shader.PropertyToID("_FailFadingProgress");
        private static readonly int CompletedProgressProperty = Shader.PropertyToID("_ActiveProgress");
        private static readonly int HighlightedProperty = Shader.PropertyToID("_ActiveProgressInside");

        #endregion

        #region Constants

        private const float DefaultDuration = 0.5f;
        private const string PuzzleShaderName = "DreamStorm/Puzzle Main";

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            GetReferences();
        }

        private void OnEnable()
        {
            AssignCallbacks();
        }

        private void OnDisable()
        {
            UnAssignCallbacks();
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
            materials = GetComponentsInChildren<MeshRenderer>().SelectMany(meshRenderer => meshRenderer.materials).Where(material => material.shader.name == PuzzleShaderName).ToList();
            field = GetComponent<StandardFieldRef>();
        }

        private void AssignCallbacks()
        {
            field.OnStateChanged += OnStateChanged;
        }

        private void UnAssignCallbacks()
        {
            field.OnStateChanged -= OnStateChanged;
        }

        private void OnStateChanged(AFieldState newState)
        {
            FieldStateData newData = stateData.GetData(newState.GetType());

            if (lastData == null)
            {
                ProcessNewData(newData);
            }
            else if (newData != lastData)
            {
                ProcessDataChanged(lastData, newData);
            }

            lastData = newData;
        }

        private void ProcessDataChanged(FieldStateData previousData, FieldStateData newData)
        {
            if (previousData.HighlightsFail != newData.HighlightsFail)
            {
                HighlightWrongCompletion(newData.HighlightsFail);
            }

            if (previousData.HighlightsInnerField != newData.HighlightsInnerField)
            {
                HighlightInner(newData.HighlightsInnerField);
            }

            if (previousData.HighlightsOuterField != newData.HighlightsOuterField)
            {
                HighlightOuter(newData.HighlightsOuterField);
            }
        }

        private void ProcessNewData(FieldStateData newData)
        {
            if (newData.HighlightsFail)
            {
                HighlightWrongCompletion(newData.HighlightsFail);
            }

            if (newData.HighlightsInnerField)
            {
                HighlightInner(newData.HighlightsInnerField);
            }

            if (newData.HighlightsOuterField)
            {
                HighlightOuter(newData.HighlightsOuterField);
            }
        }

        private void HighlightWrongCompletion(bool toggle)
        {
            float endValue = toggle ? 1 : 0;
            AnimateProperty(FailProgressProperty, endValue, toggle ? highlightData.FailProgressAnimation.OnData : highlightData.FailProgressAnimation.OffData);
            AnimateProperty(FailFadingProgressProperty, endValue, toggle ? highlightData.FailFadingAnimation.OnData : highlightData.FailFadingAnimation.OffData);
        }

        private void HighlightOuter(bool toggle)
        {
            float endValue = toggle ? 1 : 0;
            AnimateProperty(CompletedProgressProperty, endValue, toggle ? highlightData.OuterFieldAnimation.OnData : highlightData.OuterFieldAnimation.OffData);
        }

        private void HighlightInner(bool toggle)
        {
            float endValue = toggle ? 1 : 0;
            AnimateProperty(HighlightedProperty, endValue, toggle ? highlightData.InnerFieldAnimation.OnData : highlightData.InnerFieldAnimation.OffData);
        }

        private void AnimateProperty(int shaderProperty, float endValue, AnimationData data)
        {
            bool isNull = data.IsNull();
            AnimateProperty(shaderProperty, endValue, isNull ? DefaultDuration : data.Duration, isNull ? null : data.Curve);
        }

        private void AnimateProperty(int shaderProperty, float endValue, float duration, AnimationCurve curve = null)
        {
            if (!currentTweens.ContainsKey(shaderProperty))
            {
                currentTweens.Add(shaderProperty, null);
            }
            else
            {
                KillTweenFromProperty(shaderProperty);
            }

            currentTweens[shaderProperty] = materials.AnimateMaterialsProperty(shaderProperty, endValue, duration);

            if (curve != null)
            {
                currentTweens[shaderProperty].SetEase(curve);
            }
        }

        private void KillTweenFromProperty(int shaderProperty)
        {
            Tween tween = currentTweens[shaderProperty];

            if (tween != null && tween.IsPlaying())
            {
                tween.Kill();
            }
        }

        #endregion
    }
}
