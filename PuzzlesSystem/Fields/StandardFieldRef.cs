using System;
using System.Collections.Generic;
using Common.StateMachine.Abstract;
using Extensions;
using Gameplay.PuzzleRefactor.FieldStates;
using Gameplay.PuzzleRefactor.Other;
using SavingSystem;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using TMPro;
using UnityEngine;

namespace Gameplay.PuzzleRefactor.Fields
{
    [DisallowMultipleComponent]
    public class StandardFieldRef : AStateMachine<StandardFieldRef, AFieldState>, ISave
    {
        #region Events

        public event Action<StandardFieldRef, Transform> OnRegisterStep;
        public event Action<StandardFieldRef, Transform> OnPlayerEntered;
        public event Action<StandardFieldRef, Transform> OnPlayerExited;
        public event Action<StandardFieldRef, Transform> OnCompletionChanged;
        public event Action<AFieldState> OnStateChanged;

        #endregion

        #region Serialized Fields

        [SerializeField]
        protected TMP_Text stateText;
        [SerializeField, ReadOnly]
        protected PuzzleRef puzzle;

        #endregion

        #region Private Fields

        protected AFieldState walkableState;
        private AFieldState completedState;
        private AFieldState blockedState;
        private AFieldState inactiveState;
        private AFieldState completedIncorrectlyState;

        private BaseTriggerDetector[] playerDetectors;
        private Transform currentBase;

        #endregion

        #region Public Properties

        [field: SerializeField]
        public List<StandardFieldRef> Neighbours { get; protected set; }

        public virtual AFieldState InactiveState => inactiveState ??= new FieldInactiveState(this, puzzle);
        public AFieldState CompletedState => completedState ??= new FieldCompletedState(this, puzzle.EventsHandler);
        public virtual AFieldState WalkableState => walkableState ??= new FieldWalkableState(this, puzzle);
        public AFieldState BlockedState => blockedState ??= new FieldBlockedState(this, puzzle);
        public AFieldState CompletedIncorrectlyState => completedIncorrectlyState ??= new FieldCompletedIncorrectlyState(this, puzzle);
        public bool IsActive => !(currentState is FieldInactiveState);
        public bool IsCompleted { get; protected set; }
        public string ListName => SavingSystemUtils.FieldIdList;

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            playerDetectors = GetComponentsInChildren<BaseTriggerDetector>();

#if !DEVELOPMENT_BUILD && !UNITY_EDITOR
            Destroy(stateText.transform.parent.gameObject);
#endif

            if (!puzzle && !transform.TryGetComponentInParent(out puzzle))
            {
                Debug.LogWarning($"{name} doesnt have a reference to the Puzzle, this may cause errors, make sure to initialize the puzzle");

                return;
            }

            stateText.transform.parent.gameObject.SetActive(puzzle.ShowDebugFieldStates);
        }

        private void OnEnable()
        {
            AssignEvents();
        }

        private void OnDisable()
        {
            UnAssignEvents();
        }

        #endregion

        #region Public Methods

        public void ToggleStateCanvas(bool toggle)
        {
            stateText.transform.parent.gameObject.SetActive(toggle);

            if (toggle)
            {
                stateText.SetText($"{currentState.GetType().Name}");
            }
        }

        public override void SwitchTo(AFieldState newState)
        {
            bool invokeEvent = newState != currentState;
            base.SwitchTo(newState);

#if DEVELOPMENT_BUILD || UNITY_EDITOR

            if (puzzle.ShowDebugFieldStates)
            {
                stateText.SetText($"{newState.GetType().Name}");
            }
#endif

            if (invokeEvent)
            {
                OnStateChanged?.Invoke(newState);
            }
        }

        public void InvokeCompletionChanged(bool toggle)
        {
            if (IsCompleted == toggle)
            {
                return;
            }

            IsCompleted = toggle;
            OnCompletionChanged?.Invoke(this, currentBase);
        }

        #endregion

        #region Protected Methods

        protected override AFieldState GetFirstState() => InactiveState;

        #endregion

        #region Private Methods

        private void AssignEvents()
        {
            playerDetectors.ForEach(AssignTriggerEvents);
        }

        private void AssignTriggerEvents(BaseTriggerDetector trigger)
        {
            trigger.OnObjectEntered += PlayerEntered;
            trigger.OnObjectExited += PlayerExited;
        }

        private void UnassignTriggerEvents(BaseTriggerDetector trigger)
        {
            trigger.OnObjectEntered -= PlayerEntered;
            trigger.OnObjectExited -= PlayerExited;
        }

        private void UnAssignEvents()
        {
            playerDetectors.ForEach(UnassignTriggerEvents);
        }

        private void PlayerEntered(Transform fieldBase)
        {
            currentBase = fieldBase;

            OnRegisterStep?.Invoke(this, currentBase);
            OnPlayerEntered?.Invoke(this, currentBase);
        }

        private void PlayerExited(Transform fieldBase)
        {
            OnPlayerExited?.Invoke(this, fieldBase);
        }

        #endregion
    }
}
