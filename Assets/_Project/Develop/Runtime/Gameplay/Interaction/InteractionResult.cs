namespace _Project.Develop.Runtime.Gameplay.Interaction
{
    public enum InteractionStatus { None, Success, Unavailable, Rejected, Cancelled, Failed }

    public sealed class InteractionRejectedException : System.InvalidOperationException
    {
        public InteractionRejectedException(string message) : base(message) { }
    }

    public readonly struct InteractionResult
    {
        public InteractionStatus Status { get; }
        public bool IsSuccess => Status == InteractionStatus.Success;
        public string FailedMessage { get; }

        public InteractionResult(bool isSuccess, string failedMessage = null)
        {
            Status = isSuccess ? InteractionStatus.Success : InteractionStatus.Rejected;
            FailedMessage = failedMessage;
        }

        public InteractionResult(InteractionStatus status, string failedMessage = null)
        {
            Status = status;
            FailedMessage = failedMessage;
        }
    }
}
