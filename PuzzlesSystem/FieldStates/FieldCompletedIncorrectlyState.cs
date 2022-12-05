using Gameplay.PuzzleRefactor.Fields;

namespace Gameplay.PuzzleRefactor.FieldStates
{
    public class FieldCompletedIncorrectlyState : AFieldState
    {
        #region Constructors

        public FieldCompletedIncorrectlyState(StandardFieldRef stateMachine, PuzzleRef puzzle) : base(stateMachine, puzzle.EventsHandler)
        {
        }

        #endregion
    }
}
