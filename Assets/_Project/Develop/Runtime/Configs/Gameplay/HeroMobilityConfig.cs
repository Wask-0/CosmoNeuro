using UnityEngine;

namespace _Project.Develop.Runtime.Configs.Gameplay
{
    [CreateAssetMenu(fileName = "HeroMobilityConfig", menuName = "Configs/Gameplay/Hero Mobility")]
    public sealed class HeroMobilityConfig : ScriptableObject
    {
        [Header("Вес еды: чем меньше еды в тюке, тем легче идти")]
        [Range(0.1f, 1f)] public float SpeedFactorWithFullFood = 0.7f;
        [Range(0.1f, 1f)] public float SpeedFactorWithoutFood = 1f;
        [Min(0.1f)] public float JumpHeightWithFullFood = 2.5f;
        [Min(0.1f)] public float JumpHeightWithoutFood = 4.6f;

        [Header("Моральное состояние")]
        [Min(1f)] public float MaximumMorale = 100f;
        [Tooltip("Падение за игровой час вне палатки: однообразие и тоска по дому")]
        [Min(0f)] public float MoraleDecayPerHour = 6f;
        [Min(0f)] public float RestMoraleGain = 20f;
        [Tooltip("Прибавка за перемены: новый участок пути, преодолённый уступ, находка")]
        [Min(0f)] public float ChangeMoraleGain = 6f;

        [Header("Пороги и скорость")]
        [Range(0f, 100f)] public float ExcellentFrom = 66f;
        [Range(0f, 100f)] public float GoodFrom = 33f;
        [Range(0.1f, 1f)] public float ExcellentSpeed = 1f;
        [Range(0.1f, 1f)] public float GoodSpeed = 0.8f;
        [Range(0.1f, 1f)] public float BadSpeed = 0.5f;
    }
}
