namespace _Project.Develop.Runtime.UI.Core
{
    public interface ISubscribedPresenter : IPresenter
    {
        void Subscribe();

        void Unsubscribe();
    }
}