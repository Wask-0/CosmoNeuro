using _Project.Develop.Runtime.Gameplay.Hero;
using _Project.Develop.Runtime.Infrastructure.DI;

namespace _Project.Develop.Runtime.Gameplay.World
{
    public sealed class TunnelZone : HeroZone
    {
        private HeroEnvironment _environment;

        public override void Bind(DIContainer container)
        {
            base.Bind(container);
            _environment = container.Resolve<HeroEnvironment>();
        }

        protected override void OnHeroEntered() => _environment.SetInTunnel(true);
        protected override void OnHeroExited() => _environment.SetInTunnel(false);
    }
}
