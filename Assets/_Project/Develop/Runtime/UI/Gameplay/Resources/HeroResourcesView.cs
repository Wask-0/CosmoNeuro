using UnityEngine;

namespace _Project.Develop.Runtime.UI.Gameplay.Resources
{
    public sealed class HeroResourcesView : MonoBehaviour
    {
        [SerializeField] private ResourceGaugeView _food;
        [SerializeField] private ResourceGaugeView _energy;
        [SerializeField] private ResourceGaugeView _oxygen;
        [SerializeField] private ResourceGaugeView _warmDust;

        public ResourceGaugeView Food => _food;
        public ResourceGaugeView Energy => _energy;
        public ResourceGaugeView Oxygen => _oxygen;
        public ResourceGaugeView WarmDust => _warmDust;

        public void Slosh(float amount)
        {
            _food.Slosh(amount);
            _energy.Slosh(amount);
            _oxygen.Slosh(amount);
            _warmDust.Slosh(amount);
        }
    }
}
