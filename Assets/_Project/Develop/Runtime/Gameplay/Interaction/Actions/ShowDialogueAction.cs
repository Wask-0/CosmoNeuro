using System.Threading;
using System.Threading.Tasks;
using _Project.Develop.Runtime.Configs.Gameplay;
using _Project.Develop.Runtime.Gameplay.Interaction.Core;
using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Interaction.Actions
{
    public sealed class ShowDialogueAction : MonoBehaviour, IInteractionAction
    {
        [SerializeField] private DialogueDefinition _dialogue;
        [SerializeField] private bool _blockGameplay = true;
        public Task ExecuteAsync(InteractionContext context, CancellationToken cancellationToken)
        {
            if (_dialogue == null) throw new System.InvalidOperationException("Assign a DialogueDefinition.");
            var owner = GetComponentInParent<Interactable>();
            return context.Dialogue.PlayAsync(_dialogue.Lines, _blockGameplay,
                owner != null ? owner.InteractionPoint : transform, cancellationToken);
        }
    }
}
