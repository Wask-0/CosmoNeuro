using _Project.Develop.Runtime.Infrastructure.AppStates;
using UnityEngine;
using _Project.Develop.Runtime.UI;
using _Project.Develop.Runtime.UI.Core;
using _Project.Develop.Runtime.Utilities.Audio;
using _Project.Develop.Runtime.UI.LoadingScreen;
using _Project.Develop.Runtime.Infrastructure.DI;
using _Project.Develop.Runtime.Utilities.Settings;
using _Project.Develop.Runtime.Infrastructure.Scopes;
using _Project.Develop.Runtime.Utilities.Localization;
using _Project.Develop.Runtime.Utilities.DataManagement;
using _Project.Develop.Runtime.Utilities.Foundation.Text;
using _Project.Develop.Runtime.Utilities.SceneManagement;
using _Project.Develop.Runtime.Utilities.AssetsManagement;
using _Project.Develop.Runtime.Utilities.ConfigsManagement;
using _Project.Develop.Runtime.Utilities.CoroutinesManagement;
using _Project.Develop.Runtime.Utilities.DataManagement.KeysStorage;
using _Project.Develop.Runtime.Utilities.DataManagement.Serializers;
using _Project.Develop.Runtime.Utilities.DataManagement.DataProviders;
using _Project.Develop.Runtime.Utilities.DataManagement.DataRepository;
using _Project.Develop.Runtime.Utilities.Input.Common;
using Object = UnityEngine.Object;

namespace _Project.Develop.Runtime.Infrastructure.EntryPoint
{
    public class ProjectContextRegistrations
    {
        public static void Process(DIContainer container, string saveDirectory = null)
        {
            container.RegisterAsSingle(_ => new ProjectUnityObjects()).NonLazy();
            container.RegisterAsSingle(container => new AppStateService(container.Resolve<ICommonInput>()));
            
            //localization
            container.RegisterAsSingle<ITextLocalizer>( _ => new GameTextLocalizer()).NonLazy();
            
            //Common Utilities
            container.RegisterAsSingle<ICoroutinesPerformer>(CreateCoroutinesPerformer);
            container.RegisterAsSingle<ILoadingScreen>(CreateLoadingScreen);
            container.RegisterAsSingle(CreateConfigsProviderService);
            container.RegisterAsSingle(CreateResourcesAssetsLoader);
            container.RegisterAsSingle(CreateSceneSwitcherService);
            container.RegisterAsSingle(CreateSceneLoaderService);

            //Settings
            container.RegisterAsSingle(CreateSettingsService);
            container.RegisterAsSingle(CreateAudioService);
            
            //InputSystem
            container.RegisterAsSingle<ICommonInput>(CreateCommonInputReader).NonLazy();

            //Data Management
            container.RegisterAsSingle<ISaveLoadService>(CreateSaveLoadService);
            container.RegisterAsSingle(CreateSettingsDataProvider);
            container.RegisterAsSingle(CreatePlayerDataProvider);

            //UI
            container.RegisterAsSingle(CreateProjectPresentersFactory);
            container.RegisterAsSingle(CreateViewsFactory);
        }
        
        //Common Utilities
        private static CoroutinesPerformer CreateCoroutinesPerformer(DIContainer container)
        {
            ResourcesAssetsLoader resourcesAssetsLoader = container.Resolve<ResourcesAssetsLoader>();

            CoroutinesPerformer coroutinesPerformerPrefab = resourcesAssetsLoader
                .Load<CoroutinesPerformer>("Utilities/CoroutinesPerformer");

            return container.Resolve<ProjectUnityObjects>()
                .Own(Object.Instantiate(coroutinesPerformerPrefab));
        }
        
        private static StandardLoadingScreen CreateLoadingScreen(DIContainer container)
        {
            ResourcesAssetsLoader resourcesAssetsLoader = container.Resolve<ResourcesAssetsLoader>();

            StandardLoadingScreen standardLoadingScreenPrefab = resourcesAssetsLoader
                .Load<StandardLoadingScreen>("Utilities/StandardLoadingScreen");

            return container.Resolve<ProjectUnityObjects>()
                .Own(Object.Instantiate(standardLoadingScreenPrefab));
        }
        
        private static ConfigsProviderService CreateConfigsProviderService(DIContainer container)
        {
            ResourcesAssetsLoader resourcesAssetsLoader = container.Resolve<ResourcesAssetsLoader>();

            ResourcesConfigsLoader resourcesConfigsLoader = new ResourcesConfigsLoader(resourcesAssetsLoader);

            return new ConfigsProviderService(resourcesConfigsLoader);
        }
        
        private static ResourcesAssetsLoader CreateResourcesAssetsLoader(DIContainer container)
            => new ResourcesAssetsLoader();
        
        private static SceneSwitcherService CreateSceneSwitcherService(DIContainer container)
            => new SceneSwitcherService(
                container.Resolve<SceneLoaderService>(),
                container.Resolve<ILoadingScreen>(),
                container,
                container.Resolve<AppStateService>());
        
        private static SceneLoaderService CreateSceneLoaderService(DIContainer container)
            => new SceneLoaderService();
        
        
        //Settings
        private static SettingsService CreateSettingsService(DIContainer container)
            => new SettingsService(
                container.Resolve<ConfigsProviderService>(),
                container.Resolve<AudioService>());

        private static AudioService CreateAudioService(DIContainer container)
        {
            ResourcesAssetsLoader resourcesAssetsLoader = container.Resolve<ResourcesAssetsLoader>();

            AudioService audioServicePrefab = resourcesAssetsLoader
                .Load<AudioService>("Utilities/AudioService");

            return container.Resolve<ProjectUnityObjects>()
                .Own(Object.Instantiate(audioServicePrefab));
        }
        
        //InputSystem
        private static CommonInputReader CreateCommonInputReader(DIContainer container)
            => new CommonInputReader();
        
        //Data Management
        private static SaveLoadService CreateSaveLoadService(DIContainer container)
        {
            IDataSerializer dataSerializer = new JsonSerializer();
            IDataKeysStorage dataKeysStorage = new MapDataKeysStorage();

            string saveFolderPath = Application.isEditor 
                    ? UnityEngine.Application.dataPath 
                    : UnityEngine.Application.persistentDataPath;

            IDataRepository dataRepository = new LocalFileDataRepository(saveFolderPath, "json");

            return new SaveLoadService(dataSerializer, dataKeysStorage, dataRepository);
        }
        
        private static SettingsDataProvider CreateSettingsDataProvider(DIContainer container)
        {
            SettingsDataProvider provider = new SettingsDataProvider(
                container.Resolve<ISaveLoadService>(),
                container.Resolve<ConfigsProviderService>());

            SettingsService settingsService =
                container.Resolve<SettingsService>();

            provider.RegisterReader(settingsService);
            provider.RegisterWriter(settingsService);

            return provider;
        }
        
        private static PlayerDataProvider CreatePlayerDataProvider(DIContainer container)
            => new PlayerDataProvider(container.Resolve<ISaveLoadService>(),
                container.Resolve<ConfigsProviderService>());
        
        //UI
        private static ViewsFactory CreateViewsFactory(DIContainer container)
            => new ViewsFactory(container.Resolve<ResourcesAssetsLoader>());

        private static ProjectPresentersFactory CreateProjectPresentersFactory(DIContainer container)
            => new ProjectPresentersFactory(container);
    }
}
