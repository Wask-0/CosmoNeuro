using _Project.Develop.Runtime.Gameplay.Hero;
using _Project.Develop.Runtime.Gameplay.Tents;
using _Project.Develop.Runtime.Utilities.Controllers;
using UnityEngine;

namespace _Project.Develop.Runtime.UI.Gameplay.Environment
{
    public sealed class DirectionHintController : Controller
    {
        private const float IdleSeconds = 5f;

        private readonly HeroContext _hero;
        private readonly HeroInventory _inventory;
        private readonly DirectionHintView _view;
        private readonly TentService _tents;
        private float _idle;

        public DirectionHintController(HeroContext hero, HeroInventory inventory, DirectionHintView view,
            TentService tents)
        {
            _tents = tents;
            _hero = hero;
            _inventory = inventory;
            _view = view;
        }

        protected override void UpdateLogic(float deltaTime)
        {
            // У палатки герой стоит намеренно, и подсказывать дорогу незачем.
            if (_hero.Character == null || !_inventory.HasAll || _tents.HasTent)
            {
                _idle = 0f;
                _view.SetVisible(false);
                return;
            }

            bool moving = Mathf.Abs(_hero.Character.CurrentVelocity.x) > 0.1f;
            _idle = moving ? 0f : _idle + deltaTime;
            _view.SetVisible(_idle >= IdleSeconds);
        }
    }
}
