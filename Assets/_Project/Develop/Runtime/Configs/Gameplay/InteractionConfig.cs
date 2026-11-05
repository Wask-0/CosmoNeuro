using UnityEngine;

namespace _Project.Develop.Runtime.Configs.Gameplay
{
    [CreateAssetMenu(fileName = "InteractionConfig", menuName = "Configs/Gameplay/Interaction")]
    public sealed class InteractionConfig : ScriptableObject
    {
        [Min(0.1f)] public float Radius = 2.5f;
        public LayerMask DetectionMask = ~0;
        [Min(0f)] public float PromptHeight = 0.5f;
        [Min(0.1f)] public float DialogueCloseDistance = 4f;
    }
}
