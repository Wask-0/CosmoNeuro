using System;
using System.Collections.Generic;
using _Project.Develop.Runtime.Gameplay.GameTime;
using UnityEngine;

namespace _Project.Develop.Runtime.Configs.Gameplay
{
    [Serializable]
    public struct DayPeriodDefinition
    {
        [SerializeField, Range(0, 23)] private int _hour;
        [SerializeField, Range(0, 59)] private int _minute;
        [SerializeField] private DayPeriod _period;
        [SerializeField] private DangerCause _dangerCause;
        [SerializeField, Min(0.01f), Tooltip("Игровые минуты до смерти при непрерывном воздействии опасности")]
        private float _survivalGameMinutes;
        [SerializeField, Tooltip("Грунт ещё тёплый: лунную пыль можно собирать про запас")]
        private bool _groundIsWarm;

        public int Hour => _hour;
        public int Minute => _minute;
        public int MinuteOfDay => _hour * 60 + _minute;

        public DayPeriod Period => _period;
        public DangerCause DangerCause => _dangerCause;
        public bool GroundIsWarm => _groundIsWarm;
        public double SurvivalGameSeconds => Math.Max(0.01d, _survivalGameMinutes) * 60d;

        public DayPeriodDefinition(
            int hour,
            int minute,
            DayPeriod period,
            DangerCause dangerCause = DangerCause.Cold,
            float survivalGameMinutes = 30f,
            bool groundIsWarm = false)
        {
            _hour = hour;
            _minute = minute;
            _period = period;
            _dangerCause = dangerCause;
            _survivalGameMinutes = survivalGameMinutes;
            _groundIsWarm = groundIsWarm;
        }
    }

    [CreateAssetMenu(
        fileName = "DayScheduleConfig",
        menuName = "Configs/Gameplay/Day Schedule")]
    public sealed class DayScheduleConfig : ScriptableObject
    {
        [Tooltip("Время начала каждого периода")]
        [SerializeField]
        private DayPeriodDefinition[] _periods =
        {
            new DayPeriodDefinition(6, 0, DayPeriod.Safe),
            new DayPeriodDefinition(18, 0, DayPeriod.Dangerous)
        };

        public IReadOnlyList<DayPeriodDefinition> Periods => _periods;
    }
}
