using System;
using System.Collections.Generic;
using System.Linq;
using Common.StateMachine.Abstract;
using Extensions;
using Gameplay.PuzzleRefactor.Fields;
using Gameplay.PuzzleRefactor.Interfaces;
using Gameplay.PuzzleRefactor.Other;
using Gameplay.PuzzleRefactor.PuzzleStates;
using Gameplay.PuzzleRefactor.ScriptableObjects;
using Gameplay.PuzzleRefactor.Utils;
using Gameplay.Puzzles;
using Gameplay.Puzzles.Data;
using QFSW.QC;
using SavingSystem;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

namespace Gameplay.PuzzleRefactor
{
    public class PuzzleRef : AStateMachine<PuzzleRef, APuzzleState>, ICompletable, ISave
    {
        #region Events

        public event Action OnCompleted;
        public event Action<StandardFieldRef> OnCompletedFieldAdded;
        public event Action<StandardFieldRef, Transform> OnFieldEntered;

        #endregion

        #region Serialized Fields

        [SerializeField, ReadOnly]
        private List<StandardFieldRef> fields;
        [SerializeField]
        private List<Transform> resetPoints;

        [field: SerializeField, FoldoutGroup("Overrides")]
        public bool overrideStateData;
        [SerializeField, ShowIf("overrideStateData"), Required, FoldoutGroup("Overrides")]
        private OverrideStateData stateOverrideData;
        [field: SerializeField, FoldoutGroup("Overrides")]
        public bool overrideHighlightData;
        [SerializeField, ShowIf("overrideHighlightData"), Required, FoldoutGroup("Overrides")]
        private PuzzleRefData highlightOverrideData;

        #endregion

        #region Private Fields

        [ShowInInspector, ReadOnly, FoldoutGroup("RuntimeData")]
        private List<PuzzleStep> puzzleSteps;
        private TagTriggerDetector playerDetector;
        private APuzzleState lastSavedState;

        private CompletedState completedState;
        private CompletedIncorrectlyState completedIncorrectlyState;
        private InactiveState inactiveState;
        private InProgressState inProgressState;

        #endregion

        #region Constants

        public const int ActivationFieldsRequiredAmount = 2;

        #endregion

        #region Public Properties

        [field: SerializeField, Required, PropertyOrder(-2)]
        public PuzzleEventsHandler EventsHandler { get; private set; }
        [field: SerializeField, PropertyOrder(-1)]
        public bool ShowDebugFieldStates { get; private set; }

        [field: ShowInInspector, ReadOnly, FoldoutGroup("RuntimeData")]
        public List<StandardFieldRef> CompletedFields { get; private set; }
        [field: ShowInInspector, ReadOnly, FoldoutGroup("RuntimeData")]
        public Transform CurrentBase { get; private set; }
        [field: ShowInInspector, ReadOnly, FoldoutGroup("RuntimeData")]
        public StandardFieldRef CurrentField { get; private set; }
        public bool IsCompletedIncorrectly => currentState is CompletedIncorrectlyState;
        public int ActivationFieldsCount { get; private set; }
        public bool PlayerIsOnField => CurrentField != null;
        public bool IsCompleted => currentState is CompletedState;
        public bool HasActiveField => CompletedFields != null && CompletedFieldsCount > 0;
        public int CompletedFieldsCount => CompletedFields?.Count ?? 0;
        public Vector3 CurrentResetPoint { get; set; }
        public CompletedState CompletedState => completedState ??= new CompletedState(this);
        public CompletedIncorrectlyState CompletedIncorrectlyState => completedIncorrectlyState ??= new CompletedIncorrectlyState(this);
        public InactiveState InactiveState => inactiveState ??= new InactiveState(this);
        public InProgressState InProgressState => inProgressState ??= new InProgressState(this, playerDetector);
        public IReadOnlyCollection<StandardFieldRef> Fields => fields;
        public IReadOnlyList<PuzzleStep> PuzzleSteps => puzzleSteps;
        public IReadOnlyCollection<Transform> ResetPoints => resetPoints;

        public string ListName => SavingSystemUtils.PuzzleIdList;

        public bool IsLoading { get; private set; }

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            GetReferences();
            ActivationFieldsCount = fields.Count(field => field is ActivationFieldRef);
            AssignHighlightData();
            AssignStateData();
        }

        private void OnEnable() => AssignEvents();
        private void OnDisable() => UnassignEvents();

        #endregion

        #region Public Methods

        [Command("puzzleref_debug_states", "Show debug canvas of field states", Platform.AllPlatforms, MonoTargetType.All)]
        public void ToggleFieldsDebugState(bool toggle)
        {
            ShowDebugFieldStates = toggle;
            fields.ForEach(field => field.ToggleStateCanvas(toggle));
        }

        public void AddStep(PuzzleStep puzzleStep)
        {
            puzzleSteps.Add(puzzleStep);
            CurrentField = puzzleStep.ToField;
            CurrentBase = puzzleStep.ToBase;
        }

        public void SetCurrentField(StandardFieldRef field)
        {
            CurrentField = field;
        }

        public void SetCurrentBase(Transform fieldBase)
        {
            CurrentBase = fieldBase;
        }

        public void AddCompleted(StandardFieldRef completedField)
        {
            CompletedFields.Add(completedField);
            OnCompletedFieldAdded?.Invoke(completedField);
        }

        public void DeactivatePuzzle()
        {
            SwitchTo(InactiveState);
            EventsHandler.Invoke(PuzzleEventType.PuzzleReset);
        }

