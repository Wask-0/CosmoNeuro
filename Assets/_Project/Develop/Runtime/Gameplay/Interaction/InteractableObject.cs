using _Project.Develop.Runtime.Gameplay.Interaction.Core;
using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Interaction
{
    public class InteractableObject : Interactable
    {
        [SerializeField] private string _interactionText = "Взаимодействовать";
        [SerializeField] private Transform _interactionPoint;

        public override Transform InteractionPoint => _interactionPoint != null ? _interactionPoint : transform;

        protected override string GetInteractionText() => _interactionText;
        
    }
}
