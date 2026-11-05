using _Project.Develop.Runtime.Configs.Gameplay;
using _Project.Develop.Runtime.Gameplay.GameTime;
using _Project.Develop.Runtime.Gameplay.Hero;
using _Project.Develop.Runtime.Gameplay.Resources;
using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Tents
{
    public sealed class TentService
    {
        private readonly TentFactory _factory;
        private readonly HeroResources _resources;
        private readonly GameTimeService _gameTime;
        private readonly DayScheduleService _schedule;
        private readonly HeroSurvivalConfig _config;
        private readonly HeroInventory _inventory;
        private readonly HeroBag _bag;

        private GameObject _tent;
        public bool HasTent => _tent != null;
        public bool CanPlace => !HasTent && _inventory.Has(HeroItem.Tent) && _bag.IsOnHero;

        public event System.Action Placed;
        public event System.Action Packed;
        public event System.Action RestStarted;

        public TentService(
            TentFactory factory,
            HeroResources resources,
            GameTimeService gameTime,
            HeroSurvivalConfig config,
            DayScheduleService schedule,
            HeroInventory inventory,
            HeroBag bag)
        {
            _factory = factory;
            _resources = resources;
            _gameTime = gameTime;
            _config = config;
            _schedule = schedule;
            _inventory = inventory;
            _bag = bag;
        }

        public bool IsBeyondLeash(Vector3 heroPosition, float direction)
        {
            if (_tent == null) return false;
            float offset = heroPosition.x - _tent.transform.position.x;
            return Mathf.Abs(offset) >= _config.TentLeashDistance && Mathf.Sign(offset) == Mathf.Sign(direction);
        }

        public void Use(Vector3 heroFeetPosition)
        {
            if (_gameTime.IsFastForwarding)
                return;

            if (_tent == null)
            {
                if (!CanPlace)
                    return;

                _tent = _factory.Create(heroFeetPosition);
                TentInteractable interactable = _tent.GetComponent<TentInteractable>();
                if (interactable == null)
                    throw new System.InvalidOperationException("Tent prefab requires TentInteractable.");
                interactable.Initialize(this);

                _gameTime.StartFastForward(
                    _config.SetupMinutes * 60d,
                    _config.TentWorkFastForwardSeconds,
                    TimeAdvanceReason.TentSetup);

                Placed?.Invoke();
                return;
            }

            if (!IsNearby(heroFeetPosition))
                return;

            if (!TryRest(_tent))
            {
                Debug.Log("Отдых невозможен: закончилась еда.");
            }
        }

        public void Pack(Vector3 heroFeetPosition)
        {
            if (_tent == null || !IsNearby(heroFeetPosition))
                return;

            TryPack(_tent);
        }

        public bool TryRest(GameObject tent)
        {
            if (tent == null || tent != _tent || _gameTime.IsFastForwarding)
                return false;

            var now = _gameTime.Current.DateTime;
            DayPeriodDefinition current = _schedule.GetDefinitionAt(now);
            double gameSeconds = _config.RestMinutes * 60d;
            if (current.Period != DayPeriod.Safe &&
                _schedule.FindNextSafeStart(now) is System.DateTime safeStart)
            {
                gameSeconds = _gameTime.SecondsUntil(safeStart);
            }
            else if (_schedule.FindNextDangerStart(now) is System.DateTime dangerStart)
            {
                // Лишний отдых заканчивается к началу опасной фазы, иначе к тоннелю в жару не успеть.
                gameSeconds = System.Math.Min(gameSeconds, _gameTime.SecondsUntil(dangerStart));
            }

            bool isCold = current.Period == DayPeriod.Dangerous && current.DangerCause == DangerCause.Cold;
            if (!_resources.TryRest(isCold))
                return false;

            _gameTime.StartFastForward(gameSeconds, _config.TentRestFastForwardSeconds, TimeAdvanceReason.TentRest);
            RestStarted?.Invoke();
            return true;
        }

        public bool TryPack(GameObject tent)
        {
            if (tent == null || tent != _tent || _gameTime.IsFastForwarding) return false;
            Object.Destroy(_tent);
            _tent = null;
            _gameTime.StartFastForward(_config.PackingMinutes * 60d, _config.TentWorkFastForwardSeconds, TimeAdvanceReason.TentPacking);
            Packed?.Invoke();
            return true;
        }

        private bool IsNearby(Vector3 heroFeetPosition)
        {
            Vector2 difference =
                (Vector2)(_tent.transform.position - heroFeetPosition);

            float distance = _config.InteractionDistance;

            return difference.sqrMagnitude <= distance * distance;
        }
    }
}
