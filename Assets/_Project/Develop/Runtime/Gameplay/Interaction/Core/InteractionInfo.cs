namespace _Project.Develop.Runtime.Gameplay.Interaction.Core
{
    public readonly struct InteractionInfo
    {
        public string ActionText { get; }
        public bool IsAvailable { get; }
        public InteractionType Type { get; }
        public bool CanExecute { get; }
        public string SecondaryActionText { get; }
        public string PrimaryKey { get; }

        public InteractionInfo(
            string actionText,
            bool isAvailable,
            InteractionType type = InteractionType.Default,
            bool canExecute = true,
            string secondaryActionText = null,
            string primaryKey = "E")
        {
            ActionText = actionText;
            IsAvailable = isAvailable;
            Type = type;
            CanExecute = isAvailable && canExecute;
            SecondaryActionText = secondaryActionText;
            PrimaryKey = primaryKey;
        }
        
        public static InteractionInfo Unavailable =>
            new InteractionInfo(string.Empty, false);
    }
}
