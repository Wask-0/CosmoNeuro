using System.Collections.Generic;
using _Project.Develop.Runtime.Configs.Gameplay;
using _Project.Develop.Runtime.Gameplay.Characters.Core;
using _Project.Develop.Runtime.Gameplay.GameTime;
using _Project.Develop.Runtime.Gameplay.Hero;
using _Project.Develop.Runtime.Gameplay.Hero.Resources;
using _Project.Develop.Runtime.Gameplay.Resources;
using _Project.Develop.Runtime.Gameplay.Story;
using _Project.Develop.Runtime.Utilities.Controllers;
using _Project.Develop.Runtime.Utilities.Input.Contracts;
using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Dust
{
    // ЛКМ: МУ вытягивает энергию из намагниченного облака или набирает пыль, пока грунт тёплый.
    public sealed class MagneticDeviceController : Controller
    {
        private const string HintSource = "device";

        private readonly Character _character;
        private readonly CharacterView _view;
        private readonly ICharacterInput _input;
        private readonly HeroInventory _inventory;
        private readonly HeroResources _resources;
        private readonly HeroSurvivalConfig _config;
        private readonly DayScheduleService _schedule;
        private readonly GameTimeService _time;
        private readonly DustCollectionState _state;
        private readonly HintService _hints;
        private readonly DustStream _stream;
        private readonly List<Collider2D> _hits = new();
        private ContactFilter2D _filter;

        public MagneticDeviceController(Character character, ICharacterInput input, HeroInventory inventory,
            HeroResources resources, HeroSurvivalConfig config, DayScheduleService schedule, GameTimeService time,
            DustCollectionState state, HintService hints)
        {
            _character = character;
            _view = character.GetComponentInChildren<CharacterView>(true);
            _input = input;
            _inventory = inventory;
            _resources = resources;
            _config = config;
            _schedule = schedule;
            _time = time;
            _state = state;
            _hints = hints;
            _stream = new DustStream(character.transform);
            _filter = new ContactFilter2D { useTriggers = true };
            _filter.NoFilter();
        }

        protected override void UpdateLogic(float deltaTime)
        {
            if (_character == null || !_inventory.Has(HeroItem.MagneticDevice) || _character.IsScripted ||
                _time.IsFastForwarding)
            {
                Stop();
                _hints.Clear(HintSource);
                return;
            }

            DustSource source = FindNearest();
            if (source == null)
            {
                Stop();
                _hints.Clear(HintSource);
                return;
            }

            bool warm = _schedule.CurrentDefinition.GroundIsWarm;
            source.SetGroundWarm(warm);

            bool magnetic = source.Kind == DustKind.Magnetic;
            Resource storage = magnetic ? _resources.Energy : _resources.WarmDust;
            bool full = storage.Current.Value >= storage.Maximum;
            bool usable = magnetic || warm;

            _hints.Show(HintSource, HintFor(magnetic, usable, full));

            if (!_input.DeviceHeld || !usable || full)
            {
                Stop();
                return;
            }

            float rate = magnetic ? _config.EnergyCollectPerSecond : _config.WarmDustCollectPerSecond;
            float taken = source.Drain(rate * deltaTime);
            storage.Restore(taken);

            _character.Face(source.Center.x - _character.Position.x);
            _character.SetMoveDirection(0f);
            if (_view != null) _view.IsUsingDevice = true;

            _state.Begin(source.Kind);

            Color color = magnetic ? new Color(0.55f, 0.92f, 1f) : new Color(1f, 0.78f, 0.55f);
            Vector3 muzzle = _character.Position + new Vector3(_character.Facing * 0.9f, -0.1f, 0f);
            _stream.Show(source.Center, muzzle, color, deltaTime);

            if (source.IsDepleted)
                _state.NotifyDepleted(source.Kind);
        }

        private static string HintFor(bool magnetic, bool usable, bool full)
        {
            if (full)
                return magnetic ? "Накопитель МУ заполнен" : "Больше пыли не унести";
            if (!usable)
                return "Пыль остыла: собирать её нужно, пока грунт тёплый";
            return magnetic ? "[ЛКМ] Удерживать — собрать энергию" : "[ЛКМ] Удерживать — собрать тёплую пыль";
        }

        private void Stop()
        {
            if (_view != null) _view.IsUsingDevice = false;
            _stream.Hide();
            _state.End();
        }

        private DustSource FindNearest()
        {
            _hits.Clear();
            Physics2D.OverlapCircle(_character.Position, _config.DeviceRadius, _filter, _hits);
            DustSource nearest = null;
            float best = float.MaxValue;
            foreach (Collider2D hit in _hits)
            {
                if (hit == null) continue;
                DustSource candidate = hit.GetComponentInParent<DustSource>();
                if (candidate == null || candidate.IsDepleted) continue;
                float distance = ((Vector2)(candidate.transform.position - _character.Position)).sqrMagnitude;
                if (distance >= best) continue;
                best = distance;
                nearest = candidate;
            }

            return nearest;
        }

        protected override void OnDisable()
        {
            Stop();
            _hints.Clear(HintSource);
        }
    }
}
