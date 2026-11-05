using System;

namespace _Project.Develop.Runtime.Utilities.DataManagement
{
    public enum LoadStatus
    {
        Success,
        NotFound,
        InvalidData,
        Failed
    }
    
    public class LoadResult<TData>where TData : class, ISaveData
    {
        public LoadStatus Status { get; }
        public TData Data { get; }
        public string Error { get; }

        public bool IsSuccess => Status == LoadStatus.Success;

        private LoadResult(LoadStatus status, TData data, string error)
        {
            Status = status;
            Data = data;
            Error = error;
        }

        public static LoadResult<TData> Success(TData data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            return new LoadResult<TData>(LoadStatus.Success, data, null);
        }

        public static LoadResult<TData> Failure(LoadStatus status, string error = null)
        {
            if (status == LoadStatus.Success)
                throw new ArgumentException("Use Success to create a successful result.", nameof(status));

            return new LoadResult<TData>(status, null, error);
        }
    }
}