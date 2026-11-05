using System;
using System.Collections.Generic;
using _Project.Develop.Runtime.UI.Core.Popup;
using _Project.Develop.Runtime.Utilities.CoroutinesManagement;
using _Project.Develop.Runtime.Utilities.DataManagement;
using _Project.Develop.Runtime.Utilities.DataManagement.DataProviders;
using _Project.Develop.Runtime.Utilities.Input.Common;
using _Project.Develop.Runtime.Utilities.Settings;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Linq;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace _Project.Develop.Runtime.UI.Settings
{
    public class SettingsPopupPresenter : PopupPresenterBase
    {
        private readonly SettingsPopupView _view;
        private readonly SettingsService _settings;
        private readonly SettingsDataProvider _dataProvider;
        private readonly ICommonInput _input;

        private bool _saving;
        private bool _closing;
        private bool _disposed;

        private SettingsData _appliedSettings;

        private string[] _languageCodes;

        private readonly FullScreenMode[] _screenModes =
        {
            FullScreenMode.FullScreenWindow,
            FullScreenMode.Windowed
        };

        private readonly FrameRateLimit[] _frameRates =
        {
            FrameRateLimit.FPS30,
            FrameRateLimit.FPS60,
            FrameRateLimit.FPS120,
            FrameRateLimit.FPS144,
            FrameRateLimit.FPS240,
            FrameRateLimit.Unlimited
        };

        private FullScreenMode SelectedScreenMode => _screenModes[_view.ScreenModeIndex];

        private FrameRateLimit SelectedFrameRate => _frameRates[_view.FrameRateIndex];

        private string SelectedLanguageCode => _languageCodes[_view.LanguageIndex];

        public SettingsPopupPresenter(
            ICoroutinesPerformer coroutines,
            SettingsPopupView view,
            SettingsService settings,
            SettingsDataProvider dataProvider,
            ICommonInput input)
            : base(coroutines)
        {
            _view = view;
            _settings = settings;
            _dataProvider = dataProvider;
            _input = input;
        }

        private Vector2Int[] _resolutions;

        private Vector2Int SelectedResolution => _resolutions[_view.ResolutionIndex];

        protected override PopupViewBase PopupView => _view;

        public GameObject FirstSelected => _view.FirstSelected;

        public override void Initialize()
        {
            base.Initialize();

            _appliedSettings = _settings.GetSnapshot();

            _view.SetMouseSensitivityRange(
                _settings.MinMouseSensitivity,
                _settings.MaxMouseSensitivity);

            _view.SetMouseSensitivity(_appliedSettings.MouseSensitivity);
            
            InitializeResolutions();

            _view.SetQualityOptions(QualitySettings.names.ToList());

            _view.SetScreenModeOptions(new List<string>
            {
                LocalizationSettings.StringDatabase.GetLocalizedString(
                    "UI", "Settings.ScreenMode.Fullscreen"),

                LocalizationSettings.StringDatabase.GetLocalizedString(
                    "UI", "Settings.ScreenMode.Windowed")
            });

            _view.SetFrameRateOptions(new List<string>
            {
                "30",
                "60",
                "120",
                "144",
                "240",

                LocalizationSettings.StringDatabase.GetLocalizedString(
                    "UI", "Settings.FrameRate.Unlimited")
            });

            RefreshVideoValues();

            List<Locale> locales = LocalizationSettings.AvailableLocales.Locales;

            _languageCodes = locales.Select(locale => locale.Identifier.Code).ToArray();

            List<string> names = locales.Select(locale => locale.LocaleName).ToList();

            int selected = Array.IndexOf(_languageCodes, _appliedSettings.LanguageCode);

            _view.SetLanguages(names, Mathf.Max(0, selected));

            _view.SetValues(
                _appliedSettings.MasterVolume,
                _appliedSettings.MusicVolume,
                _appliedSettings.SfxVolume);

            _view.ApplyClicked += OnApplyClicked;
            _view.CancelClicked += OnCancelClicked;
            _input.CancelPressed += OnCancelClicked;
            _view.ResetClicked += OnResetClicked;

            _view.ValuesChanged += RefreshApplyButton;

            RefreshApplyButton();
        }

        public override void Dispose()
        {
            _disposed = true;

            _view.ApplyClicked -= OnApplyClicked;
            _view.CancelClicked -= OnCancelClicked;
            _input.CancelPressed -= OnCancelClicked;
            _view.ResetClicked -= OnResetClicked;

            _view.ValuesChanged -= RefreshApplyButton;

            base.Dispose();
        }

        private async void OnApplyClicked()
        {
            if (_disposed || _saving || _closing || !HasChanges)
                return;

            _saving = true;
            _view.SetInteractable(false);

            SettingsData previous = null;
            bool settingsUpdated = false;

            bool appliedSuccessfully = false;

            try
            {
                previous = _settings.GetSnapshot();

                SettingsData edited = previous.Clone();

                edited.MasterVolume = _view.MasterVolume;
                edited.MusicVolume = _view.MusicVolume;
                edited.SfxVolume = _view.SfxVolume;
                edited.MouseSensitivity = _view.MouseSensitivity;

                edited.LanguageCode = SelectedLanguageCode;

                Vector2Int resolution = SelectedResolution;

                edited.ResolutionWidth = resolution.x;
                edited.ResolutionHeight = resolution.y;

                edited.ScreenMode = SelectedScreenMode;
                edited.FrameRateLimit = SelectedFrameRate;
                edited.QualityIndex = _view.QualityIndex;
                edited.IsVSyncEnabled = _view.IsVSyncEnabled;

                _settings.UpdateSettings(edited);
                settingsUpdated = true;

                _settings.ApplySettings();

                SaveResult result = await _dataProvider.SaveAsync();

                if (!result.IsSuccess)
                    throw new InvalidOperationException(
                        $"Could not save settings: {result.Error}");

                _appliedSettings = _settings.GetSnapshot();

                appliedSuccessfully = true;

                if (!_disposed)
                {
                    _view.SetMouseSensitivity(_appliedSettings.MouseSensitivity);

                    _view.SetValues(
                        _appliedSettings.MasterVolume,
                        _appliedSettings.MusicVolume,
                        _appliedSettings.SfxVolume);

                    _view.SetLanguageIndex(
                        Array.IndexOf(_languageCodes, _appliedSettings.LanguageCode));

                    RefreshVideoValues();
                }
            }
            catch (Exception exception)
            {
                if (settingsUpdated)
                {
                    try
                    {
                        _settings.UpdateSettings(previous);
                        _settings.ApplySettings();
                    }
                    catch (Exception restoreException)
                    {
                        Debug.LogException(restoreException);
                    }
                }

                Debug.LogException(exception);
            }
            finally
            {
                _saving = false;

                if (!_disposed)
                {
                    _view.SetInteractable(true);
                    RefreshApplyButton();

                    if (appliedSuccessfully && EventSystem.current != null)
                    {
                        EventSystem.current.SetSelectedGameObject(
                            _view.AfterApplySelected);
                    }
                }
            }
        }

        private void OnCancelClicked()
        {
            if (_disposed || _saving || _closing)
                return;

            RequestClose();
        }

        private void RequestClose()
        {
            _closing = true;

            OnCloseRequest();
        }

        private bool HasChanges
            => !Mathf.Approximately(_view.MasterVolume, _appliedSettings.MasterVolume)
               || !Mathf.Approximately(_view.MusicVolume, _appliedSettings.MusicVolume)
               || !Mathf.Approximately(_view.SfxVolume, _appliedSettings.SfxVolume)
               || SelectedLanguageCode != _appliedSettings.LanguageCode
               || _view.QualityIndex != _appliedSettings.QualityIndex
               || _view.IsVSyncEnabled != _appliedSettings.IsVSyncEnabled
               || SelectedScreenMode != _appliedSettings.ScreenMode
               || SelectedFrameRate != _appliedSettings.FrameRateLimit
               || SelectedResolution.x != _appliedSettings.ResolutionWidth
               || SelectedResolution.y != _appliedSettings.ResolutionHeight
               || !Mathf.Approximately(
                   _view.MouseSensitivity,
                   _appliedSettings.MouseSensitivity);

        private void RefreshApplyButton()
        {
            if (_disposed)
                return;

            _view.SetApplyAvailable(
                !_saving && !_closing && HasChanges);
        }

        private void RefreshVideoValues()
        {
            _view.SetVideoValues(
                _appliedSettings.QualityIndex,
                _appliedSettings.IsVSyncEnabled,
                Mathf.Max(0, Array.IndexOf(_screenModes, _appliedSettings.ScreenMode)),
                Mathf.Max(0, Array.IndexOf(_frameRates, _appliedSettings.FrameRateLimit)));

            _view.SetResolutionIndex(
                Array.IndexOf(_resolutions, new Vector2Int(
                    _appliedSettings.ResolutionWidth,
                    _appliedSettings.ResolutionHeight)));
        }

        private void InitializeResolutions()
        {
            Vector2Int savedResolution =
                new Vector2Int(_appliedSettings.ResolutionWidth, _appliedSettings.ResolutionHeight);

            _resolutions = Screen.resolutions
                .Select(resolution => new Vector2Int(resolution.width, resolution.height))
                .Append(savedResolution)
                .Distinct()
                .OrderBy(resolution => resolution.x)
                .ThenBy(resolution => resolution.y)
                .ToArray();

            List<string> names = _resolutions
                .Select(resolution => $"{resolution.x} × {resolution.y}").ToList();

            _view.SetResolutionOptions(names, Array.IndexOf(_resolutions, savedResolution));
        }
        
        private void OnResetClicked(int tabIndex)
        {
            if (_disposed || _saving || _closing)
                return;

            SettingsData defaults = _settings.GetDefaultSettings();

            switch (tabIndex)
            {
                case 0: // Общее
                    _view.SetLanguageIndex(Array.IndexOf(_languageCodes, defaults.LanguageCode));
                    break;

                case 1: // Видео
                {
                    Vector2Int resolution = new Vector2Int(
                        defaults.ResolutionWidth,
                        defaults.ResolutionHeight);

                    int resolutionIndex = Array.IndexOf(_resolutions, resolution);

                    // Стандартное разрешение может отсутствовать в текущем списке.
                    if (resolutionIndex < 0)
                    {
                        _resolutions = _resolutions
                            .Append(resolution)
                            .OrderBy(item => item.x)
                            .ThenBy(item => item.y)
                            .ToArray();

                        resolutionIndex = Array.IndexOf(_resolutions, resolution);

                        _view.SetResolutionOptions(_resolutions
                                .Select(item => $"{item.x} × {item.y}")
                                .ToList(), resolutionIndex);
                    }
                    else
                    {
                        _view.SetResolutionIndex(resolutionIndex);
                    }

                    _view.SetVideoValues(
                        defaults.QualityIndex,
                        defaults.IsVSyncEnabled,
                        Mathf.Max(0, Array.IndexOf(
                            _screenModes, defaults.ScreenMode)),
                        Mathf.Max(0, Array.IndexOf(
                            _frameRates, defaults.FrameRateLimit)));
                    break;
                }

                case 2: // Звук
                    _view.SetValues(
                        defaults.MasterVolume,
                        defaults.MusicVolume,
                        defaults.SfxVolume);
                    break;

                case 3: // Управление
                    _view.SetMouseSensitivity(defaults.MouseSensitivity);
                    break;

                default:
                    // Переназначение управления ещё не подключено.
                    return;
            }

            RefreshApplyButton();
        }
    }
}
