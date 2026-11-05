using UnityEngine;

namespace _Project.Develop.Runtime.Utilities.FoundationUnity.Authoring
{
    public sealed class GameIdAttribute : PropertyAttribute
    {
        public string Category { get; }

        public GameIdAttribute(string category)
        {
            Category = category;
        }
    }
}
