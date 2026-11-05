using System;

namespace _Project.Develop.Runtime.Gameplay.GameTime
{
    public readonly struct GameTimeSnapshot
    {
        public DateTime DateTime { get; }

        public double ElapsedGameSeconds { get; }

        public double DayProgress =>
            DateTime.TimeOfDay.TotalSeconds / TimeSpan.FromDays(1).TotalSeconds;

        public GameTimeSnapshot(
            DateTime dateTime,
            double elapsedGameSeconds)
        {
            DateTime = dateTime;
            ElapsedGameSeconds = elapsedGameSeconds;
        }
    }
}