using _Project.Develop.Runtime.Gameplay.Characters.Core;
using UnityEngine;

namespace _Project.Develop.Runtime.Configs.Gameplay.Characters
{
    [CreateAssetMenu(menuName = "Configs/Characters/Character", fileName = "CharacterConfig")]
    public class CharacterConfig : ScriptableObject
    {
        [field: SerializeField] public Character Prefab { get; private set; }
        [field: SerializeField, Min(0.01f)] public float MaxSpeed { get; private set; } = 5f;
        [field: SerializeField, Min(0.01f)] public float Acceleration { get; private set; } = 15f;
        [field: SerializeField, Min(0.01f)] public float Deceleration { get; private set; } = 6f;
        [field: SerializeField, Min(0.01f)] public float TurnDeceleration { get; private set; } = 20f;
        [field: SerializeField, Min(0f)] public float GravityScale { get; private set; } = 0.3f;
    }
}
