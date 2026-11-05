using System;
using System.Collections.Generic;
using _Project.Develop.Runtime.Comics;
using _Project.Develop.Runtime.Infrastructure.AppStates;
using _Project.Develop.Runtime.UI.Confirm;
using _Project.Develop.Runtime.UI.Core;
using _Project.Develop.Runtime.UI.Settings;
using _Project.Develop.Runtime.Utilities.CoroutinesManagement;
using _Project.Develop.Runtime.Utilities.SceneManagement;
using UnityEngine.EventSystems;

namespace _Project.Develop.Runtime.UI.MainScene
{
    public class MainSceneScreenPresenter : IPresenter
    {
        private readonly MainSceneScreenView _view;
        private readonly SceneSwitcherService _sceneSwitcher;
        private readonly ICoroutinesPerformer _coroutines;
        
        private readonly List<IPresenter> _childPresenters = new();
        
        private readonly AppStateService _appState;
        
        private readonly MainScenePopupService _popupService;

        private bool _initialized;
        private bool _disposed;
        
        private bool _popupOpen;

        public MainSceneScreenPresenter(
            MainSceneScreenView view,
            SceneSwitcherService sceneSwitcher,
            ICoroutinesPerformer coroutines,
            AppStateService appState,
            MainScenePopupService popupService)
        {
            _view = view;
            _sceneSwitcher = sceneSwitcher;
            _coroutines = coroutines;
            _appState = appState;
            _popupService = popupService;
        }
        
        public UnityEngine.GameObject FirstSelected => _view.FirstSelected;

        public void Initialize()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(MainSceneScreenPresenter));

            if (_initialized)
                return;

            _view.PlayClicked += OnPlayClicked;
            _view.SettingsClicked += OnSettingsClicked;
            _view.ExitClicked += OnExitClicked;

            _initialized = true;
            
            foreach (IPresenter presenter in _childPresenters)
                presenter.Initialize();
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            if (_initialized)
            {
                _view.PlayClicked -= OnPlayClicked;
                _view.SettingsClicked -= OnSettingsClicked;
                _view.ExitClicked -= OnExitClicked;
            }

            _initialized = false;
            
            foreach (IPresenter presenter in _childPresenters)
                presenter.Dispose();

            _childPresenters.Clear();
        }

        private void OnPlayClicked()
        {
            if (_disposed || _popupOpen || _appState.Current != AppState.MainMenu)
                return;

            _coroutines.StartPerform(_sceneSwitcher.ProcessSwitchTo(
                Scenes.Comic, new ComicInputArgs(ComicId.Intro, Scenes.Gameplay)));
        }
        
        private void OnExitClicked()
        {
            if (_disposed || _popupOpen || _appState.Current != AppState.MainMenu)
                return;

            _popupOpen = true;
            
            _view.SetInteractable(false);

            try
            {
                ConfirmPopupPresenter popup = _popupService.OpenConfirmation(
                    titleKey: "Exit.Title", messageKey: "Exit.Message",
                    confirmed: QuitGame, closed: OnConfirmationClosed);
                
                if (EventSystem.current != null)
                    EventSystem.current.SetSelectedGameObject(popup.FirstSelected);
            }
            catch
            {
                OnConfirmationClosed();
                throw;
            }
        }

        private void OnConfirmationClosed()
        {
            _popupOpen = false;
            
            if (_disposed)
                return;

            _view.SetInteractable(true);

            if (EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(_view.ExitSelected);
        }

        private void QuitGame()
        {
            if (UnityEngine.Application.isEditor)
            {
                UnityEngine.Debug.Log("Выход подтверждён.");
                return;
            }

            UnityEngine.Application.Quit();
        }
        
        private void OnSettingsClicked()
        {
            if (_disposed || _popupOpen || _appState.Current != AppState.MainMenu)
                return;

            _popupOpen = true;
            _view.SetInteractable(false);

            try
            {
                SettingsPopupPresenter popup = _popupService.OpenSettings(OnSettingsClosed);

                if (EventSystem.current != null)
                    EventSystem.current.SetSelectedGameObject(popup.FirstSelected);
            }
            catch
            {
                OnSettingsClosed();
                throw;
            }
        }

        private void OnSettingsClosed()
        {
            _popupOpen = false;

            if (_disposed)
                return;

            _view.SetInteractable(true);

            if (EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject( _view.SettingsSelected);
        }
    }
}