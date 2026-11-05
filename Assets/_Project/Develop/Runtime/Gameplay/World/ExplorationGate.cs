using _Project.Develop.Runtime.Gameplay.Hero;
using _Project.Develop.Runtime.Gameplay.Story;
using _Project.Develop.Runtime.Infrastructure.DI;
using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.World
{
    // Пока вещи не собраны, герой сам не уходит от места крушения.
    public sealed class ExplorationGate : HeroZone
    {
        [SerializeField] private Collider2D _blocker;

        private HeroInventory _inventory;
        private MonologueService _monologues;

        public override void Bind(DIContainer container)
        {
            base.Bind(container);
            _inventory = container.Resolve<HeroInventory>();
            _monologues = container.Resolve<MonologueService>();
            _inventory.Collected += OnCollected;
        }

        private void OnCollected(HeroItem item)
        {
            if (_inventory.HasAll && _blocker != null)
                _blocker.enabled = false;
        }

        protected override void OnHeroEntered()
        {
            if (!_inventory.HasAll)
                _monologues.Play("need_items", once: false);
        }

        private void OnDestroy()
        {
            if (_inventory != null) _inventory.Collected -= OnCollected;
        }
    }
}
