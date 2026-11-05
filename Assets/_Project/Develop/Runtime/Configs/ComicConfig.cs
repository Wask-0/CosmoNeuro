using System;
using _Project.Develop.Runtime.Comics;
using UnityEngine;

namespace _Project.Develop.Runtime.Configs
{
    [Serializable]
    public struct ComicPanel
    {
        public Sprite Image;
        [TextArea(2, 4)] public string Caption;
    }

    [Serializable]
    public struct ComicDefinition
    {
        public ComicId Id;
        public ComicPanel[] Panels;
    }

    [CreateAssetMenu(fileName = "ComicConfig", menuName = "Configs/Comic")]
    public sealed class ComicConfig : ScriptableObject
    {
        public ComicDefinition[] Comics = Array.Empty<ComicDefinition>();

        public ComicDefinition Get(ComicId id)
        {
            foreach (ComicDefinition comic in Comics)
                if (comic.Id == id)
                    return comic;

            throw new InvalidOperationException($"Comic '{id}' is not configured.");
        }
    }
}
