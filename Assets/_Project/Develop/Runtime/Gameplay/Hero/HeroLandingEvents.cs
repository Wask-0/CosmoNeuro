using System;

namespace _Project.Develop.Runtime.Gameplay.Hero
{
    // Контроллер героя создаётся позже сервисов сцены, поэтому его событие проходит через этот мост.
    public sealed class HeroLandingEvents
    {
        public event Action HardLanded;

        public void Bind(HeroBagController controller) => controller.HardLanded += () => HardLanded?.Invoke();
    }
}
