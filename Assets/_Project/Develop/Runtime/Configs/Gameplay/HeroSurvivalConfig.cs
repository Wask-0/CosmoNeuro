using UnityEngine;

namespace _Project.Develop.Runtime.Configs.Gameplay
{
    [CreateAssetMenu(
        fileName = "HeroSurvivalConfig",
        menuName = "Configs/Gameplay/Hero Survival")]
    public sealed class HeroSurvivalConfig : ScriptableObject
    {
        [Header("Начальные ресурсы")]
        [Min(0)] public int InitialFood = 0;
        [Min(1)] public int MaximumFood = 5;

        [Min(1f)] public float MaximumEnergy = 100f;
        [Min(0f)] public float InitialEnergy = 100f;

        [Min(1f)] public float MaximumOxygen = 100f;
        [Min(0f)] public float InitialOxygen = 100f;

        [Min(1f)] public float MaximumWarmDust = 100f;
        [Min(0f)] public float InitialWarmDust = 0f;

        [Header("Находки на месте крушения")]
        [Min(0)] public int FoodPickup = 5;
        [Min(0f)] public float OxygenPickup = 60f;

        [Header("Отдых в холод")]
        [Tooltip("Сколько тёплой пыли уходит на один отдых в холод")]
        [Min(0f)] public float WarmDustPerColdRest = 40f;
        [Tooltip("Во сколько раз больше еды тратится в холод без тёплой пыли")]
        [Min(1)] public int ColdRestFoodMultiplier = 2;

        [Header("Магнитное устройство")]
        [Min(0.1f)] public float DeviceRadius = 4f;
        [Min(0f)] public float EnergyCollectPerSecond = 8f;
        [Min(0f)] public float WarmDustCollectPerSecond = 16f;

        [Header("Жёсткое приземление")]
        [Min(0.1f)] public float HardLandingSpeed = 5f;
        [Min(0f)] public float HardLandingOxygenCost = 15f;
        [Min(0f)] public float HardLandingMoraleCost = 10f;

        [Header("Тоннель")]
        [Tooltip("Игровые минуты до смерти от холода в тоннеле вне жары")]
        [Min(0.1f)] public float TunnelColdSurvivalMinutes = 30f;

        [Header("Расход за игровой час")]
        [Min(0f)] public float OxygenPerHour = 5f;

        [Header("Пропуск времени — игровые минуты")]
        [Min(0f)] public float SetupMinutes = 5f;
        [Min(0f)] public float RestMinutes = 120f;
        [Min(0f)] public float PackingMinutes = 5f;

        [Header("Длительность перемотки — реальные секунды")]
        [Min(0.1f)] public float TentWorkFastForwardSeconds = 1f;
        [Min(0.1f)] public float TentRestFastForwardSeconds = 4f;

        [Header("Взаимодействие")]
        [Min(0.1f)] public float InteractionDistance = 2f;
        [Tooltip("Дальше этого расстояния от палатки уйти нельзя, пока она не собрана")]
        [Min(0.1f)] public float TentLeashDistance = 2f;
    }
}
