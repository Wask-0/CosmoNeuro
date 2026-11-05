using _Project.Develop.Runtime.Configs;
using _Project.Develop.Runtime.Utilities.ConfigsManagement;
using _Project.Develop.Runtime.Utilities.Settings;

namespace _Project.Develop.Runtime.Utilities.DataManagement.DataProviders
{
    public class SettingsDataProvider : DataProvider<SettingsData>
    {
        private readonly ConfigsProviderService _configsProviderService;

        public SettingsDataProvider(
            ISaveLoadService saveLoadService,
            ConfigsProviderService configsProviderService) : base(saveLoadService)
        {
            _configsProviderService = configsProviderService;
        }

        protected override SettingsData GetOriginData()
        {
            GameSettingsConfig config = _configsProviderService.GetConfig<GameSettingsConfig>();

            return config.GetDefaultSettings();
        }
        
        protected override bool IsValid(SettingsData data)
        {
            return SettingsDataValidator.IsValid(data);
        }
    }
}