using System;
using _Project.Develop.Runtime.Infrastructure.AppStates;
using _Project.Develop.Runtime.Infrastructure.DI;
using _Project.Develop.Runtime.UI.Gameplay;
using _Project.Develop.Runtime.UI.Gameplay.Pause;
using _Project.Develop.Runtime.UI.Settings;
using _Project.Develop.Runtime.Utilities.CoroutinesManagement;
using _Project.Develop.Runtime.Utilities.Input.Common;
using _Project.Develop.Runtime.Utilities.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;

namespace _Project.Develop.Runtime.Gameplay.Infrastructure
{
    public class GameplayPauseService : IInitializable, IDisposable
    {
        private readonly ICommonInput _input;
        private readonly AppStateService _appState;
        private readonly GameplayPopupService _popups;
        private readonly SceneSwitcherService _sceneSwitcher;
        private readonly ICoroutinesPerformer _coroutines;

        private PausePopupPresenter _pause;
        private bool _settingsOpen;
        private bool _leaving;
        private bool _disposed;

        private CursorLockMode _previousCursorLock;
        private bool _previousCursorVisible;

        public GameplayPauseService(
            ICommonInput input,
            AppStateService appState,
            GameplayPopupService popups,
            SceneSwitcherService sceneSwitcher,
            ICoroutinesPerformer coroutines)
        {
            _input = input;
            _appState = appState;
            _popups = popups;
            _sceneSwitcher = sceneSwitcher;
            _coroutines = coroutines;
        }

        public void Initialize()
        {
            _input.PausePressed += OnPausePressed;
        }

        public void Dispose()
        {
            _disposed = true;

            _input.PausePressed -= OnPausePressed;

            UnsubscribeFromPause();
            _pause = null;

            if (_appState.Current == AppState.Paused)
            {
                _appState.SetState(AppState.Gameplay);
                RestoreCursor();
            }
        }

        private void OnPausePressed()
        {
            if (_disposed || _leaving || _pause != null || _appState.Current != AppState.Gameplay)
                return;

            _previousCursorLock = Cursor.lockState;
            _previousCursorVisible = Cursor.visible;

            _appState.SetState(AppState.Paused);

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            try
            {
                _pause = _popups.OpenPause(OnPauseClosed);

                _pause.SettingsRequested += OnSettingsRequested;
                _pause.MainMenuRequested += OnMainMenuRequested;

                Select(_pause.FirstSelected);
            }
            catch
            {
                _appState.SetState(AppState.Gameplay);
                RestoreCursor();
                throw;
            }
        }

        private void OnPauseClosed()
        {
            UnsubscribeFromPause();
            _pause = null;

            if (_disposed || _leaving || _appState.Current != AppState.Paused)
                return;

            Select(null);

            _appState.SetState(AppState.Gameplay);
            RestoreCursor();
        }

        private void OnSettingsRequested()
        {
            if (_disposed || _leaving || _settingsOpen || _pause == null)
                return;

            _settingsOpen = true;
            _pause.SetInteractable(false);

            try
            {
                SettingsPopupPresenter settings = _popups.OpenSettings(OnSettingsClosed);

                Select(settings.FirstSelected);
            }
            catch
            {
                OnSettingsClosed();
                throw;
            }
        }

        private void OnSettingsClosed()
        {
            _settingsOpen = false;

            if (_disposed || _leaving || _pause == null)
                return;

            _pause.SetInteractable(true);
            Select(_pause.SettingsSelected);
        }

        private void OnMainMenuRequested()
        {
            if (_disposed || _leaving || _settingsOpen || _pause == null)
                return;

            _leaving = true;
            _pause.SetInteractable(false);

            Select(null);

            _coroutines.StartPerform(_sceneSwitcher.ProcessSwitchTo(Scenes.MainScene));
        }

        private void UnsubscribeFromPause()
        {
            if (_pause == null)
                return;

            _pause.SettingsRequested -= OnSettingsRequested;
            _pause.MainMenuRequested -= OnMainMenuRequested;
        }

        private void RestoreCursor()
        {
            Cursor.lockState = _previousCursorLock;
            Cursor.visible = _previousCursorVisible;
        }

        private static void Select(GameObject target)
        {
            if (EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(target);
        }
    }
}