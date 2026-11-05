using System;
using System.IO;
using System.Threading.Tasks;
using _Project.Develop.Runtime.Utilities.DataManagement.DataRepository;
using _Project.Develop.Runtime.Utilities.DataManagement.KeysStorage;
using _Project.Develop.Runtime.Utilities.DataManagement.Serializers;

namespace _Project.Develop.Runtime.Utilities.DataManagement
{
    public class SaveLoadService : ISaveLoadService
    {
        private readonly IDataSerializer _serializer;
        private readonly IDataKeysStorage _keysStorage;
        private readonly IDataRepository _repository;

        public SaveLoadService(
            IDataSerializer serializer,
            IDataKeysStorage keysStorage,
            IDataRepository repository)
        {
            _serializer = serializer;
            _keysStorage = keysStorage;
            _repository = repository;
        }

        public async Task<LoadResult<TData>> LoadAsync<TData>(
            Func<TData, bool> validator = null) where TData : class, ISaveData
        {
            string key = _keysStorage.GetKeyFor<TData>();

            string serializedData;

            try
            {
                bool exists = await _repository.ExistsAsync(key);

                if (!exists)
                    return LoadResult<TData>.Failure(LoadStatus.NotFound);

                serializedData = await _repository.ReadAsync(key);
            }
            catch (FileNotFoundException exception)
            {
                return LoadResult<TData>.Failure( LoadStatus.NotFound, exception.Message);
            }
            catch (DirectoryNotFoundException exception)
            {
                return LoadResult<TData>.Failure( LoadStatus.NotFound, exception.Message);
            }
            catch (UnauthorizedAccessException exception)
            {
                return LoadResult<TData>.Failure(LoadStatus.Failed, exception.Message);
            }
            catch (IOException exception)
            {
                return LoadResult<TData>.Failure(LoadStatus.Failed, exception.Message);
            }

            return DeserializeData<TData>(serializedData, validator);
        }
        
        public async Task<SaveResult> SaveAsync<TData>(TData data) where TData : ISaveData
        {
            if (data is null) throw new ArgumentNullException(nameof(data));

            string key = _keysStorage.GetKeyFor<TData>();

            string serializedData;

            try
            {
                serializedData = _serializer.Serialize(data);
            }
            catch (Newtonsoft.Json.JsonException exception)
            {
                return SaveResult.Failure(exception.Message);
            }

            try
            {
                await _repository.WriteAsync(key, serializedData);
            }
            catch (UnauthorizedAccessException exception)
            {
                return SaveResult.Failure(exception.Message);
            }
            catch (IOException exception)
            {
                return SaveResult.Failure(exception.Message);
            }

            return SaveResult.Success();
        }
        
        public Task<bool> ExistsAsync<TData>() where TData : ISaveData
        {
            string key = _keysStorage.GetKeyFor<TData>();

            return _repository.ExistsAsync(key);
        }

        public Task RemoveAsync<TData>() where TData : ISaveData
        {
            string key = _keysStorage.GetKeyFor<TData>();

            return _repository.RemoveAsync(key);
        }
        
        private LoadResult<TData> DeserializeData<TData>(string serializedData,
            Func<TData, bool> validator) where TData : class, ISaveData
        {
            if (string.IsNullOrWhiteSpace(serializedData))
                return LoadResult<TData>.Failure(LoadStatus.InvalidData, "Save contains no data.");

            TData data;

            try
            {
                data = _serializer.Deserialize<TData>(serializedData);
            }
            catch (Newtonsoft.Json.JsonException exception)
            {
                return LoadResult<TData>.Failure(LoadStatus.InvalidData, exception.Message);
            }

            if (data == null)
                return LoadResult<TData>.Failure(LoadStatus.InvalidData, "Deserialized data is null.");

            if (validator != null && !validator(data))
                return LoadResult<TData>.Failure(LoadStatus.InvalidData, "Save data failed validation.");

            return LoadResult<TData>.Success(data);
        }
    }
}