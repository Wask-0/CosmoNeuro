using System.Threading;
using System.Threading.Tasks;
using _Project.Develop.Runtime.Gameplay.Dialogue;
using _Project.Develop.Runtime.Gameplay.Interaction.Core;
using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Interaction.Actions
{
    public sealed class ShowMonologueAction : MonoBehaviour, IInteractionAction
    {
        [SerializeField, TextArea(3, 8)] private string _text = "Текст монолога";
        [SerializeField] private bool _blockGameplay;
        public Task ExecuteAsync(InteractionContext context, CancellationToken cancellationToken)
        {
            var owner = GetComponentInParent<Interactable>();
            return context.Dialogue.PlayAsync(new[] { new DialogueLine("", _text) },
                _blockGameplay, owner != null ? owner.InteractionPoint : transform, cancellationToken);
        }
    }
}
