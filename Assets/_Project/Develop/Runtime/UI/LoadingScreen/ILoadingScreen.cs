namespace _Project.Develop.Runtime.UI.LoadingScreen
{
    public interface ILoadingScreen
    {
        bool IsShow { get; }

        void Show();

        void Hide();
    }
}
