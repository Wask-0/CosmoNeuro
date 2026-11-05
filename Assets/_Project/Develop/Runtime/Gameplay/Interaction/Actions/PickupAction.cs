using System.Threading;
using System.Threading.Tasks;
using _Project.Develop.Runtime.Gameplay.Interaction.Core;
using _Project.Develop.Runtime.Gameplay.World;
using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Interaction.Actions
{
    public sealed class PickupAction : MonoBehaviour, IInteractionAction
    {
        public Task ExecuteAsync(InteractionContext context, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            PickupInteractable pickup = GetComponentInParent<PickupInteractable>();
            if (pickup == null || context.Interactor.Inventory == null)
                throw new System.InvalidOperationException("Pickup requires PickupInteractable and an inventory.");

            context.Interactor.Inventory.Add(pickup.Item);
            context.DestroyAfterSuccess(pickup.gameObject);
            return Task.CompletedTask;
        }
    }
}
