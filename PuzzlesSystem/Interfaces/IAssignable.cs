namespace Gameplay.PuzzleRefactor.Interfaces
{
    public interface IAssignable<T>
    {
        #region Public Methods

        public void SetData(T data);

        #endregion
    }
}
