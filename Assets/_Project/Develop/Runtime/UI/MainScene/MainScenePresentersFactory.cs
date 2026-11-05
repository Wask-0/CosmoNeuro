using _Project.Develop.Runtime.Infrastructure.AppStates;
using _Project.Develop.Runtime.Infrastructure.DI;
using _Project.Develop.Runtime.Utilities.CoroutinesManagement;
using _Project.Develop.Runtime.Utilities.SceneManagement;

namespace _Project.Develop.Runtime.UI.MainScene
{
    public class MainScenePresentersFactory
    {
        private readonly DIContainer _container;

        public MainScenePresentersFactory(DIContainer container)
        {
            _container = container;
        }

        public MainSceneScreenPresenter CreateMainMenuScreen(
            MainSceneScreenView view)
        {
            return new MainSceneScreenPresenter(
                view,
                _container.Resolve<SceneSwitcherService>(),
                _container.Resolve<ICoroutinesPerformer>(),
                _container.Resolve<AppStateService>(),
                _container.Resolve<MainScenePopupService>());
        }
    }
}