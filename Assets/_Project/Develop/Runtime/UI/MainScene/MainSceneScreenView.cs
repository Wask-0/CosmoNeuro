using System;
using _Project.Develop.Runtime.UI.Core;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Develop.Runtime.UI.MainScene
{
    public class MainSceneScreenView : MonoBehaviour, IView
    {
        public event Action PlayClicked;
        public event Action SettingsClicked;
        public event Action ExitClicked;
        
        [SerializeField] private Button _playButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _exitButton;
        
        private CanvasGroup _canvasGroup;
        public GameObject FirstSelected => _playButton.gameObject;
        public GameObject ExitSelected => _exitButton.gameObject;
        public GameObject SettingsSelected => _settingsButton.gameObject;

        public void SetInteractable(bool value)
        {
            _canvasGroup.interactable = value;
            _canvasGroup.blocksRaycasts = value;
        }
        
        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }
        
        private void OnEnable()
        {
            _playButton.onClick.AddListener(OnPlayClicked);
            _settingsButton.onClick.AddListener(OnSettingsClicked);
            _exitButton.onClick.AddListener(OnExitClicked);
        }

        private void OnDisable()
        {
            _playButton.onClick.RemoveListener(OnPlayClicked);
            _settingsButton.onClick.RemoveListener(OnSettingsClicked);
            _exitButton.onClick.RemoveListener(OnExitClicked);
        }

        private void OnPlayClicked() => PlayClicked?.Invoke();

        private void OnSettingsClicked() => SettingsClicked?.Invoke();

        private void OnExitClicked() => ExitClicked?.Invoke();
    }
}