using System;
using System.Collections.Generic;
using _Project.Develop.Runtime.UI.Core.Popup;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Develop.Runtime.UI.Settings
{
    public class SettingsPopupView : PopupViewBase
    {
        [SerializeField] private Slider _masterVolume;
        [SerializeField] private Slider _musicVolume;
        [SerializeField] private Slider _sfxVolume;
        [SerializeField] private Slider _mouseSensitivity;

        [SerializeField] private Button _applyButton;
        [SerializeField] private Button _cancelButton;
        
        [SerializeField] private Button _firstTabButton;
        
        [SerializeField] private SettingsTabsView _tabsView;
        
        [SerializeField] private TMP_Dropdown _language;
        
        [SerializeField] private TMP_Dropdown _screenMode;
        [SerializeField] private TMP_Dropdown _resolution;
        [SerializeField] private TMP_Dropdown _frameRate;
        [SerializeField] private TMP_Dropdown _quality;
        [SerializeField] private Toggle _vSync;
        
        [SerializeField] private Button _resetButton;

        public int ScreenModeIndex => _screenMode.value;
        public int ResolutionIndex => _resolution.value;
        public int FrameRateIndex => _frameRate.value;
        public int QualityIndex => _quality.value;
        public bool IsVSyncEnabled => _vSync.isOn;

        public event Action ApplyClicked;
        public event Action CancelClicked;
        public event Action<int> ResetClicked;
        
        public event Action ValuesChanged;

        public float MasterVolume => _masterVolume.value;
        public float MusicVolume => _musicVolume.value;
        public float SfxVolume => _sfxVolume.value;
        public float MouseSensitivity => _mouseSensitivity.value;
        
        public int LanguageIndex => _language.value;

        public GameObject FirstSelected => _firstTabButton.gameObject;
        
        public GameObject AfterApplySelected => _cancelButton.gameObject;

        public void SetValues(float masterVolume, float musicVolume, float sfxVolume)
        {
            _masterVolume.SetValueWithoutNotify(masterVolume);
            _musicVolume.SetValueWithoutNotify(musicVolume);
            _sfxVolume.SetValueWithoutNotify(sfxVolume);
        }

        public void SetMouseSensitivityRange(float min, float max)
        {
            _mouseSensitivity.minValue = min;
            _mouseSensitivity.maxValue = max;
        }

        public void SetMouseSensitivity(float value)
        {
            _mouseSensitivity.SetValueWithoutNotify(value);
        }

        public void SetInteractable(bool value)
        {
            
            _masterVolume.interactable = value;
            _musicVolume.interactable = value;
            _sfxVolume.interactable = value;

            _applyButton.interactable = value;
            _cancelButton.interactable = value;
            _resetButton.interactable = value;
            
            _language.interactable = value;
            
            _screenMode.interactable = value;
            _resolution.interactable = value;
            _frameRate.interactable = value;
            _quality.interactable = value;
            _vSync.interactable = value;
            _mouseSensitivity.interactable = value;
            
            _tabsView.RefreshNavigation();
        }
        
        public void SetApplyAvailable(bool available)
        {
            _applyButton.interactable = available;

            _tabsView.RefreshNavigation();
        }
        
        public void SetLanguages(List<string> names, int selected)
        {
            _language.ClearOptions();
            _language.AddOptions(names);
            SetLanguageIndex(selected);
        }

        public void SetLanguageIndex(int index)
        {
            _language.SetValueWithoutNotify(index);
            _language.RefreshShownValue();
        }
        
        public void SetQualityOptions(List<string> names)
        {
            _quality.ClearOptions();
            _quality.AddOptions(names);
        }

        public void SetVideoValues(int qualityIndex, bool vSyncEnabled, int screenModeIndex, int frameRateIndex)
        {
            _quality.SetValueWithoutNotify(qualityIndex);
            _quality.RefreshShownValue();

            _vSync.SetIsOnWithoutNotify(vSyncEnabled);

            _screenMode.SetValueWithoutNotify(screenModeIndex);
            _screenMode.RefreshShownValue();

            _frameRate.SetValueWithoutNotify(frameRateIndex);
            _frameRate.RefreshShownValue();
        }
        
        public void SetScreenModeOptions(List<string> names)
        {
            _screenMode.ClearOptions();
            _screenMode.AddOptions(names);
        }

        public void SetFrameRateOptions(List<string> names)
        {
            _frameRate.ClearOptions();
            _frameRate.AddOptions(names);
        }
        
        public void SetResolutionOptions(List<string> names, int selected)
        {
            _resolution.ClearOptions();
            _resolution.AddOptions(names);

            SetResolutionIndex(selected);
        }

        public void SetResolutionIndex(int index)
        {
            _resolution.SetValueWithoutNotify(index);
            _resolution.RefreshShownValue();
        }

        private void OnEnable()
        {
            _applyButton.onClick.AddListener(OnApplyClicked);
            _cancelButton.onClick.AddListener(OnCancelClicked);
            _resetButton.onClick.AddListener(OnResetClicked);
            
            _masterVolume.onValueChanged.AddListener(OnVolumeChanged);
            _musicVolume.onValueChanged.AddListener(OnVolumeChanged);
            _sfxVolume.onValueChanged.AddListener(OnVolumeChanged);
            _mouseSensitivity.onValueChanged.AddListener(OnSensitivityChanged);
            
            _language.onValueChanged.AddListener(OnLanguageChanged);
            
            _screenMode.onValueChanged.AddListener(OnVideoOptionChanged);
            _resolution.onValueChanged.AddListener(OnVideoOptionChanged);
            _frameRate.onValueChanged.AddListener(OnVideoOptionChanged);
            _quality.onValueChanged.AddListener(OnQualityChanged);
            _vSync.onValueChanged.AddListener(OnVSyncChanged);
            
        }

        private void OnDisable()
        {
            _applyButton.onClick.RemoveListener(OnApplyClicked);
            _cancelButton.onClick.RemoveListener(OnCancelClicked);
            _resetButton.onClick.RemoveListener(OnResetClicked);
            
            _masterVolume.onValueChanged.RemoveListener(OnVolumeChanged);
            _musicVolume.onValueChanged.RemoveListener(OnVolumeChanged);
            _sfxVolume.onValueChanged.RemoveListener(OnVolumeChanged);
            _mouseSensitivity.onValueChanged.RemoveListener(OnSensitivityChanged);
            
            _language.onValueChanged.RemoveListener(OnLanguageChanged);
            
            _screenMode.onValueChanged.RemoveListener(OnVideoOptionChanged);
            _resolution.onValueChanged.RemoveListener(OnVideoOptionChanged);
            _frameRate.onValueChanged.RemoveListener(OnVideoOptionChanged);
            _quality.onValueChanged.RemoveListener(OnQualityChanged);
            _vSync.onValueChanged.RemoveListener(OnVSyncChanged);
        }

        private void OnApplyClicked() => ApplyClicked?.Invoke();

        private void OnCancelClicked() => CancelClicked?.Invoke();
        
        private void OnVolumeChanged(float value) => ValuesChanged?.Invoke();

        private void OnSensitivityChanged(float value)
        {
            ValuesChanged?.Invoke();
        }
        
        private void OnLanguageChanged(int value) => ValuesChanged?.Invoke();
        
        private void OnQualityChanged(int value) => ValuesChanged?.Invoke();

        private void OnVSyncChanged(bool value) => ValuesChanged?.Invoke();
        
        private void OnVideoOptionChanged(int value) => ValuesChanged?.Invoke();
        
        private void OnResetClicked()
        {
            ResetClicked?.Invoke(_tabsView.CurrentIndex);
        }
    }
}
