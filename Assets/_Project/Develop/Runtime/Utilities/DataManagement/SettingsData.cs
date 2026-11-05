using _Project.Develop.Runtime.Utilities.Settings;
using UnityEngine;

namespace _Project.Develop.Runtime.Utilities.DataManagement
{
    public class SettingsData : ISaveData
    {
        //Language
        public string LanguageCode;
        
        //Screen
        public FullScreenMode ScreenMode;
        public int ResolutionWidth;
        public int ResolutionHeight;
        public bool IsVSyncEnabled;
        public FrameRateLimit FrameRateLimit;

        //Graphics
        public int QualityIndex;

        //Audio
        public float MasterVolume;
        public float MusicVolume;
        public float SfxVolume;

        //Controller
        public float MouseSensitivity;

        public SettingsData Clone()
        {
            return new SettingsData
            {
                ScreenMode = ScreenMode,
                ResolutionWidth = ResolutionWidth,
                ResolutionHeight = ResolutionHeight,
                IsVSyncEnabled = IsVSyncEnabled,
                FrameRateLimit = FrameRateLimit,
                QualityIndex = QualityIndex,
                MasterVolume = MasterVolume,
                MusicVolume = MusicVolume,
                SfxVolume = SfxVolume,
                MouseSensitivity = MouseSensitivity,
                LanguageCode = LanguageCode,
            };
        }
    }
}