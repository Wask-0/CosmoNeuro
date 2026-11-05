using System.Threading;
using System.Threading.Tasks;
using _Project.Develop.Runtime.Gameplay.Interaction.Core;
using UnityEngine;
using UnityEngine.Events;

namespace _Project.Develop.Runtime.Gameplay.Interaction.Actions
{
    public sealed class UnityEventInteractionAction : MonoBehaviour, IInteractionAction
    {
        [SerializeField] private UnityEvent _onExecute = new();
        public Task ExecuteAsync(InteractionContext context, CancellationToken cancellationToken)
        { cancellationToken.ThrowIfCancellationRequested(); _onExecute.Invoke(); return Task.CompletedTask; }
    }
}
