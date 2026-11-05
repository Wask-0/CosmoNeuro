using System;
using System.Threading;
using System.Threading.Tasks;
using _Project.Develop.Runtime.Gameplay.Characters.Core;
using DG.Tweening;
using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Hero
{
    // Тюк с палаткой, едой и баллонами. Его закидывают на уступ или сбрасывают вниз раньше героя.
    public sealed class HeroBag : IDisposable
    {
        private BagView _view;
        private Character _hero;

        public bool IsOnHero { get; private set; } = true;
        public bool IsInFlight { get; private set; }
        public object ActiveLedge { get; private set; }
        public Vector3 Position => _view != null ? _view.transform.position : Vector3.zero;

        public event Action Thrown;
        public event Action Landed;
        public event Action PickedUp;
        public event Action ClimbStarted;

        public void Bind(BagView view, Character hero)
        {
            _view = view;
            _hero = hero;
            _view.Attach(hero);
            _view.SetVisible(false);
        }

        public void Show() => _view?.SetVisible(true);

        public async Task ThrowAsync(Vector3 target, float arcHeight, float duration, object ledge,
            CancellationToken cancellationToken)
        {
            if (!IsOnHero || _view == null)
                return;

            IsOnHero = false;
            IsInFlight = true;
            ActiveLedge = ledge;
            _view.Detach();
            Thrown?.Invoke();

            var completion = new TaskCompletionSource<bool>();
            Tween tween = _view.transform
                .DOJump(target, arcHeight, 1, duration)
                .SetEase(Ease.Linear)
                .SetLink(_view.gameObject)
                .OnComplete(() => completion.TrySetResult(true));

            try
            {
                using (cancellationToken.Register(() =>
                       {
                           tween.Kill();
                           completion.TrySetCanceled();
                       }))
                {
                    await completion.Task;
                }
            }
            finally
            {
                IsInFlight = false;
            }

            Landed?.Invoke();
        }

        public void NotifyClimbStarted() => ClimbStarted?.Invoke();

        public void PickUp()
        {
            if (IsOnHero || _view == null)
                return;

            IsOnHero = true;
            ActiveLedge = null;
            _view.Attach(_hero);
            PickedUp?.Invoke();
        }

        public void Dispose()
        {
            Thrown = null;
            Landed = null;
            PickedUp = null;
            ClimbStarted = null;
        }
    }
}