        public void ForceComplete()
        {
            if (IsCompleted)
            {
                return;
            }

            RefreshCompletionList();
            RefreshStepsList();
            SwitchTo(CompletedState);
        }

        public void RevertMove()
        {
            if (!HasActiveField)
            {
                return;
            }

            if (CompletedFieldsCount > 1)
            {
                RevertLastMove();

                return;
            }

            DeactivatePuzzle();
        }

        public void RefreshCompletionList() => CompletedFields = new List<StandardFieldRef>();
        public void RefreshStepsList() => puzzleSteps = new List<PuzzleStep>();
        public void FieldEntered(StandardFieldRef field, Transform fieldBase) => OnFieldEntered?.Invoke(field, fieldBase);
        public void InvokeOnCompleted() => OnCompleted?.Invoke();

        #endregion

        #region Protected Methods

        protected override APuzzleState GetFirstState() => InactiveState;

        #endregion

        #region Private Methods

        private void GetReferences()
        {
            fields ??= GetComponentsInChildren<StandardFieldRef>().ToList();
            playerDetector = GetComponentInChildren<TagTriggerDetector>();
        }

        private void AssignEvents()
        {
            playerDetector.OnObjectExited += PlayerExitedResetRange;
            playerDetector.OnObjectEntered += PlayerEnteredRange;

            SavingManager.Instance.OnGameDataLoaded += GameDataLoaded;
            SavingManager.Instance.OnGameDataSavingStarted += GameDataSavingStarted;
        }

        private void UnassignEvents()
        {
            playerDetector.OnObjectExited -= PlayerExitedResetRange;
            playerDetector.OnObjectEntered -= PlayerEnteredRange;

            if (SavingManager.IsInstanceOnScene())
            {
                SavingManager.Instance.OnGameDataLoaded -= GameDataLoaded;
                SavingManager.Instance.OnGameDataSavingStarted -= GameDataSavingStarted;
            }
        }

        private void AssignHighlightData()
        {
            PuzzleRefData puzzleRefData =
                overrideHighlightData ? highlightOverrideData == null ? PuzzleDataProvider.DefaultAnimationData : highlightOverrideData : PuzzleDataProvider.DefaultAnimationData;
            GetComponentsInChildren<IAssignable<PuzzleRefData>>(true).ForEach(assignable => assignable.SetData(puzzleRefData));
        }

        private void AssignStateData()
        {
            IStateDataProvider stateDataProvider = overrideStateData ? stateOverrideData == null ? PuzzleDataProvider.DefaultStateData : stateOverrideData : PuzzleDataProvider.DefaultStateData;
            GetComponentsInChildren<IAssignable<IStateDataProvider>>(true).ForEach(assignable => assignable.SetData(stateDataProvider));
        }

        private void GameDataSavingStarted() => lastSavedState = IsCompletedIncorrectly ? InactiveState : currentState;

        private void GameDataLoaded()
        {
            if (lastSavedState == null)
            {
                return;
            }

            IsLoading = true;

            if (lastSavedState is InProgressState && !CompletedFields.IsNullOrEmpty())
            {
                CompletedFields.ForEach(field => field.SwitchTo(field.CompletedState));
                StandardFieldRef lastCompleted = CompletedFields.Last();

                foreach (StandardFieldRef notCompletedField in fields.Where(field => !CompletedFields.Contains(field)))
                {
                    notCompletedField.SwitchTo(notCompletedField.IsNeighbourOf(lastCompleted) ? notCompletedField.WalkableState : notCompletedField.BlockedState);
                }
            }

            SwitchTo(lastSavedState);
            IsLoading = false;
        }

        private void RevertLastMove()
        {
            StandardFieldRef lastCompleted = CompletedFields[CompletedFields.Count - 1];
            StandardFieldRef oneBeforeLast = CompletedFields[CompletedFields.Count - 2];

            lastCompleted.Neighbours.WithoutNeighboursOf(oneBeforeLast, oneBeforeLast).Where(neighbour => !neighbour.IsCompleted).ForEach(field => field.SwitchTo(field.BlockedState));
            oneBeforeLast.Neighbours.Where(neighbour => !neighbour.IsCompleted).ForEach(neighbour => neighbour.SwitchTo(neighbour.WalkableState));

            oneBeforeLast.SwitchTo(oneBeforeLast.CompletedState);
            lastCompleted.SwitchTo(lastCompleted.WalkableState);

            RevertToLastStep(lastCompleted);
            EventsHandler.Invoke(PuzzleEventType.MoveCanceled);
        }

        private void RevertToLastStep(StandardFieldRef lastCompleted)
        {
            PuzzleStep puzzleStep = puzzleSteps.Last(step => step.ToField == lastCompleted && step.WasCompletion);
            int lastStepIndexToDelete = puzzleSteps.IndexOf(puzzleStep);
            puzzleSteps.RemoveRange(lastStepIndexToDelete, puzzleSteps.Count - lastStepIndexToDelete);
            CurrentBase = puzzleStep.ToBase;
        }

        private void PlayerEnteredRange() => PuzzleDataProvider.RuntimePuzzleData.currentPuzzle = this;

        private void PlayerExitedResetRange()
        {
            if (PuzzleDataProvider.RuntimePuzzleData.currentPuzzle != this)
            {
                return;
            }

            PuzzleDataProvider.RuntimePuzzleData.currentPuzzle = null;
        }

        #endregion
    }
}
