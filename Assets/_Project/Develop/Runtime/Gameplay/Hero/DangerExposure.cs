using System;
using _Project.Develop.Runtime.Gameplay.GameTime;

namespace _Project.Develop.Runtime.Gameplay.Hero
{
    public sealed class DangerExposure
    {
        public bool IsInDanger { get; private set; }
        public DangerCause Cause { get; private set; }
        public double ElapsedSeconds { get; private set; }
        public double LimitSeconds { get; private set; }
        public double RemainingSeconds => Math.Max(0d, LimitSeconds - ElapsedSeconds);
        public bool IsLethal => IsInDanger && ElapsedSeconds >= LimitSeconds;

        public void Apply(DayPeriod period, DangerCause cause, double limitSeconds, double seconds)
        {
            if (period == DayPeriod.Safe) { Reset(); return; }
            if (!IsInDanger || Cause != cause) ElapsedSeconds = 0d;
            IsInDanger = true;
            Cause = cause;
            LimitSeconds = Math.Max(0.01d, limitSeconds);
            ElapsedSeconds += seconds;
        }

        public void Reset() { IsInDanger = false; ElapsedSeconds = 0d; LimitSeconds = 0d; }
    }
}
