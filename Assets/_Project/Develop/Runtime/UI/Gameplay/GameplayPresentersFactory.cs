using _Project.Develop.Runtime.Infrastructure.DI;
using _Project.Develop.Runtime.UI.Gameplay.Pause;
using _Project.Develop.Runtime.Utilities.CoroutinesManagement;
using _Project.Develop.Runtime.Utilities.Input.Common;

namespace _Project.Develop.Runtime.UI.Gameplay
{
    public class GameplayPresentersFactory
    {
        private readonly DIContainer _container;

        public GameplayPresentersFactory(DIContainer container)
        {
            _container = container;
        }

        public PausePopupPresenter CreatePausePopupPresenter(PausePopupView view)
        {
            return new PausePopupPresenter(_container.Resolve<ICoroutinesPerformer>(),
                view, _container.Resolve<ICommonInput>());
        }
    }
}