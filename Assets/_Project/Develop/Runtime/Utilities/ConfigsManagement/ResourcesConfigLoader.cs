using System;
using System.Collections;
using System.Collections.Generic;
using _Project.Develop.Runtime.Configs;
using _Project.Develop.Runtime.Configs.Gameplay;
using _Project.Develop.Runtime.Configs.Gameplay.Characters;
using _Project.Develop.Runtime.Utilities.AssetsManagement;
using UnityEngine;

namespace _Project.Develop.Runtime.Utilities.ConfigsManagement
{
    public class ResourcesConfigsLoader : IConfigsLoader
    {
        private readonly ResourcesAssetsLoader _resources;

        private readonly Dictionary<Type, string> _configsResourcesPaths = new()
        {
            {typeof(InteractionConfig), "Configs/Gameplay/InteractionConfig"},
            {typeof(HeroMobilityConfig), "Configs/Gameplay/HeroMobilityConfig"},
            {typeof(StoryConfig), "Configs/Gameplay/StoryConfig"},
            {typeof(GameplayAudioConfig), "Configs/Gameplay/GameplayAudioConfig"},
            {typeof(ComicConfig), "Configs/ComicConfig"},
            {typeof(HeroDeathConfig), "Configs/Gameplay/HeroDeathConfig"},
            {typeof(GameSettingsConfig), "Configs/GameSettingsConfig" },
            {typeof(GameTimeConfig), "Configs/Gameplay/GameTimeConfig"},
            {typeof(DayScheduleConfig), "Configs/Gameplay/DayScheduleConfig"},
            {typeof(CharacterConfig), "Configs/Gameplay/Characters/RigidbodyCharacter"},
            
            {
                typeof(HeroSurvivalConfig),
                "Configs/Gameplay/HeroSurvivalConfig"
            },
        };

        public ResourcesConfigsLoader(ResourcesAssetsLoader resources)
        {
            _resources = resources;
        }

        public IEnumerator LoadAsync(Action<Dictionary<Type, object>> onConfigsLoaded)
        {
            Dictionary<Type, object> loadedConfigs = new();

            foreach (KeyValuePair<Type, string> configResourcesPath in _configsResourcesPaths)
            {
                ScriptableObject config = _resources.Load<ScriptableObject>(configResourcesPath.Value);
                loadedConfigs.Add(configResourcesPath.Key, config);
                yield return null;
            }

            onConfigsLoaded?.Invoke(loadedConfigs);
        }
    }
}
