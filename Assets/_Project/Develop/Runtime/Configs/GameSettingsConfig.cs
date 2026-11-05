using _Project.Develop.Runtime.Utilities.DataManagement;
using _Project.Develop.Runtime.Utilities.Settings;
using UnityEngine;

namespace _Project.Develop.Runtime.Configs
{
    [CreateAssetMenu(fileName = "GameSettingsConfig", menuName = "Configs/NewGameSettingsConfig")]
    public class GameSettingsConfig : ScriptableObject
    {
        [field: SerializeField] public string DefaultLanguageCode { get; private set; } = "en";
        
        [Header("Screen")] 
        [field: SerializeField] public FullScreenMode DefaultScreenMode {get; private set; }
        [field: SerializeField] public bool DefaultVSync {get; private set; }
        [field: SerializeField] public FrameRateLimit DefaultFrameRateLimit {get; private set;}

        [Header("Graphics")]
        [field: SerializeField] public int DefaultQualityIndex {get; private set;}

        [Header("Audio")] 
        [field: SerializeField] public float DefaultMasterVolume {get; private set;}
        [field: SerializeField] public float DefaultMusicVolume {get; private set;}
        [field: SerializeField] public float DefaultSfxVolume {get; private set;} 

        [Header("Controls")] 
        [field: SerializeField] public float DefaultMouseSensitivity {get; private set;}
        
        [field: SerializeField] public float MinMouseSensitivity {get; private set;}
        [field: SerializeField] public float MaxMouseSensitivity {get; private set;}

        public SettingsData GetDefaultSettings()
        {
            return new SettingsData
            {
                ScreenMode = DefaultScreenMode,
                ResolutionWidth = Screen.currentResolution.width,
                ResolutionHeight = Screen.currentResolution.height,
                IsVSyncEnabled = DefaultVSync,
                FrameRateLimit = DefaultFrameRateLimit,
                QualityIndex = DefaultQualityIndex,
                MasterVolume = DefaultMasterVolume,
                MusicVolume = DefaultMusicVolume,
                SfxVolume = DefaultSfxVolume,
                MouseSensitivity = DefaultMouseSensitivity,
                LanguageCode = DefaultLanguageCode,
            };
        }
    }
}