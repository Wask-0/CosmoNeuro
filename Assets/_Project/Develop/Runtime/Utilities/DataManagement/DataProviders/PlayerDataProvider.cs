using _Project.Develop.Runtime.Utilities.ConfigsManagement;

namespace _Project.Develop.Runtime.Utilities.DataManagement.DataProviders
{
    public class PlayerDataProvider : DataProvider<PlayerData>
    {
        private readonly ConfigsProviderService _configsProviderService;

        public PlayerDataProvider(
            ISaveLoadService saveLoadService, 
            ConfigsProviderService configsProviderService) : base(saveLoadService)
        {
            _configsProviderService = configsProviderService;
        }

        protected override PlayerData GetOriginData()
        {
            return new PlayerData()
            {
                //WalletData = InitWalletData(),
            };
        }

        // private Dictionary<CurrencyTypes, int> InitWalletData()
        // {
        //     Dictionary<CurrencyTypes, int> walletData = new();
        //
        //     StartWalletConfig walletConfig = _configsProviderService.GetConfig<StartWalletConfig>();
        //
        //     foreach (CurrencyTypes currencyType in Enum.GetValues(typeof(CurrencyTypes)))
        //         walletData[currencyType] = walletConfig.GetValueFor(currencyType);
        //
        //     return walletData;
        // }
    }
}