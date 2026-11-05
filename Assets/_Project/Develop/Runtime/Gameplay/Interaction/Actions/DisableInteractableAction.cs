using System.Threading;
using System.Threading.Tasks;
using _Project.Develop.Runtime.Gameplay.Interaction.Core;
using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Interaction.Actions
{
    public class DisableInteractableAction : MonoBehaviour, IInteractionAction
    {
        [SerializeField] private Interactable _interactable;

        public Task ExecuteAsync(InteractionContext context, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            if (_interactable == null)
                throw new System.InvalidOperationException("DisableInteractableAction requires a target.");
            // An authored terminal action disables availability but lets its own operation
            // finish. Disabling another target still cancels that target's operation.
            _interactable.Disable(cancelCurrentOperation: !ReferenceEquals(_interactable, GetComponentInParent<Interactable>()));

            return Task.CompletedTask;
        }

        private void Reset()
        {
            if (_interactable == null)
                _interactable = GetComponentInParent<Interactable>();
        }
    }
}
