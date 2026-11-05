using System;

namespace _Project.Develop.Runtime.Gameplay.Hero
{
    public sealed class HeroEnvironment
    {
        public bool InTunnel { get; private set; }

        public event Action Changed;

        public void SetInTunnel(bool inTunnel)
        {
            if (InTunnel == inTunnel) return;
            InTunnel = inTunnel;
            Changed?.Invoke();
        }
    }
}
