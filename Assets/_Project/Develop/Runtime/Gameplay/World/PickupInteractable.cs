using _Project.Develop.Runtime.Gameplay.Hero;
using _Project.Develop.Runtime.Gameplay.Interaction.Core;
using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.World
{
    public sealed class PickupInteractable : Interactable
    {
        [SerializeField] private HeroItem _item;
        [SerializeField] private string _interactionText = "Подобрать";
        [SerializeField] private Transform _interactionPoint;

        public HeroItem Item => _item;
        public override Transform InteractionPoint => _interactionPoint != null ? _interactionPoint : transform;
        protected override string GetInteractionText() => _interactionText;
    }
}
