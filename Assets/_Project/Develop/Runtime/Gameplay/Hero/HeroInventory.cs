using System;
using System.Collections.Generic;

namespace _Project.Develop.Runtime.Gameplay.Hero
{
    public enum HeroItem
    {
        Tent,
        Food,
        Oxygen,
        MagneticDevice
    }

    public sealed class HeroInventory
    {
        private const int RequiredItems = 4;

        private readonly HashSet<HeroItem> _items = new();

        public int Count => _items.Count;
        public bool HasAll => _items.Count >= RequiredItems;

        public event Action<HeroItem> Collected;

        public bool Has(HeroItem item) => _items.Contains(item);

        public void Add(HeroItem item)
        {
            if (_items.Add(item))
                Collected?.Invoke(item);
        }
    }
}
