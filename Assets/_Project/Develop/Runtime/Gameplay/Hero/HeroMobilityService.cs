using _Project.Develop.Runtime.Configs.Gameplay;
using _Project.Develop.Runtime.Gameplay.Resources;
using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Hero
{
    public sealed class HeroMobilityService
    {
        private readonly HeroMobilityConfig _config;
        private readonly HeroResources _resources;
        private readonly HeroMoraleService _morale;
        private readonly HeroBag _bag;

        public HeroMobilityService(HeroMobilityConfig config, HeroResources resources,
            HeroMoraleService morale, HeroBag bag)
        {
            _config = config;
            _resources = resources;
            _morale = morale;
            _bag = bag;
        }

        // Пока тюк закинут или сброшен, еда героя не тяготит.
        private float FoodLoad => _bag.IsOnHero
            ? Mathf.Clamp01((float)_resources.Food.Value / Mathf.Max(1, _resources.MaximumFood))
            : 0f;

        public float SpeedMultiplier =>
            Mathf.Lerp(_config.SpeedFactorWithoutFood, _config.SpeedFactorWithFullFood, FoodLoad) *
            _morale.SpeedMultiplier;

        public float JumpHeight =>
            Mathf.Lerp(_config.JumpHeightWithoutFood, _config.JumpHeightWithFullFood, FoodLoad);
    }
}
