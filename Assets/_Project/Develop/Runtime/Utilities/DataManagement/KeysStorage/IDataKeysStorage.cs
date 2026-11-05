namespace _Project.Develop.Runtime.Utilities.DataManagement.KeysStorage
{
    public interface IDataKeysStorage
    {
        string GetKeyFor<TData>() where TData : ISaveData;
    }
}