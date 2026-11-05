namespace _Project.Develop.Runtime.Utilities.DataManagement.DataProviders
{
    public interface IDataReader<TData> where TData : ISaveData
    {
        void ReadFrom(TData data);
    }
}
