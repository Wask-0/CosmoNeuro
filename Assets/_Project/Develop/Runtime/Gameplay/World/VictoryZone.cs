using _Project.Develop.Runtime.Gameplay.Story;
using _Project.Develop.Runtime.Infrastructure.DI;

namespace _Project.Develop.Runtime.Gameplay.World
{
    public sealed class VictoryZone : HeroZone
    {
        private VictoryService _victory;

        public override void Bind(DIContainer container)
        {
            base.Bind(container);
            _victory = container.Resolve<VictoryService>();
        }

        protected override void OnHeroEntered() => _victory.Complete();
    }
}
