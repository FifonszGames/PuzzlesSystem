using System.Collections;
using Gameplay.PuzzleRefactor.Fields;
using SavingSystem;
using UnityEngine;

namespace Gameplay.PuzzleRefactor.Other
{
    [DisallowMultipleComponent, RequireComponent(typeof(PuzzleRef))]
    public class FieldToggler : MonoBehaviour, ISave
    {
        #region Serialized Fields

        [SerializeField]
        private StandardFieldRef field;

        #endregion

        #region Private Fields

        private PuzzleRef puzzle;
        private bool isActivated;
        private bool loadedSave;
        private static readonly WaitForSeconds WaitForSeconds = new WaitForSeconds(1f);

        #endregion

        #region Public Properties

        public string ListName => SavingSystemUtils.PuzzleIdList;

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            puzzle = GetComponent<PuzzleRef>();
        }

        private void Start()
        {
            if (Application.isEditor || Debug.isDebugBuild)
            {
                StartCoroutine(SetFieldAfterTime());
            }
        }

        private IEnumerator SetFieldAfterTime()
        {
            yield return WaitForSeconds;

            if (!loadedSave)
            {
                ToggleField(false);
            }
        }

        private void OnEnable()
        {
            AssignEvents();
        }

        private void OnDisable()
        {
            UnassignEvents();
        }

        #endregion

        #region Private Methods

        private void AssignEvents()
        {
            puzzle.OnCompleted += ActivateField;
            SavingManager.Instance.OnGameDataLoaded += GameDataLoaded;
        }

        private void UnassignEvents()
        {
            puzzle.OnCompleted -= ActivateField;

            if (SavingManager.IsInstanceOnScene())
            {
                SavingManager.Instance.OnGameDataLoaded -= GameDataLoaded;
            }
        }

        private void GameDataLoaded()
        {
            loadedSave = true;
            ToggleField(isActivated);
        }

        private void ActivateField()
        {
            ToggleField(true);
        }

        private void ToggleField(bool toggle)
        {
            isActivated = toggle;
            field.gameObject.SetActive(toggle);
        }

        #endregion
    }
}
