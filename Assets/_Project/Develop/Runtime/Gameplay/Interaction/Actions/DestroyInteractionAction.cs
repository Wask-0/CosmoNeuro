using System.Threading;
using System.Threading.Tasks;
using _Project.Develop.Runtime.Gameplay.Interaction.Core;
using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Interaction.Actions
{
    public class DestroyInteractionAction : MonoBehaviour, IInteractionAction
    {
        [SerializeField] private GameObject _target;

        public Task ExecuteAsync(InteractionContext context, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            Interactable owner = GetComponentInParent<Interactable>();
            GameObject target = _target != null ? _target : owner != null ? owner.gameObject : gameObject;
            // Do not destroy the operation owner before its success chain has finished.
            context.DestroyAfterSuccess(target);

            return Task.CompletedTask;
        }
    }
}
