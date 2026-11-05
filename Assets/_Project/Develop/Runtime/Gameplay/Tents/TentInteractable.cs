using System.Threading;
using System.Threading.Tasks;
using _Project.Develop.Runtime.Gameplay.Interaction;
using _Project.Develop.Runtime.Gameplay.Interaction.Core;
using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Tents
{
    public sealed class TentInteractable : Interactable, ISecondaryInteractable
    {
        [SerializeField] private Transform _interactionPoint;
        private TentService _service;
        public override Transform InteractionPoint => _interactionPoint != null ? _interactionPoint : transform;
        public string SecondaryActionText => "Собрать палатку";
        public void Initialize(TentService service) => _service = service;
        protected override string GetInteractionText() => "Войти в палатку";
        public bool TryRest() => _service != null && _service.TryRest(gameObject);
        public Task<InteractionResult> InteractSecondaryAsync(InteractionContext context, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            bool packed = _service != null && _service.TryPack(gameObject);
            return Task.FromResult(new InteractionResult(packed, packed ? null : "Невозможно собрать палатку."));
        }
    }
}
