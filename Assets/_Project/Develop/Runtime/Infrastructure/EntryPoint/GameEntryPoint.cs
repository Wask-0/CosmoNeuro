using _Project.Develop.Runtime.Infrastructure.Scopes;
using System;
using SceneNames = _Project.Develop.Runtime.Utilities.SceneManagement.Scenes;
using System.Collections;
using System.Threading.Tasks;
using _Project.Develop.Runtime.UI.LoadingScreen;
using _Project.Develop.Runtime.Utilities.SceneManagement;
using _Project.Develop.Runtime.Utilities.Settings;
using _Project.Develop.Runtime.Infrastructure.DI;
using _Project.Develop.Runtime.Utilities.ConfigsManagement;
using _Project.Develop.Runtime.Utilities.CoroutinesManagement;
using _Project.Develop.Runtime.Utilities.DataManagement;
using _Project.Develop.Runtime.Utilities.DataManagement.DataProviders;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace _Project.Develop.Runtime.Infrastructure.EntryPoint
{
    public class GameEntryPoint : MonoBehaviour
    {
        private DIContainer _projectContainer;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            
            Debug.Log("Процесс регистрации сервисов всего проекта");

            _projectContainer = new DIContainer();

            ProjectContextRegistrations.Process(_projectContainer);

            _projectContainer.Initialize();

            _projectContainer.Resolve<ICoroutinesPerformer>().StartPerform(Initialize(_projectContainer));
        }

        private IEnumerator Initialize(DIContainer container)
        {
            ILoadingScreen loadingScreen = container.Resolve<ILoadingScreen>();
            SceneSwitcherService sceneSwitcherService = container.Resolve<SceneSwitcherService>();
            PlayerDataProvider playerDataProvider = container.Resolve<PlayerDataProvider>();

            loadingScreen.Show();

            Debug.Log("Начинается инициализация сервисов");

            yield return container.Resolve<ConfigsProviderService>().LoadAsync();

            yield return SetupAppSettings();

            yield return new WaitForSeconds(2f);

            Debug.Log("Завершается инициализация сервисов");

            loadingScreen.Hide();

            yield return sceneSwitcherService.ProcessSwitchTo(SceneNames.MainScene);
        }

        private IEnumerator SetupAppSettings()
        {
            SettingsService settingsService = _projectContainer.Resolve<SettingsService>();

            SettingsDataProvider settingsDataProvider = _projectContainer.Resolve<SettingsDataProvider>();
            
            yield return WaitForLocalization();

            settingsService.Initialize();

            Task<LoadResult<SettingsData>> loadTask = settingsDataProvider.LoadAsync();

            while (!loadTask.IsCompleted)
                yield return null;

            LoadResult<SettingsData> result = loadTask.GetAwaiter().GetResult();

            if (result.Status == LoadStatus.Failed)
                throw new InvalidOperationException($"Settings could not be loaded. {result.Error}");

            if (result.Status == LoadStatus.InvalidData)
                Debug.LogWarning($"Saved settings are invalid; defaults are used. {result.Error}");
            
            yield return WaitForLocalization();
        }
        
        private void OnDestroy()
        {
            DIContainer container = _projectContainer;
            _projectContainer = null;

            container?.Dispose();
        }
        
        private IEnumerator WaitForLocalization()
        {
            AsyncOperationHandle<LocalizationSettings> operation = LocalizationSettings.InitializationOperation;

            if (!operation.IsDone)
                yield return operation;

            if (operation.Status != AsyncOperationStatus.Succeeded)
                throw new InvalidOperationException(
                    "Localization initialization failed.", operation.OperationException);
        }
    }
}