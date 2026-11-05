using System;
using _Project.Develop.Runtime.Configs.Gameplay;
using _Project.Develop.Runtime.Gameplay.Dialogue;
using _Project.Develop.Runtime.Gameplay.GameTime;
using _Project.Develop.Runtime.Gameplay.Resources;
using _Project.Develop.Runtime.Infrastructure.AppStates;
using _Project.Develop.Runtime.Infrastructure.DI;

namespace _Project.Develop.Runtime.Gameplay.Hero
{
    public sealed class HeroLifeService : IInitializable, IDisposable
    {
        private readonly GameTimeService _time;
        private readonly DayScheduleService _schedule;
        private readonly AppStateService _app;
        private readonly DialogueState _dialogue;
        private readonly HeroResources _resources;
        private readonly HeroEnvironment _environment;
        private readonly HeroSurvivalConfig _config;
        public DangerExposure Exposure { get; } = new();
        public bool IsDead { get; private set; }
        public event Action Changed;
        public event Action<DangerCause> Died;

        public HeroLifeService(GameTimeService time, DayScheduleService schedule,
            AppStateService app, DialogueState dialogue, HeroResources resources,
            HeroEnvironment environment, HeroSurvivalConfig config)
        {
            _time = time;
            _schedule = schedule;
            _app = app;
            _dialogue = dialogue;
            _resources = resources;
            _environment = environment;
            _config = config;
        }

        public void Initialize()
        {
            ApplyExposure(_schedule.GetDefinitionAt(_time.Current.DateTime), 0d);
            _time.TimePassed += OnTimePassed;
        }

        private void OnTimePassed(double seconds, TimeAdvanceReason reason)
        {
            if (IsDead || _app.Current != AppState.Gameplay) return;
            if (reason != TimeAdvanceReason.TentRest && _resources.Oxygen.Current.Value <= 0f)
            {
                Die(DangerCause.Suffocation);
                Changed?.Invoke();
                return;
            }
            if (reason == TimeAdvanceReason.TentRest)
            {
                Exposure.Reset();
            }
            else
            {
                DateTime until = _time.Current.DateTime;
                _schedule.VisitSegments(until.AddSeconds(-seconds), until, (period, duration) =>
                {
                    ApplyExposure(period, duration);
                    if (!Exposure.IsLethal) return true;
                    Die(Exposure.Cause);
                    return false;
                });
                // A step ending exactly at a boundary has no duration in the new period.
                if (!IsDead)
                    ApplyExposure(_schedule.GetDefinitionAt(until), 0d);
            }
            Changed?.Invoke();
        }

        // В тоннеле всё наоборот: в жару там безопасно, в остальное время скала вытягивает тепло.
        private void ApplyExposure(DayPeriodDefinition definition, double seconds)
        {
            if (_environment.InTunnel)
            {
                bool isHeat = definition.Period == DayPeriod.Dangerous && definition.DangerCause == DangerCause.Heat;
                Exposure.Apply(isHeat ? DayPeriod.Safe : DayPeriod.Dangerous, DangerCause.Cold,
                    _config.TunnelColdSurvivalMinutes * 60d, seconds);
                return;
            }

            Exposure.Apply(definition.Period, definition.DangerCause, definition.SurvivalGameSeconds, seconds);
        }

        private void Die(DangerCause cause)
        {
            IsDead = true;
            _dialogue.Close();
            _app.SetState(AppState.Dead);
            Died?.Invoke(cause);
        }

        public void Dispose() { _time.TimePassed -= OnTimePassed; Changed = null; Died = null; }
    }
}
