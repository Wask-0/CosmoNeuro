namespace _Project.Develop.Runtime.Utilities.DataManagement.DataProviders
{
    public interface IDataWriter<TData> where TData : ISaveData
    {
        void WriteTo(TData data);
    }
}
