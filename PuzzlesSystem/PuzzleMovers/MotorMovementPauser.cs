using KinematicCharacterController;
using Movement.MovementSystem.Core;
using UnityEngine;

namespace Gameplay.PuzzleRefactor.PuzzleMovers
{
    public class MotorMovementPauser
    {
        #region Private Fields

        private KinematicCharacterMotor characterMotor;
        private PlayerInputController playerInputController;
        private Transform playerTransform;

        #endregion

        #region Constructors

        public MotorMovementPauser(KinematicCharacterMotor characterMotor, PlayerInputController playerInputController, Transform playerTransform)
        {
            this.characterMotor = characterMotor;
            this.playerInputController = playerInputController;
            this.playerTransform = playerTransform;
        }

        #endregion

        #region Public Methods

        public void Pause()
        {
            ToggleMovement(false);
        }

        public void Resume()
        {
            characterMotor.SetTransientPosition(playerTransform.position);
            ToggleMovement(true);
        }

        #endregion

        #region Private Methods

        private void ToggleMovement(bool toggle)
        {
            playerInputController.ForceToggleInput(toggle);
            characterMotor.enabled = toggle;
            characterMotor.Capsule.enabled = toggle;
        }

        #endregion
    }
}
