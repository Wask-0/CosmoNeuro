using System.Threading;
using System.Threading.Tasks;
using _Project.Develop.Runtime.Gameplay.Interaction.Core;
using _Project.Develop.Runtime.Gameplay.World;
using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Interaction.Actions
{
    public sealed class LedgeAction : MonoBehaviour, IInteractionAction
    {
        public Task ExecuteAsync(InteractionContext context, CancellationToken cancellationToken)
        {
            LedgeInteractable ledge = GetComponentInParent<LedgeInteractable>();
            if (ledge == null)
                throw new System.InvalidOperationException("LedgeAction requires LedgeInteractable.");
            return ledge.PerformAsync(cancellationToken);
        }
    }
}
