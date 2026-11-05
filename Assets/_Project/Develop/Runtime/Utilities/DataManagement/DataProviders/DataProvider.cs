using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace _Project.Develop.Runtime.Utilities.DataManagement.DataProviders
{
    public abstract class DataProvider<TData> where TData : class, ISaveData
    {
        private readonly ISaveLoadService _saveLoadService;

        private readonly List<IDataWriter<TData>> _writers = new();
        private readonly List<IDataReader<TData>> _readers = new();

        private TData _data;
        
        private readonly SemaphoreSlim _operationGate = new(1, 1);

        protected DataProvider(ISaveLoadService saveLoadService)
        {
            _saveLoadService = saveLoadService ?? throw new ArgumentNullException(nameof(saveLoadService));
        }

        public void RegisterWriter(IDataWriter<TData> writer)
        {
            if (writer == null)
                throw new ArgumentNullException(nameof(writer));

            if (_writers.Contains(writer))
                throw new ArgumentException(
                    "Writer is already registered.", nameof(writer));

            _writers.Add(writer);
        }

        public void RegisterReader(IDataReader<TData> reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            if (_readers.Contains(reader))
                throw new ArgumentException(
                    "Reader is already registered.", nameof(reader));

            _readers.Add(reader);
        }

        public async Task<LoadResult<TData>> LoadAsync()
        {
            await _operationGate.WaitAsync();

            try
            {
                LoadResult<TData> result =
                    await _saveLoadService.LoadAsync<TData>(IsValid);
                
                if (result.Status == LoadStatus.Failed)
                    return result;

                TData preparedData = result.IsSuccess ? result.Data : GetOriginData();

                if (!IsValid(preparedData))
                    throw new InvalidOperationException("Data provider produced invalid data.");

                _data = preparedData;

                SendDataToReaders();

                return result;
            }
            finally
            {
                _operationGate.Release();
            }
        }

        public async Task<SaveResult> SaveAsync()
        {
            await _operationGate.WaitAsync();
            
            try
            {
                if (_data == null)
                    throw new InvalidOperationException("Data must be loaded or reset before saving.");

                UpdateDataFromWriters();

                if (!IsValid(_data))
                    throw new InvalidOperationException("Cannot save invalid data.");

                return await _saveLoadService.SaveAsync(_data);
            }
            finally
            {
                _operationGate.Release();
            }
        }

        public Task<bool> ExistsAsync()
        {
            return _saveLoadService.ExistsAsync<TData>();
        }

        public async Task ResetAsync()
        {
            await _operationGate.WaitAsync();

            try
            {
                TData defaultData = GetOriginData();

                if (!IsValid(defaultData))
                    throw new InvalidOperationException(
                        "Data provider produced invalid default data.");

                _data = defaultData;

                SendDataToReaders();
            }
            finally
            {
                _operationGate.Release();
            }
        }

        protected abstract TData GetOriginData();
        
        protected virtual bool IsValid(TData data)
        {
            return data != null;
        }

        private void SendDataToReaders()
        {
            foreach (IDataReader<TData> reader in _readers)
                reader.ReadFrom(_data);
        }

        private void UpdateDataFromWriters()
        {
            foreach (IDataWriter<TData> writer in _writers)
                writer.WriteTo(_data);
        }
    }
}