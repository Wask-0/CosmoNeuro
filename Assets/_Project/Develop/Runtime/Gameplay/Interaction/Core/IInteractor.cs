using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Interaction.Core
{
    public interface IInteractor
    {
        Transform BodyTransform { get; }
        _Project.Develop.Runtime.Gameplay.Dialogue.DialogueState Dialogue { get; }
        _Project.Develop.Runtime.Gameplay.Resources.HeroResources Resources { get; }
        _Project.Develop.Runtime.Gameplay.Hero.HeroInventory Inventory { get; }
    }
}
