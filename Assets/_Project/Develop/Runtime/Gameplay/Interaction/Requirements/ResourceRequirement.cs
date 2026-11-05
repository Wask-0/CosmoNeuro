using _Project.Develop.Runtime.Gameplay.Interaction.Core;
using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Interaction.Requirements
{
    public sealed class ResourceRequirement : MonoBehaviour, IInteractionRequirement
    {
        public enum ResourceKind { Food, Energy, Oxygen }
        [SerializeField] private ResourceKind _resource;
        [SerializeField, Min(0f)] private float _minimum = 1f;
        public bool IsSatisfied(InteractionContext context)
        {
            if (context.Resources == null) return false;
            float value = _resource switch
            {
                ResourceKind.Food => context.Resources.Food.Value,
                ResourceKind.Energy => context.Resources.Energy.Current.Value,
                _ => context.Resources.Oxygen.Current.Value
            };
            return value >= _minimum;
        }
    }
}
