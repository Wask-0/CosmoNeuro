using _Project.Develop.Runtime.Gameplay.Hero;
using _Project.Develop.Runtime.Utilities.Controllers;
using UnityEngine;

namespace _Project.Develop.Runtime.UI.Gameplay.Resources
{
    // Жидкость в шкалах отзывается на разгон, остановку и приземление героя.
    public sealed class HudSloshController : Controller
    {
        private const float Sensitivity = 0.9f;

        private readonly HeroContext _hero;
        private readonly HeroResourcesView _view;
        private Vector2 _previousVelocity;

        public HudSloshController(HeroContext hero, HeroResourcesView view)
        {
            _hero = hero;
            _view = view;
        }

        protected override void UpdateLogic(float deltaTime)
        {
            if (_hero.Character == null) return;
            Vector2 velocity = _hero.Character.CurrentVelocity;
            float change = (velocity - _previousVelocity).magnitude;
            _previousVelocity = velocity;
            if (change > 0.02f)
                _view.Slosh(change * Sensitivity);
        }
    }
}
