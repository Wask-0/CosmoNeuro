using UnityEngine;

namespace _Project.Develop.Runtime.Configs.Gameplay
{
    [CreateAssetMenu(fileName = "HeroDeathConfig", menuName = "Configs/Gameplay/Hero Death")]
    public sealed class HeroDeathConfig : ScriptableObject
    {
        [Min(0f)] public float PopupDelayRealSeconds = 1.5f;
        public Sprite ColdImage;
        public Sprite HeatImage;
    }
}
