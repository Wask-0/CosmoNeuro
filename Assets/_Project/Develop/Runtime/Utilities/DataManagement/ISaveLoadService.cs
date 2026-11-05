using System;
using System.Collections;
using System.Threading.Tasks;

namespace _Project.Develop.Runtime.Utilities.DataManagement
{
    public interface ISaveLoadService
    {
        Task<LoadResult<TData>> LoadAsync<TData>(
            Func<TData, bool> validator = null) where TData : class, ISaveData;

        Task<SaveResult> SaveAsync<TData>(TData data) where TData : ISaveData;

        Task RemoveAsync<TData>() where TData : ISaveData;

        Task<bool> ExistsAsync<TData>() where TData : ISaveData;
    }
}
