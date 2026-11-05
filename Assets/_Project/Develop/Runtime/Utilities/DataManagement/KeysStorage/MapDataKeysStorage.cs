using System;
using System.Collections.Generic;

namespace _Project.Develop.Runtime.Utilities.DataManagement.KeysStorage
{
    public class MapDataKeysStorage : IDataKeysStorage
    {
        private readonly Dictionary<Type, string> Keys = new Dictionary<Type, string>()
        {
            {typeof(PlayerData), "PlayerData" },
            { typeof(SettingsData), "GameSettingsData" },
        };

        public string GetKeyFor<TData>() where TData : ISaveData
            => Keys[typeof(TData)];
    }
}
