using _Project.Develop.Runtime.Infrastructure.DI;
using _Project.Develop.Runtime.UI.LoadingScreen;
using System;
using System.Collections;
using _Project.Develop.Runtime.Infrastructure;
using _Project.Develop.Runtime.Infrastructure.AppStates;
using Object = UnityEngine.Object;

namespace _Project.Develop.Runtime.Utilities.SceneManagement
{
    public class SceneSwitcherService : IDisposable
    {
        private readonly SceneLoaderService _sceneLoaderService;
        private readonly ILoadingScreen _loadingScreen;
        private readonly DIContainer _projectContainer;
        
        private readonly AppStateService _appState;

        private DIContainer _currentSceneContainer;

        private bool _isSwitching;

        public SceneSwitcherService(
            SceneLoaderService sceneLoaderService,
            ILoadingScreen loadingScreen,
            DIContainer projectContainer,
            AppStateService appState)
        {
            _sceneLoaderService = sceneLoaderService;
            _loadingScreen = loadingScreen;
            _projectContainer = projectContainer;
            _appState = appState;
        }

        public IEnumerator ProcessSwitchTo(string sceneName, IInputSceneArgs sceneArgs = null)
        {
            if (_isSwitching)
                yield break;

            _isSwitching = true;

            try
            {
                _appState.SetState(AppState.Loading);
                
                _loadingScreen.Show();

                _currentSceneContainer?.Dispose();
                _currentSceneContainer = null;

                // Object.Destroy срабатывает в конце кадра. Если в этот момент уже идёт асинхронная
                // загрузка сцены, редактор может намертво зависнуть: уничтожение SpriteRenderer и поток
                // загрузки ждут блокировки друг друга. Поэтому загрузка начинается только на следующем кадре.
                yield return null;
                yield return _sceneLoaderService.LoadAsync(Scenes.Empty);
                yield return null;
                yield return _sceneLoaderService.LoadAsync(sceneName);

                SceneBootstrap sceneBootstrap = Object.FindAnyObjectByType<SceneBootstrap>();

                if (sceneBootstrap == null)
                    throw new InvalidOperationException("SceneBootstrap not found");

                _currentSceneContainer = new DIContainer(_projectContainer);

                sceneBootstrap.ProcessRegistrations(_currentSceneContainer, sceneArgs);
                
                _currentSceneContainer.Initialize();

                yield return sceneBootstrap.Initialize();

                _loadingScreen.Hide();

                sceneBootstrap.Run();
            }
            finally
            {
                _isSwitching = false;
                _loadingScreen.Hide();
            }
        }

        public void Dispose()
        {
            _currentSceneContainer?.Dispose();
            _currentSceneContainer = null;
        }
    }
}