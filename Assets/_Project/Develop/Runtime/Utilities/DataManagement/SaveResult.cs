namespace _Project.Develop.Runtime.Utilities.DataManagement
{
    public sealed class SaveResult
    {
        public bool IsSuccess { get; }
        public string Error { get; }

        private SaveResult(bool isSuccess, string error)
        {
            IsSuccess = isSuccess;
            Error = error;
        }

        public static SaveResult Success()
        {
            return new SaveResult(true, null);
        }

        public static SaveResult Failure(string error)
        {
            return new SaveResult(false, error);
        }
    }
}