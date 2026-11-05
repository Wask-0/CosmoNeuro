using System;
using _Project.Develop.Runtime.Configs;
using _Project.Develop.Runtime.Utilities.Audio;
using _Project.Develop.Runtime.Utilities.ConfigsManagement;
using _Project.Develop.Runtime.Utilities.DataManagement;
using _Project.Develop.Runtime.Utilities.DataManagement.DataProviders;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace _Project.Develop.Runtime.Utilities.Settings
{
    public class SettingsService : IDataReader<SettingsData>, IDataWriter<SettingsData>
    {
        public event Action<float> MouseSensitivityChanged;
        
        private const int MinResolutionWidth = 640;
        private const int MinResolutionHeight = 480;

        private readonly ConfigsProviderService _configsProviderService;
        private readonly AudioService _audioService;

        private GameSettingsConfig _defaultSettingsConfig;
        private SettingsData _data;

        public SettingsService(
            ConfigsProviderService configsProviderService,
            AudioService audioService)
        {
            _configsProviderService = configsProviderService;
            _audioService = audioService;
        }
        
        public float MouseSensitivity => _data.MouseSensitivity;

        public float MinMouseSensitivity =>
            _defaultSettingsConfig.MinMouseSensitivity;

        public float MaxMouseSensitivity =>
            _defaultSettingsConfig.MaxMouseSensitivity;
        
        public void Initialize()
        {
            _defaultSettingsConfig =
                _configsProviderService.GetConfig<GameSettingsConfig>();

            UpdateSettings(_defaultSettingsConfig.GetDefaultSettings());
        }

        public void ReadFrom(SettingsData settings)
        {
            UpdateSettings(settings);
            ApplySettings();
        }
        
        public void WriteTo(SettingsData data)
        {
            EnsureInitialized();

            if (data == null)
                throw new ArgumentNullException(nameof(data));

            data.ScreenMode = _data.ScreenMode;
            data.ResolutionWidth = _data.ResolutionWidth;
            data.ResolutionHeight = _data.ResolutionHeight;
            data.IsVSyncEnabled = _data.IsVSyncEnabled;
            data.FrameRateLimit = _data.FrameRateLimit;

            data.QualityIndex = _data.QualityIndex;

            data.MasterVolume = _data.MasterVolume;
            data.MusicVolume = _data.MusicVolume;
            data.SfxVolume = _data.SfxVolume;

            data.MouseSensitivity = _data.MouseSensitivity;
            
            data.LanguageCode = _data.LanguageCode;
        }

        public SettingsData GetSnapshot()
        {
            EnsureInitialized();

            return _data.Clone();
        }

        public void UpdateSettings(SettingsData settings)
        {
            EnsureInitialized();
            
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            if (!SettingsDataValidator.IsValid(settings))
                throw new ArgumentException(nameof(settings));

            SettingsData preparedSettings = settings.Clone();

            Normalize(preparedSettings);

            _data = preparedSettings;
        }

        public void ApplySettings()
        {
            EnsureInitialized();
            
            ApplyQualitySettings();
            ApplyScreenSettings();
            ApplyFrameRateSettings();
            ApplyAudioSettings();
            ApplyLanguage();
            
            NotifyMouseSensitivityChanged();
        }
        
        public SettingsData GetDefaultSettings()
        {
            EnsureInitialized();

            SettingsData defaults = _defaultSettingsConfig.GetDefaultSettings();

            Normalize(defaults);

            return defaults;
        }
        
        private void ApplyQualitySettings() => QualitySettings.SetQualityLevel(_data.QualityIndex);

        private void ApplyScreenSettings()
        {
            Screen.SetResolution(
                _data.ResolutionWidth,
                _data.ResolutionHeight,
                _data.ScreenMode);
        }

        private void ApplyFrameRateSettings()
        {
            QualitySettings.vSyncCount = _data.IsVSyncEnabled ? 1 : 0;
            Application.targetFrameRate = _data.IsVSyncEnabled ? -1 : (int)_data.FrameRateLimit;
        }
        
        private void ApplyAudioSettings()
        {
            _audioService.SetMasterVolume(_data.MasterVolume);
            _audioService.SetMusicVolume(_data.MusicVolume);
            _audioService.SetSoundVolume(_data.SfxVolume);
        }
        
        private void ApplyLanguage()
        {
            Locale locale = LocalizationSettings.AvailableLocales
                .GetLocale(_data.LanguageCode);

            if (locale == null)
                throw new InvalidOperationException(
                    $"Language '{_data.LanguageCode}' is unavailable.");

            if (LocalizationSettings.SelectedLocale != locale)
                LocalizationSettings.SelectedLocale = locale;
        }
        
        private void NotifyMouseSensitivityChanged() =>
            MouseSensitivityChanged?.Invoke(_data.MouseSensitivity);
        
        private void Normalize(SettingsData settings)
        {
            settings.ResolutionWidth = Mathf.Max(MinResolutionWidth, settings.ResolutionWidth);

            settings.ResolutionHeight = Mathf.Max(MinResolutionHeight, settings.ResolutionHeight);

            settings.QualityIndex = Mathf.Clamp(settings.QualityIndex, 0, QualitySettings.names.Length - 1);

            settings.MasterVolume = Mathf.Clamp01(settings.MasterVolume);
            settings.MusicVolume = Mathf.Clamp01(settings.MusicVolume);
            settings.SfxVolume = Mathf.Clamp01(settings.SfxVolume);

            settings.MouseSensitivity = Mathf.Clamp(settings.MouseSensitivity,
                _defaultSettingsConfig.MinMouseSensitivity,
                _defaultSettingsConfig.MaxMouseSensitivity);
            
            Locale locale = string.IsNullOrWhiteSpace(settings.LanguageCode)
                ? LocalizationSettings.SelectedLocale
                : LocalizationSettings.AvailableLocales.GetLocale(
                    settings.LanguageCode);

            locale ??= LocalizationSettings.SelectedLocale;

            if (locale == null)
                throw new InvalidOperationException(
                    "No localization language is available.");

            settings.LanguageCode = locale.Identifier.Code;
        }
        
        private void EnsureInitialized()
        {
            if (_defaultSettingsConfig == null)
                throw new InvalidOperationException("SettingsService must be initialized before use.");
        }
    }
}

