using System;
using _Project.Develop.Runtime.Configs.Gameplay;
using _Project.Develop.Runtime.Gameplay.GameTime;
using _Project.Develop.Runtime.Gameplay.Tents;
using _Project.Develop.Runtime.Infrastructure.DI;
using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Hero
{
    public enum MoraleLevel
    {
        Excellent,
        Good,
        Bad
    }

    public sealed class HeroMoraleService : IInitializable, IDisposable
    {
        private const double RisingGameSeconds = 1800d;

        private readonly HeroMobilityConfig _config;
        private readonly GameTimeService _time;
        private readonly TentService _tents;
        private double _risingUntil;

        public float Value { get; private set; }
        public float Normalized => Value / _config.MaximumMorale;
        public MoraleLevel Level { get; private set; }

        // Стрелка рядом с лицом: растёт после отдыха и перемен, в дороге падает.
        public bool IsRising => _time.Current.ElapsedGameSeconds < _risingUntil;

        public float SpeedMultiplier => Level switch
        {
            MoraleLevel.Excellent => _config.ExcellentSpeed,
            MoraleLevel.Good => _config.GoodSpeed,
            _ => _config.BadSpeed
        };

        public event Action Changed;
        public event Action<MoraleLevel> LevelChanged;

        public HeroMoraleService(HeroMobilityConfig config, GameTimeService time, TentService tents)
        {
            _config = config;
            _time = time;
            _tents = tents;
            Value = config.MaximumMorale;
            Level = Evaluate(Normalized);
        }

        public void Initialize()
        {
            _time.TimePassed += OnTimePassed;
            _tents.RestStarted += OnRestStarted;
        }

        public void AddChange() => Add(_config.ChangeMoraleGain);

        public void Add(float amount)
        {
            if (amount > 0f)
                _risingUntil = _time.Current.ElapsedGameSeconds + RisingGameSeconds;
            Value = Mathf.Clamp(Value + amount, 0f, _config.MaximumMorale);
            Refresh();
        }

        private void OnRestStarted() => Add(_config.RestMoraleGain);

        private void OnTimePassed(double gameSeconds, TimeAdvanceReason reason)
        {
            if (reason != TimeAdvanceReason.Normal)
                return;

            Value = Mathf.Max(0f, Value - (float)(gameSeconds / 3600d) * _config.MoraleDecayPerHour);
            Refresh();
        }

        private void Refresh()
        {
            MoraleLevel level = Evaluate(Normalized);
            bool levelChanged = level != Level;
            Level = level;
            Changed?.Invoke();
            if (levelChanged) LevelChanged?.Invoke(level);
        }

        private MoraleLevel Evaluate(float normalized)
        {
            float percent = normalized * 100f;
            if (percent >= _config.ExcellentFrom) return MoraleLevel.Excellent;
            return percent >= _config.GoodFrom ? MoraleLevel.Good : MoraleLevel.Bad;
        }

        public void Dispose()
        {
            _time.TimePassed -= OnTimePassed;
            _tents.RestStarted -= OnRestStarted;
            Changed = null;
            LevelChanged = null;
        }
    }
}
