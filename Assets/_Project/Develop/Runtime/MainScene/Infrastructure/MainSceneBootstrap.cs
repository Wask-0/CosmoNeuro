using System.Collections;
using _Project.Develop.Runtime.Infrastructure;
using _Project.Develop.Runtime.Infrastructure.AppStates;
using _Project.Develop.Runtime.Infrastructure.DI;
using _Project.Develop.Runtime.UI.MainScene;
using _Project.Develop.Runtime.Utilities.Input.Common;
using _Project.Develop.Runtime.Utilities.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;

namespace _Project.Develop.Runtime.MainScene.Infrastructure
{
    public class MainSceneBootstrap : SceneBootstrap
    {
        [SerializeField] private EventSystem _eventSystem;
        [SerializeField] private CommonInputUIBinding _uiInput;
        
        private DIContainer _container;
        private ICommonInput _input;
        
        private AppStateService _appState;
        
        public override void ProcessRegistrations(DIContainer container, IInputSceneArgs sceneArgs = null)
        {
            _container =  container;
            
            _input = _container.Resolve<ICommonInput>();
            _appState = _container.Resolve<AppStateService>();
            
            MainSceneContextRegistrations.Process(_container);
        }

        public override IEnumerator Initialize()
        {
            Debug.Log("Инициализация основной сцены");

            _uiInput.Initialize((CommonInputReader)_input);

            yield break;
        }

        public override void Run()
        {
            Debug.Log("Старт основной сцены");
            
            MainSceneScreenPresenter presenter = _container.Resolve<MainSceneScreenPresenter>();

            _eventSystem.firstSelectedGameObject = presenter.FirstSelected;
            _eventSystem.SetSelectedGameObject(presenter.FirstSelected);
            
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            _appState.SetState(AppState.MainMenu);
        }
    }
}