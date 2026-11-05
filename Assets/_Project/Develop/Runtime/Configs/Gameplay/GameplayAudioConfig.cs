using UnityEngine;

namespace _Project.Develop.Runtime.Configs.Gameplay
{
    [CreateAssetMenu(fileName = "GameplayAudioConfig", menuName = "Configs/Gameplay/Audio")]
    public sealed class GameplayAudioConfig : ScriptableObject
    {
        [Header("Фон")]
        public AudioClip Ambient;
        public AudioClip Music;
        [Range(0f, 1f)] public float AmbientVolume = 0.55f;
        [Range(0f, 1f)] public float MusicVolume = 0.3f;

        [Header("Герой")]
        public AudioClip Step;
        [Min(0.05f)] public float StepInterval = 0.62f;
        public AudioClip JumpBreath;
        public AudioClip Exhale;
        public AudioClip[] ClimbSighs;
        public AudioClip HardLanding;

        [Header("Тюк и находки")]
        public AudioClip BagThrow;
        public AudioClip BagLand;
        public AudioClip Pickup;

        [Header("Механики")]
        public AudioClip Tent;
        public AudioClip DeviceLoop;
        public AudioClip Warning;
        public AudioClip Victory;
    }
}
