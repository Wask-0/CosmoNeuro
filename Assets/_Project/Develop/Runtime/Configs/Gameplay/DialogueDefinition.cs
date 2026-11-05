using _Project.Develop.Runtime.Gameplay.Dialogue;
using UnityEngine;

namespace _Project.Develop.Runtime.Configs.Gameplay
{
    [CreateAssetMenu(fileName = "Dialogue", menuName = "Configs/Gameplay/Dialogue")]
    public sealed class DialogueDefinition : ScriptableObject
    {
        public DialogueLine[] Lines = { new DialogueLine("", "Текст реплики") };
    }
}
