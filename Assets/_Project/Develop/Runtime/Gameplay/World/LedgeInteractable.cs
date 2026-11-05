using System.Threading;
using System.Threading.Tasks;
using _Project.Develop.Runtime.Gameplay.Hero;
using _Project.Develop.Runtime.Gameplay.Interaction.Core;
using _Project.Develop.Runtime.Gameplay.Tents;
using _Project.Develop.Runtime.Infrastructure.DI;
using DG.Tweening;
using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.World
{
    public enum LedgeDirection
    {
        Up,
        Down
    }

    // Уступ из диз-дока: сначала тюк, потом герой. Наверх тюк закидывают, вниз сбрасывают.
    public sealed class LedgeInteractable : Interactable, IWorldBindable
    {
        private const float ClimbSpeed = 2.4f;

        [SerializeField] private LedgeDirection _direction;
        [SerializeField] private Transform _interactionPoint;
        [SerializeField] private Transform _wallPoint;
        [SerializeField] private Transform _bagTarget;
        [SerializeField] private Transform _heroTarget;

        private HeroBag _bag;
        private HeroContext _hero;
        private HeroMoraleService _morale;
        private TentService _tents;

        public override Transform InteractionPoint => _interactionPoint;
        public override bool AcceptsBagButton => true;
        protected override string PrimaryKeyLabel => "ПКМ";

        public void Bind(DIContainer container)
        {
            _bag = container.Resolve<HeroBag>();
            _hero = container.Resolve<HeroContext>();
            _morale = container.Resolve<HeroMoraleService>();
            _tents = container.Resolve<TentService>();
        }

        // Палатка едет в том же тюке: пока она стоит, уступ не мешает войти в неё или собрать её.
        protected override bool IsAvailableNow(InteractionContext context)
            => _bag != null && !_tents.HasTent && (_bag.IsOnHero || ReferenceEquals(_bag.ActiveLedge, this));

        protected override string GetInteractionText()
        {
            if (_bag != null && _bag.IsOnHero)
                return _direction == LedgeDirection.Up ? "Закинуть тюк наверх" : "Сбросить тюк вниз";
            return _direction == LedgeDirection.Up ? "Залезть наверх" : "Спуститься на тюк";
        }

        public async Task PerformAsync(CancellationToken cancellationToken)
        {
            if (_bag.IsOnHero)
            {
                bool up = _direction == LedgeDirection.Up;
                float height = Mathf.Abs(_bagTarget.position.y - _hero.Character.Position.y);
                await _bag.ThrowAsync(_bagTarget.position, up ? 1.6f : 0.4f,
                    Mathf.Clamp(height * 0.16f, 0.6f, 1.3f), this, cancellationToken);
                return;
            }

            if (!ReferenceEquals(_bag.ActiveLedge, this))
                return;

            await MoveHeroAsync(cancellationToken);
            _bag.PickUp();
            _morale.AddChange();
        }

        private async Task MoveHeroAsync(CancellationToken cancellationToken)
        {
            var character = _hero.Character;
            var view = _hero.View;
            character.BeginScriptedMotion();
            _bag.NotifyClimbStarted();
            if (view != null) view.IsClimbing = _direction == LedgeDirection.Up;

            Transform body = character.transform;
            var completion = new TaskCompletionSource<bool>();
            Sequence sequence = DOTween.Sequence().SetLink(character.gameObject);

            if (_direction == LedgeDirection.Up)
            {
                float height = Mathf.Abs(_heroTarget.position.y - _wallPoint.position.y);
                sequence.Append(body.DOMove(_wallPoint.position, 0.25f).SetEase(Ease.Linear));
                sequence.Append(body.DOMoveY(_heroTarget.position.y, height / ClimbSpeed).SetEase(Ease.Linear));
                sequence.Append(body.DOMoveX(_heroTarget.position.x, 0.3f).SetEase(Ease.OutQuad));
            }
            else
            {
                sequence.Append(body.DOJump(_heroTarget.position, 0.5f, 1, 1f).SetEase(Ease.Linear));
            }

            sequence.OnComplete(() => completion.TrySetResult(true));

            try
            {
                using (cancellationToken.Register(() =>
                       {
                           sequence.Kill();
                           completion.TrySetCanceled();
                       }))
                {
                    await completion.Task;
                }
            }
            finally
            {
                if (view != null) view.IsClimbing = false;
                if (character != null) character.EndScriptedMotion();
            }
        }
    }
}
