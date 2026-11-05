using System;
using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Dialogue
{
    [Serializable]
    public struct DialogueLine
    {
        public string Speaker;
        [TextArea(3, 8)] public string Text;
        public DialogueLine(string speaker, string text) { Speaker = speaker; Text = text; }
    }
}
