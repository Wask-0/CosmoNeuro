using System;

namespace _Project.Develop.Runtime.Gameplay.GameTime
{
    public class GameTimeService
    {
        public event Action<double, TimeAdvanceReason> TimePassed;
        
        private readonly DateTime _startDateTime;
        private readonly double _gameSecondsPerRealSecond;

        private double _elapsedGameSeconds;
        private double _speedMultiplier = 1d;
        private double _fastForwardEnd;
        private double _fastForwardRate;
        private TimeAdvanceReason _fastForwardReason;

        public GameTimeSnapshot Current { get; private set; }

        public double SpeedMultiplier => _speedMultiplier;
        public bool IsFastForwarding => _elapsedGameSeconds < _fastForwardEnd;

        // Передаёт снимок времени и фактически прошедшие игровые секунды.
        public event Action<GameTimeSnapshot, double> Advanced;

        public GameTimeService(
            DateTime startDateTime,
            double gameSecondsPerRealSecond)
        {
            if (!IsFinite(gameSecondsPerRealSecond) ||
                gameSecondsPerRealSecond <= 0d)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(gameSecondsPerRealSecond));
            }

            // Игровой календарь не зависит от часового пояса компьютера.
            _startDateTime = DateTime.SpecifyKind(
                startDateTime,
                DateTimeKind.Unspecified);

            _gameSecondsPerRealSecond = gameSecondsPerRealSecond;

            Current = new GameTimeSnapshot(_startDateTime, 0d);
        }

        public void Advance(double realDeltaSeconds)
        {
            if (!IsFinite(realDeltaSeconds) || realDeltaSeconds < 0d)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(realDeltaSeconds));
            }

            if (IsFastForwarding)
            {
                AdvanceGameSeconds(
                    Math.Min(_fastForwardRate * realDeltaSeconds, _fastForwardEnd - _elapsedGameSeconds),
                    _fastForwardReason);
                return;
            }

            double gameDeltaSeconds =
                realDeltaSeconds *
                _gameSecondsPerRealSecond *
                _speedMultiplier;

            AdvanceGameSeconds(
                gameDeltaSeconds,
                TimeAdvanceReason.Normal);
        }

        public double SecondsUntil(DateTime target)
            => Math.Max(0d, (target - _startDateTime).TotalSeconds - _elapsedGameSeconds);

        public void StartFastForward(double gameSeconds, double realSeconds, TimeAdvanceReason reason)
        {
            if (!IsFinite(gameSeconds) || gameSeconds < 0d)
                throw new ArgumentOutOfRangeException(nameof(gameSeconds));
            if (!IsFinite(realSeconds) || realSeconds <= 0d)
                throw new ArgumentOutOfRangeException(nameof(realSeconds));
            if (IsFastForwarding)
                throw new InvalidOperationException("Time is already fast-forwarding.");

            _fastForwardEnd = _elapsedGameSeconds + gameSeconds;
            _fastForwardRate = Math.Max(gameSeconds / realSeconds, _gameSecondsPerRealSecond * _speedMultiplier);
            _fastForwardReason = reason;
        }

        public void SetSpeedMultiplier(double multiplier)
        {
            if (!IsFinite(multiplier) || multiplier <= 0d)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(multiplier));
            }

            _speedMultiplier = multiplier;
        }

        public void ResetSpeedMultiplier()
        {
            _speedMultiplier = 1d;
        }

        private static bool IsFinite(double value)
        {
            return !double.IsNaN(value) &&
                   !double.IsInfinity(value);
        }
        
        public void AdvanceGameSeconds(
            double gameSeconds,
            TimeAdvanceReason reason)
        {
            if (!IsFinite(gameSeconds) || gameSeconds < 0d)
                throw new ArgumentOutOfRangeException(nameof(gameSeconds));

            if (gameSeconds == 0d)
                return;

            double nextElapsed = _elapsedGameSeconds + gameSeconds;

            if (!IsFinite(nextElapsed))
                throw new ArgumentOutOfRangeException(nameof(gameSeconds));

            DateTime nextDateTime =
                _startDateTime.AddSeconds(nextElapsed);

            _elapsedGameSeconds = nextElapsed;

            Current = new GameTimeSnapshot(
                nextDateTime,
                _elapsedGameSeconds);

            TimePassed?.Invoke(gameSeconds, reason);

            // Существующие часы и расписание продолжают получать это событие.
            Advanced?.Invoke(Current, gameSeconds);
        }
    }
}
