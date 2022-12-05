using Gameplay.PuzzleRefactor.Fields;
using Gameplay.PuzzleRefactor.Validators.FieldDataProviders;
using Gameplay.Puzzles.Data;
using Gameplay.Puzzles.Musical.Data;

namespace Gameplay.PuzzleRefactor.FieldStates
{
    public class FieldCompletedState : AFieldState
    {
        #region Private Fields

        private PuzzleEventType eventInvoke;

        #endregion

        #region Constructors

        public FieldCompletedState(StandardFieldRef stateMachine, PuzzleEventsHandler eventsHandler) : base(stateMachine, eventsHandler)
        {
            SetEventType();
        }

        #endregion

        #region Public Methods

        public override void OnEnter()
        {
            base.OnEnter();
            stateMachine.InvokeCompletionChanged(true);
            eventsHandler.Invoke(eventInvoke);
        }

        public override void OnExit()
        {
            stateMachine.InvokeCompletionChanged(false);
            base.OnExit();
        }

        #endregion

        #region Private Methods

        private void SetEventType()
        {
            if (stateMachine.TryGetComponent(out MusicalFieldData musicalFieldData))
            {
                eventInvoke = musicalFieldData.SignalType == SignalType.One ? PuzzleEventType.ShortMusicalFieldCaptured : PuzzleEventType.LongMusicalFieldCaptured;
            }
            else
            {
                eventInvoke = PuzzleEventType.FieldCaptured;
            }
        }

        #endregion
    }
}
