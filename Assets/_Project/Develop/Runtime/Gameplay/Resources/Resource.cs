using System;
using _Project.Develop.Runtime.Utilities.Foundation.Reactive;

namespace _Project.Develop.Runtime.Gameplay.Hero.Resources
{
    public sealed class Resource
    {
        private readonly ReactiveVariable<float> _current;

        public IReadOnlyVariable<float> Current => _current;
        public float Maximum { get; }

        public Resource(float maximum, float initial)
        {
            if (float.IsNaN(maximum) ||
                float.IsInfinity(maximum) ||
                maximum <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(maximum));
            }

            Maximum = maximum;

            _current = new ReactiveVariable<float>(
                UnityEngine.Mathf.Clamp(initial, 0f, maximum));
        }

        public void Spend(float amount)
        {
            ValidateAmount(amount);

            _current.Value = UnityEngine.Mathf.Max(
                0f,
                _current.Value - amount);
        }

        public void Restore(float amount)
        {
            ValidateAmount(amount);

            _current.Value = UnityEngine.Mathf.Min(
                Maximum,
                _current.Value + amount);
        }

        private static void ValidateAmount(float amount)
        {
            if (float.IsNaN(amount) ||
                float.IsInfinity(amount) ||
                amount < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(amount));
            }
        }
    }
}