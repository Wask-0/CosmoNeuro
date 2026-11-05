using _Project.Develop.Runtime.Configs.Gameplay;
using _Project.Develop.Runtime.Gameplay.Characters.Core;
using _Project.Develop.Runtime.Gameplay.Resources;
using _Project.Develop.Runtime.Utilities.Controllers;
using System;
using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Hero
{
    // Подбирает лежащий тюк при касании и наказывает за падение с высоты без него.
    public sealed class HeroBagController : Controller
    {
        private const float PickupRangeX = 1.5f;
        private const float PickupRangeY = 1.6f;
        private const float CushionRange = 3f;

        private readonly Character _character;
        private readonly HeroBag _bag;
        private readonly HeroResources _resources;
        private readonly HeroMoraleService _morale;
        private readonly HeroSurvivalConfig _config;
        private float _fallSpeed;
        private bool _wasGrounded = true;

        public event Action HardLanded;

        public HeroBagController(Character character, HeroBag bag, HeroResources resources,
            HeroMoraleService morale, HeroSurvivalConfig config)
        {
            _character = character;
            _bag = bag;
            _resources = resources;
            _morale = morale;
            _config = config;
        }

        protected override void UpdateLogic(float deltaTime)
        {
            if (_character == null || _character.IsScripted)
            {
                _fallSpeed = 0f;
                _wasGrounded = true;
                return;
            }

            bool grounded = _character.IsGrounded;
            if (!grounded)
                _fallSpeed = Mathf.Max(_fallSpeed, -_character.CurrentVelocity.y);
            else if (!_wasGrounded)
                OnLanded();

            _wasGrounded = grounded;

            if (_bag.IsOnHero || _bag.IsInFlight || !grounded)
                return;

            Vector2 toBag = _bag.Position - _character.Position;
            if (Mathf.Abs(toBag.x) <= PickupRangeX && Mathf.Abs(toBag.y) <= PickupRangeY)
                _bag.PickUp();
        }

        private void OnLanded()
        {
            float speed = _fallSpeed;
            _fallSpeed = 0f;

            if (speed < _config.HardLandingSpeed)
                return;

            // Мягкая посадка на сброшенный тюк, как и задумано для спуска.
            bool cushioned = !_bag.IsOnHero &&
                             Vector2.Distance(_bag.Position, _character.Position) <= CushionRange;
            if (cushioned)
                return;

            _resources.Oxygen.Spend(Mathf.Min(_config.HardLandingOxygenCost, _resources.Oxygen.Current.Value));
            _morale.Add(-_config.HardLandingMoraleCost);
            HardLanded?.Invoke();
        }
    }
}
