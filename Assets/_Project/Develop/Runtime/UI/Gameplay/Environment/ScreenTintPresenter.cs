using System;
using _Project.Develop.Runtime.Gameplay.GameTime;
using _Project.Develop.Runtime.Gameplay.Hero;
using _Project.Develop.Runtime.UI.Core;
using UnityEngine;

namespace _Project.Develop.Runtime.UI.Gameplay.Environment
{
    public sealed class ScreenTintPresenter : IPresenter
    {
        private static readonly Color Heat = new(1f, 0.42f, 0.1f, 0.2f);
        private static readonly Color Cold = new(0.2f, 0.42f, 1f, 0.24f);
        private static readonly Color Calm = new(0f, 0f, 0f, 0f);

        private readonly DayScheduleService _schedule;
        private readonly ScreenTintView _view;
        private readonly HeroEnvironment _environment;

        public ScreenTintPresenter(DayScheduleService schedule, ScreenTintView view, HeroEnvironment environment)
        {
            _schedule = schedule;
            _view = view;
            _environment = environment;
        }

        public void Initialize()
        {
            _schedule.PeriodChanged += OnPeriodChanged;
            _environment.Changed += Refresh;
            Refresh();
        }

        private void OnPeriodChanged(DayPeriod period, DateTime time) => Refresh();

        private void Refresh()
        {
            bool isHeat = !_schedule.IsSafe && _schedule.CurrentDefinition.DangerCause == DangerCause.Heat;

            if (_environment.InTunnel)
                _view.SetTarget(isHeat ? Calm : Cold);
            else if (_schedule.IsSafe)
                _view.SetTarget(Calm);
            else
                _view.SetTarget(isHeat ? Heat : Cold);
        }

        public void Dispose()
        {
            _schedule.PeriodChanged -= OnPeriodChanged;
            _environment.Changed -= Refresh;
        }
    }
}
