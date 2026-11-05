using System;
using System.Collections;
using System.Threading.Tasks;

namespace _Project.Develop.Runtime.Utilities.DataManagement.DataRepository
{
    public interface IDataRepository
    {
        Task<string> ReadAsync(string key);
        Task WriteAsync(string key, string serializedData);
        Task RemoveAsync(string key);
        Task<bool> ExistsAsync(string key);
    }
}
