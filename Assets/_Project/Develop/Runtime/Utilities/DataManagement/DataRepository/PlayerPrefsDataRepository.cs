using UnityEngine;
using System.Threading.Tasks;

namespace _Project.Develop.Runtime.Utilities.DataManagement.DataRepository
{
    public class PlayerPrefsDataRepository : IDataRepository
    {
        public Task<bool> ExistsAsync(string key)
        {
            bool exists = PlayerPrefs.HasKey(key);

            return Task.FromResult(exists);
        }

        public Task<string> ReadAsync(string key)
        {
            string text = PlayerPrefs.GetString(key);

            return Task.FromResult(text);
        }

        public Task RemoveAsync(string key)
        {
            PlayerPrefs.DeleteKey(key);
            PlayerPrefs.Save();
            
            return Task.CompletedTask;
        }

        public Task WriteAsync(string key, string serializedData)
        {
            PlayerPrefs.SetString(key, serializedData);
            PlayerPrefs.Save();

            return Task.CompletedTask;
        }
    }
}