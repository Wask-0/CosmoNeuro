using _Project.Develop.Runtime.Gameplay.Hero;
using _Project.Develop.Runtime.Gameplay.Story;
using _Project.Develop.Runtime.Infrastructure.DI;
using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.World
{
    public sealed class MonologueZone : HeroZone
    {
        [SerializeField] private string _monologueId;
        [SerializeField, Tooltip("Новый участок пути поднимает настроение")] private bool _isNewPlace;

        private MonologueService _monologues;
        private HeroMoraleService _morale;

        public override void Bind(DIContainer container)
        {
            base.Bind(container);
            _monologues = container.Resolve<MonologueService>();
            _morale = container.Resolve<HeroMoraleService>();
        }

        protected override void OnHeroEntered()
        {
            bool played = _monologues.Play(_monologueId);
            if (played && _isNewPlace) _morale.AddChange();
        }
    }
}
