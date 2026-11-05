using _Project.Develop.Runtime.Gameplay.Interaction.Core;

namespace _Project.Develop.Runtime.Gameplay.Interaction.Requirements
{
    public interface  IInteractionRequirement
    {
        bool IsSatisfied(InteractionContext context);
    }
}