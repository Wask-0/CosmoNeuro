using System.Threading;
using System.Threading.Tasks;
using _Project.Develop.Runtime.Gameplay.Interaction.Core;
using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Interaction.Actions
{
    public class DebugInteractionAction : MonoBehaviour, IInteractionAction
    {
        [SerializeField] private string _message = "Interaction Action executed";

        public Task ExecuteAsync(InteractionContext context, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            UnityEngine.Debug.Log(_message, this);

            return Task.CompletedTask;
        }
    }
}