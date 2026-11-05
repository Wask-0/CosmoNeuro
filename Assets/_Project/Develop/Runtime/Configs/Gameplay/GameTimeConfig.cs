using System;
using UnityEngine;

namespace _Project.Develop.Runtime.Configs.Gameplay
{
    [CreateAssetMenu(
        fileName = "GameTimeConfig",
        menuName = "Configs/Gameplay/Game Time")]
    public sealed class GameTimeConfig : ScriptableObject
    {
        [Header("Начальная дата")]
        [SerializeField, Range(1, 9999)] private int _year = 2036;
        [SerializeField, Range(1, 12)] private int _month = 11;
        [SerializeField, Range(1, 31)] private int _day = 10;

        [Header("Начальное время")]
        [SerializeField, Range(0, 23)] private int _hour = 8;
        [SerializeField, Range(0, 59)] private int _minute;

        [Header("Скорость времени")]
        [Tooltip("Сколько игровых секунд проходит за реальную секунду")]
        [SerializeField, Min(0.01f)]
        private float _gameSecondsPerRealSecond = 60f;

        public double GameSecondsPerRealSecond =>
            _gameSecondsPerRealSecond;

        public DateTime CreateStartDateTime()
        {
            try
            {
                return new DateTime(
                    _year,
                    _month,
                    _day,
                    _hour,
                    _minute,
                    0,
                    DateTimeKind.Unspecified);
            }
            catch (ArgumentOutOfRangeException exception)
            {
                throw new InvalidOperationException(
                    $"В конфиге {name} указана некорректная дата.",
                    exception);
            }
        }
    }
}