using System;
using _Project.Develop.Runtime.UI.Core.Popup;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Develop.Runtime.UI.Gameplay.Pause
{
    public class PausePopupView : PopupViewBase
    {
        [SerializeField] private Button _resumeButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _mainMenuButton;

        public event Action ResumeClicked;
        public event Action SettingsClicked;
        public event Action MainMenuClicked;

        public GameObject FirstSelected => _resumeButton.gameObject;
        public GameObject SettingsSelected => _settingsButton.gameObject;

        public void SetInteractable(bool value)
        {
            _resumeButton.interactable = value;
            _settingsButton.interactable = value;
            _mainMenuButton.interactable = value;
        }

        private void OnEnable()
        {
            _resumeButton.onClick.AddListener(OnResumeClicked);
            _settingsButton.onClick.AddListener(OnSettingsClicked);
            _mainMenuButton.onClick.AddListener(OnMainMenuClicked);
        }

        private void OnDisable()
        {
            _resumeButton.onClick.RemoveListener(OnResumeClicked);
            _settingsButton.onClick.RemoveListener(OnSettingsClicked);
            _mainMenuButton.onClick.RemoveListener(OnMainMenuClicked);
        }

        private void OnResumeClicked() => ResumeClicked?.Invoke();
        private void OnSettingsClicked() => SettingsClicked?.Invoke();
        private void OnMainMenuClicked() => MainMenuClicked?.Invoke();
    }
}