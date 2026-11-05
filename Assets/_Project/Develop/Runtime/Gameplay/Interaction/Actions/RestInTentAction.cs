using System.Threading;
using System.Threading.Tasks;
using _Project.Develop.Runtime.Gameplay.Interaction.Core;
using _Project.Develop.Runtime.Gameplay.Tents;
using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Interaction.Actions
{
    public sealed class RestInTentAction : MonoBehaviour, IInteractionAction
    {
        public Task ExecuteAsync(InteractionContext context, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            TentInteractable tent = GetComponentInParent<TentInteractable>();
            if (tent == null || !tent.TryRest())
                throw new _Project.Develop.Runtime.Gameplay.Interaction.InteractionRejectedException("Для отдыха нужна еда.");
            return Task.CompletedTask;
        }
    }
}
