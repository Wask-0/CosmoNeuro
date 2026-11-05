using _Project.Develop.Runtime.Gameplay.Dialogue;
using _Project.Develop.Runtime.Gameplay.Hero;
using _Project.Develop.Runtime.Gameplay.Resources;
using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Interaction.Core
{
    public sealed class CharacterInteractor : IInteractor
    {
        public Transform BodyTransform { get; }
        public DialogueState Dialogue { get; }
        public HeroResources Resources { get; }
        public HeroInventory Inventory { get; }
        public CharacterInteractor(Transform body, DialogueState dialogue, HeroResources resources,
            HeroInventory inventory = null)
        { BodyTransform = body; Dialogue = dialogue; Resources = resources; Inventory = inventory; }
    }
}
