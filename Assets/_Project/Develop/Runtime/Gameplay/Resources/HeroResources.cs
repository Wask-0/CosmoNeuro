using System;
using _Project.Develop.Runtime.Configs.Gameplay;
using _Project.Develop.Runtime.Gameplay.GameTime;
using _Project.Develop.Runtime.Gameplay.Hero;
using _Project.Develop.Runtime.Gameplay.Hero.Resources;
using _Project.Develop.Runtime.Infrastructure.DI;
using _Project.Develop.Runtime.Utilities.Foundation.Reactive;
using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Resources
{
    public sealed class HeroResources : IInitializable, IDisposable
    {
        private readonly HeroSurvivalConfig _config;
        private readonly GameTimeService _gameTime;
        private readonly HeroInventory _inventory;
        private readonly ReactiveVariable<int> _food;

        private bool _initialized;

        public IReadOnlyVariable<int> Food => _food;

        public Resource Energy { get; }
        public Resource Oxygen { get; }
        public Resource WarmDust { get; }
        public int MaximumFood => _config.MaximumFood;

        // Передаёт потраченную еду и то, грела ли палатку лунная пыль.
        public event Action<int, bool> Rested;

        public HeroResources(
            HeroSurvivalConfig config,
            GameTimeService gameTime,
            HeroInventory inventory)
        {
            _config = config;
            _gameTime = gameTime;
            _inventory = inventory;

            _food = new ReactiveVariable<int>(
                Mathf.Max(0, config.InitialFood));

            Energy = new Resource(
                config.MaximumEnergy,
                config.InitialEnergy);

            Oxygen = new Resource(
                config.MaximumOxygen,
                config.InitialOxygen);

            WarmDust = new Resource(
                config.MaximumWarmDust,
                config.InitialWarmDust);
        }

        public void Initialize()
        {
            if (_initialized)
                return;

            _gameTime.TimePassed += OnTimePassed;
            _inventory.Collected += OnItemCollected;

            _initialized = true;
        }

        public bool TryRest(bool isCold)
        {
            if (_food.Value <= 0)
                return false;

            // В холод палатку греет собранная пыль, без неё уходит вдвое больше еды.
            int foodCost = 1;
            bool usedWarmDust = false;
            if (isCold)
            {
                if (WarmDust.Current.Value >= _config.WarmDustPerColdRest - 0.01f)
                {
                    WarmDust.Spend(Mathf.Min(_config.WarmDustPerColdRest, WarmDust.Current.Value));
                    usedWarmDust = true;
                }
                else
                {
                    foodCost = Mathf.Max(1, _config.ColdRestFoodMultiplier);
                }
            }

            float oxygenSpace =
                Oxygen.Maximum - Oxygen.Current.Value;

            float energyToSpend = Mathf.Min(
                Energy.Current.Value,
                oxygenSpace);

            int foodSpent = Mathf.Min(foodCost, _food.Value);
            _food.Value -= foodSpent;

            Energy.Spend(energyToSpend);

            Oxygen.Restore(energyToSpend);

            Rested?.Invoke(foodSpent, usedWarmDust);

            return true;
        }

        private void OnItemCollected(HeroItem item)
        {
            if (item == HeroItem.Food)
                _food.Value = Mathf.Min(_config.MaximumFood, _food.Value + _config.FoodPickup);
            else if (item == HeroItem.Oxygen)
                Oxygen.Restore(_config.OxygenPickup);
        }

        private void OnTimePassed(
            double gameSeconds,
            TimeAdvanceReason reason)
        {
            if (reason == TimeAdvanceReason.TentRest)
                return;

            double oxygenCost =
                gameSeconds / 3600d * _config.OxygenPerHour;

            // Для большого пропуска достаточно списать весь остаток.
            Oxygen.Spend((float)Math.Min(
                oxygenCost,
                Oxygen.Current.Value));
        }

        public void Dispose()
        {
            if (!_initialized)
                return;

            _gameTime.TimePassed -= OnTimePassed;
            _inventory.Collected -= OnItemCollected;

            _initialized = false;
        }
    }
}
