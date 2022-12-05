using Gameplay.PuzzleRefactor.FieldStates;

namespace Gameplay.PuzzleRefactor.Fields
{
    public class ActivationFieldRef : StandardFieldRef
    {
        #region Public Properties

        public override AFieldState InactiveState => WalkableState;
        public override AFieldState WalkableState => walkableState ??= new ActivationFieldWalkableState(this, puzzle);

        #endregion

        #region Protected Methods

        protected override AFieldState GetFirstState() => WalkableState;

        #endregion
    }
}
