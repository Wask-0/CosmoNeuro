using System.Threading;
using System.Threading.Tasks;

namespace _Project.Develop.Runtime.Gameplay.Interaction.Core
{
    public interface ISecondaryInteractable
    {
        string SecondaryActionText { get; }
        Task<InteractionResult> InteractSecondaryAsync(InteractionContext context, CancellationToken cancellationToken);
    }
}
