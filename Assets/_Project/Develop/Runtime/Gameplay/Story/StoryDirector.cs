using System;
using _Project.Develop.Runtime.Configs.Gameplay;
using _Project.Develop.Runtime.Gameplay.Dust;
using _Project.Develop.Runtime.Gameplay.GameTime;
using _Project.Develop.Runtime.Gameplay.Hero;
using _Project.Develop.Runtime.Gameplay.Resources;
using _Project.Develop.Runtime.Gameplay.Tents;
using _Project.Develop.Runtime.Infrastructure.AppStates;
using _Project.Develop.Runtime.Infrastructure.DI;
using _Project.Develop.Runtime.Utilities.Controllers;
using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Story
{
    // Сценарий обучения и реакции героя на происходящее.
    public sealed class StoryDirector : Controller, IInitializable, IDisposable
    {
        private const string TentHint = "tent";
        private const string ControlsHint = "controls";
        private const string LeashHint = "leash";
        private const double WarningGameSeconds = 2400d;
        private const float ControlsHintSeconds = 12f;

        private readonly MonologueService _monologues;
        private readonly StoryConfig _config;
        private readonly HeroInventory _inventory;
        private readonly HeroResources _resources;
        private readonly HeroMoraleService _morale;
        private readonly HeroBag _bag;
        private readonly HeroLandingEvents _landing;
        private readonly DustCollectionState _dust;
        private readonly TentService _tents;
        private readonly DayScheduleService _schedule;
        private readonly GameTimeService _time;
        private readonly AppStateService _app;
        private readonly HintService _hints;
        private readonly HeroContext _hero;

        private float _ambientTimer;
        private int _ambientIndex;
        private float _controlsTimer;
        private DateTime _warnedDanger;
        private bool _restWasRunning;
        private int _rests;

        public StoryDirector(MonologueService monologues, StoryConfig config, HeroInventory inventory,
            HeroResources resources, HeroMoraleService morale, HeroBag bag, HeroLandingEvents landing,
            DustCollectionState dust, TentService tents, DayScheduleService schedule, GameTimeService time,
            AppStateService app, HintService hints, HeroContext hero)
        {
            _monologues = monologues;
            _config = config;
            _inventory = inventory;
            _resources = resources;
            _morale = morale;
            _bag = bag;
            _landing = landing;
            _dust = dust;
            _tents = tents;
            _schedule = schedule;
            _time = time;
            _app = app;
            _hints = hints;
            _hero = hero;
        }

        public void Initialize()
        {
            _inventory.Collected += OnItemCollected;
            _dust.Started += OnDustStarted;
            _dust.SourceDepleted += OnSourceDepleted;
            _morale.LevelChanged += OnMoraleLevelChanged;
            _landing.HardLanded += OnHardLanded;
            _tents.RestStarted += OnRestStarted;
            _tents.Placed += OnTentPlaced;
            _schedule.PeriodChanged += OnPeriodChanged;
            _resources.Rested += OnRested;
            ResetAmbientTimer();
        }

        public void Begin()
        {
            _monologues.Play("wake_up");
            if (_hero.Character != null && _hero.View != null)
            {
                var character = _hero.Character;
                character.BeginScriptedMotion();
                _hero.View.PlayGetUp(2.2f, 1.1f, character.EndScriptedMotion);
            }
            _controlsTimer = ControlsHintSeconds;
            _hints.Show(ControlsHint, "[A/D] Идти   [Пробел] Прыжок   [E] Подобрать");
        }

        protected override void UpdateLogic(float deltaTime)
        {
            if (_app.Current != AppState.Gameplay)
                return;

            TickControlsHint(deltaTime);
            TickDangerWarning();
            TickRestEnd();
            TickResources();
            TickLeashHint();
            TickAmbient(deltaTime);
        }

        private void TickControlsHint(float deltaTime)
        {
            if (_controlsTimer <= 0f) return;
            _controlsTimer -= deltaTime;
            if (_controlsTimer <= 0f) _hints.Clear(ControlsHint);
        }

        private void TickDangerWarning()
        {
            if (!_inventory.Has(HeroItem.Tent) || _tents.HasTent || !_schedule.IsSafe)
            {
                if (_tents.HasTent || !_inventory.Has(HeroItem.Tent)) _hints.Clear(TentHint);
                return;
            }

            DateTime now = _time.Current.DateTime;
            if (_schedule.FindNextDangerStart(now) is not DateTime danger)
                return;

            if ((danger - now).TotalSeconds > WarningGameSeconds)
            {
                _hints.Clear(TentHint);
                return;
            }

            _hints.Show(TentHint, "[E] Поставить палатку");
            if (_warnedDanger == danger) return;
            _warnedDanger = danger;

            DayPeriodDefinition next = _schedule.GetDefinitionAt(danger.AddMinutes(1d));
            _monologues.Play(next.DangerCause == DangerCause.Heat ? "heat_soon" : "cold_soon");
        }

        private void TickRestEnd()
        {
            bool running = _restWasRunning && _time.IsFastForwarding;
            if (_restWasRunning && !running)
            {
                _restWasRunning = false;
                _monologues.Play(_rests == 1 ? "after_first_rest" : null);
            }
        }

        private void TickResources()
        {
            if (_resources.Oxygen.Current.Value <= _resources.Oxygen.Maximum * 0.25f)
                _monologues.Play("low_oxygen");
            if (_inventory.Has(HeroItem.Food) && _resources.Food.Value <= 1)
                _monologues.Play("low_food");
        }

        private void TickLeashHint()
        {
            if (_hero.Character != null && _tents.HasTent &&
                _tents.IsBeyondLeash(_hero.Character.Position, _hero.Character.Facing))
                _hints.Show(LeashHint, "[Q] Сначала соберите палатку");
            else
                _hints.Clear(LeashHint);
        }

        private void TickAmbient(float deltaTime)
        {
            if (_monologues.IsPlaying || !_inventory.HasAll || _time.IsFastForwarding ||
                _config.AmbientMonologues == null || _ambientIndex >= _config.AmbientMonologues.Length)
                return;

            _ambientTimer -= deltaTime;
            if (_ambientTimer > 0f) return;

            _monologues.Play(_config.AmbientMonologues[_ambientIndex++]);
            ResetAmbientTimer();
        }

        private void ResetAmbientTimer()
            => _ambientTimer = UnityEngine.Random.Range(_config.AmbientIntervalMin, _config.AmbientIntervalMax);

        private void OnItemCollected(HeroItem item)
        {
            _morale.AddChange();
            _bag.Show();
            if (_inventory.HasAll)
                _monologues.Play("all_items");
        }

        private void OnDustStarted()
        {
            if (_dust.Kind == DustKind.Magnetic)
                _monologues.Play("device_works");
        }

        private void OnSourceDepleted(DustKind kind)
        {
            if (kind == DustKind.Magnetic)
                _monologues.Play("energy_to_oxygen");
        }

        private void OnMoraleLevelChanged(MoraleLevel level)
        {
            if (level == MoraleLevel.Good) _monologues.Play("morale_good");
            else if (level == MoraleLevel.Bad) _monologues.Play("morale_bad");
        }

        private void OnHardLanded() => _monologues.Play("hard_landing", once: false);

        private void OnTentPlaced() => _monologues.Play("tent_placed");

        private void OnRestStarted()
        {
            _rests++;
            _restWasRunning = true;
        }

        private void OnRested(int foodSpent, bool usedWarmDust)
        {
            if (usedWarmDust) _monologues.Play("warm_dust_used");
            else if (foodSpent > 1) _monologues.Play("cold_without_dust");
        }

        private void OnPeriodChanged(DayPeriod period, DateTime time)
        {
            if (period != DayPeriod.Dangerous || _time.IsFastForwarding)
                return;

            DangerCause cause = _schedule.CurrentDefinition.DangerCause;
            _monologues.Play(cause == DangerCause.Heat ? "heat_started" : "cold_started");
        }

        public void Dispose()
        {
            _inventory.Collected -= OnItemCollected;
            _dust.Started -= OnDustStarted;
            _dust.SourceDepleted -= OnSourceDepleted;
            _morale.LevelChanged -= OnMoraleLevelChanged;
            _landing.HardLanded -= OnHardLanded;
            _tents.RestStarted -= OnRestStarted;
            _tents.Placed -= OnTentPlaced;
            _schedule.PeriodChanged -= OnPeriodChanged;
            _resources.Rested -= OnRested;
        }
    }
}
