using System;

namespace _Project.Develop.Runtime.Gameplay.Dust
{
    public sealed class DustCollectionState
    {
        public bool IsCollecting { get; private set; }
        public DustKind Kind { get; private set; }

        public event Action Started;
        public event Action Stopped;
        public event Action<DustKind> SourceDepleted;

        public void Begin(DustKind kind)
        {
            Kind = kind;
            if (IsCollecting) return;
            IsCollecting = true;
            Started?.Invoke();
        }

        public void End()
        {
            if (!IsCollecting) return;
            IsCollecting = false;
            Stopped?.Invoke();
        }

        public void NotifyDepleted(DustKind kind) => SourceDepleted?.Invoke(kind);
    }
}
