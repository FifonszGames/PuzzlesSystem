using System.Collections.Generic;
using System.Linq;
using Gameplay.PuzzleRefactor.ScriptableObjects;
using KinematicCharacterController;
using Movement.MovementSystem.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Gameplay.PuzzleRefactor.PuzzleMovers
{
    [RequireComponent(typeof(KinematicCharacterMotor), typeof(PlayerInputController))]
    public abstract class APuzzleMover : MonoBehaviour
    {
        #region Serialized Fields

        [SerializeField]
        private InputActionReference moverInput;
        [SerializeField]
        private RuntimePuzzleData runtimePuzzleData;

        #endregion

        #region Private Fields

        protected Transform playerTransform;
        protected bool isActive;
        private MotorMovementPauser motorMovementPauser;
        private List<APuzzleMover> otherMovers;

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            Initialize();
        }

        private void OnEnable()
        {
            moverInput.action.performed += TryPerformMovement;
        }

        private void OnDisable()
        {
            moverInput.action.performed -= TryPerformMovement;
        }

        #endregion

        #region Protected Methods

        protected void MovementToggled(bool toggle)
        {
            isActive = toggle;

            if (toggle)
            {
                motorMovementPauser.Pause();
            }
            else
            {
                motorMovementPauser.Resume();
            }
        }

        protected abstract void PerformMovement(PuzzleRef puzzleRef);

        protected virtual bool CanPerformMovement(PuzzleRef puzzleRef) => puzzleRef != null && !puzzleRef.IsCompleted && puzzleRef.HasActiveField && !otherMovers.Any(mover => mover.isActive);

        #endregion

        #region Private Methods

        private void Initialize()
        {
            playerTransform = transform;
            otherMovers = GetComponents<APuzzleMover>().ToList();
            otherMovers.Remove(this);
            //TODO REPLACE IN MOVEMENT REFACTOR
            motorMovementPauser = new MotorMovementPauser(GetComponent<KinematicCharacterMotor>(), GetComponent<PlayerInputController>(), playerTransform);
        }

        private void TryPerformMovement(InputAction.CallbackContext context)
        {
            PuzzleRef puzzleRef = runtimePuzzleData.currentPuzzle;

            if (!CanPerformMovement(puzzleRef))
            {
                return;
            }

            PerformMovement(puzzleRef);
        }

        #endregion
    }
}
