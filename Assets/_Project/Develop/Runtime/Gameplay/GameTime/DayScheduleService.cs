using System;
using System.Collections.Generic;
using _Project.Develop.Runtime.Configs.Gameplay;
using _Project.Develop.Runtime.Infrastructure.DI;

namespace _Project.Develop.Runtime.Gameplay.GameTime
{
    public sealed class DayScheduleService : IInitializable, IDisposable
    {
        private readonly GameTimeService _gameTime;
        private readonly DayPeriodDefinition[] _periods;

        private DateTime _lastProcessedTime;
        private bool _initialized;
        private bool _disposed;

        public DayPeriod CurrentPeriod { get; private set; }
        public DayPeriodDefinition CurrentDefinition { get; private set; }

        public bool IsSafe => CurrentPeriod == DayPeriod.Safe;

        // В событии передаётся точное время перехода.
        public event Action<DayPeriod, DateTime> PeriodChanged;

        public DayScheduleService(
            GameTimeService gameTime,
            IReadOnlyList<DayPeriodDefinition> periods)
        {
            _gameTime = gameTime ??
                throw new ArgumentNullException(nameof(gameTime));

            if (periods == null || periods.Count == 0)
            {
                throw new ArgumentException(
                    "Расписание должно содержать хотя бы один период.",
                    nameof(periods));
            }

            _periods = new DayPeriodDefinition[periods.Count];

            for (int i = 0; i < periods.Count; i++)
            {
                DayPeriodDefinition definition = periods[i];

                if (definition.Hour < 0 || definition.Hour > 23 ||
                    definition.Minute < 0 || definition.Minute > 59 ||
                    !Enum.IsDefined(typeof(DayPeriod), definition.Period))
                {
                    throw new ArgumentException(
                        $"Некорректная запись расписания: {i}.",
                        nameof(periods));
                }

                _periods[i] = definition;
            }

            Array.Sort(
                _periods,
                (first, second) =>
                    first.MinuteOfDay.CompareTo(second.MinuteOfDay));

            for (int i = 1; i < _periods.Length; i++)
            {
                if (_periods[i - 1].MinuteOfDay ==
                    _periods[i].MinuteOfDay)
                {
                    throw new ArgumentException(
                        "Два периода не могут начинаться одновременно.",
                        nameof(periods));
                }
            }

            _lastProcessedTime = _gameTime.Current.DateTime;
            CurrentPeriod = GetPeriodAt(_lastProcessedTime);
            CurrentDefinition = GetDefinitionAt(_lastProcessedTime);
        }

        public void Initialize()
        {
            if (_disposed)
                throw new ObjectDisposedException(
                    nameof(DayScheduleService));

            if (_initialized)
                return;

            _lastProcessedTime = _gameTime.Current.DateTime;
            CurrentPeriod = GetPeriodAt(_lastProcessedTime);
            CurrentDefinition = GetDefinitionAt(_lastProcessedTime);

            _gameTime.Advanced += OnTimeAdvanced;

            _initialized = true;
        }

        public DayPeriod GetPeriodAt(DateTime dateTime)
            => GetDefinitionAt(dateTime).Period;

        public DateTime? FindNextSafeStart(DateTime from)
        {
            DateTime? next = null;
            foreach (DayPeriodDefinition definition in _periods)
            {
                if (definition.Period != DayPeriod.Safe)
                    continue;

                DateTime start = from.Date.AddMinutes(definition.MinuteOfDay);
                if (start <= from)
                    start = start.AddDays(1);
                if (next == null || start < next.Value)
                    next = start;
            }

            return next;
        }

        public DateTime? FindNextDangerStart(DateTime from)
        {
            DateTime? next = null;
            foreach (DayPeriodDefinition definition in _periods)
            {
                if (definition.Period != DayPeriod.Dangerous)
                    continue;

                DateTime start = from.Date.AddMinutes(definition.MinuteOfDay);
                if (start <= from)
                    start = start.AddDays(1);
                if (next == null || start < next.Value)
                    next = start;
            }

            return next;
        }

        public DayPeriodDefinition GetDefinitionAt(DateTime dateTime)
        {
            double minuteOfDay =
                dateTime.TimeOfDay.TotalMinutes;

            // До первой записи действует последний период прошлых суток.
            DayPeriodDefinition result = _periods[_periods.Length - 1];

            foreach (DayPeriodDefinition definition in _periods)
            {
                if (minuteOfDay < definition.MinuteOfDay)
                    break;

                result = definition;
            }

            return result;
        }

        // Split large skips at every schedule boundary; exposure must not treat
        // a mixed safe/dangerous interval as one uniform period.
        public void VisitSegments(DateTime from, DateTime until, Func<DayPeriodDefinition, double, bool> visitor)
        {
            DateTime cursor = from;
            while (cursor < until)
            {
                DateTime next = until;
                foreach (DayPeriodDefinition definition in _periods)
                {
                    DateTime boundary = cursor.Date.AddMinutes(definition.MinuteOfDay);
                    if (boundary <= cursor)
                    {
                        if (cursor.Date == until.Date) continue;
                        boundary = boundary.AddDays(1);
                    }
                    if (boundary < next) next = boundary;
                }
                if (!visitor(GetDefinitionAt(cursor), (next - cursor).TotalSeconds)) return;
                cursor = next;
            }
        }

        private void OnTimeAdvanced(
            GameTimeSnapshot snapshot,
            double elapsedGameSeconds)
        {
            DateTime previous = _lastProcessedTime;
            DateTime current = snapshot.DateTime;

            _lastProcessedTime = current;

            DateTime day = previous.Date;

            // Обрабатываем каждую пересечённую границу,
            // даже при ускорении времени или смене нескольких суток.
            while (true)
            {
                foreach (DayPeriodDefinition definition in _periods)
                {
                    DateTime boundary =
                        day.AddMinutes(definition.MinuteOfDay);

                    if (boundary <= previous || boundary > current)
                        continue;

                    bool changed = CurrentPeriod != definition.Period ||
                        CurrentDefinition.DangerCause != definition.DangerCause;
                    CurrentPeriod = definition.Period;
                    CurrentDefinition = definition;
                    if (changed) PeriodChanged?.Invoke(CurrentPeriod, boundary);
                }

                if (day == current.Date)
                    break;

                day = day.AddDays(1);
            }
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            if (_initialized)
                _gameTime.Advanced -= OnTimeAdvanced;

            PeriodChanged = null;
        }
    }
}
