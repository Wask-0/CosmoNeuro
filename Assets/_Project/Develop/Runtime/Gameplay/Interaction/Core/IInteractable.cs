using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Interaction.Core
{
    public interface  IInteractable
    {
        Transform InteractionPoint { get; }
        
        InteractionInfo GetInteractionInfo(InteractionContext context);

        Task<InteractionResult> InteractAsync(InteractionContext context, CancellationToken cancellationToken);
    }
}