using System;
using UnityEngine;

namespace _Project.Develop.Runtime.Configs.Gameplay
{
    [Serializable]
    public struct MonologueLine
    {
        [TextArea(2, 5)] public string Text;
        [Tooltip("Запись голоса. Пока пусто — показываются только субтитры")]
        public AudioClip Voice;
    }

    [Serializable]
    public struct MonologueEntry
    {
        public string Id;
        public MonologueLine[] Lines;
    }

    // Все реплики героя лежат в одном ассете, чтобы озвучку и правки текста делать в одном месте.
    [CreateAssetMenu(fileName = "StoryConfig", menuName = "Configs/Gameplay/Story")]
    public sealed class StoryConfig : ScriptableObject
    {
        public MonologueEntry[] Monologues = Array.Empty<MonologueEntry>();

        [Tooltip("Монологи о семье и работе, идут по порядку во время пути")]
        public string[] AmbientMonologues = Array.Empty<string>();
        [Min(1f)] public float AmbientIntervalMin = 40f;
        [Min(1f)] public float AmbientIntervalMax = 70f;

        [Header("Субтитры без записи голоса")]
        [Min(0.1f)] public float SecondsPerCharacter = 0.065f;
        [Min(0.1f)] public float MinimumLineSeconds = 2.6f;

        public bool TryGet(string id, out MonologueEntry entry)
        {
            foreach (MonologueEntry item in Monologues)
            {
                if (item.Id != id) continue;
                entry = item;
                return true;
            }

            entry = default;
            return false;
        }
    }
}
