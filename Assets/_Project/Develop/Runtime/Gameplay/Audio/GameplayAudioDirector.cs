using System;
using _Project.Develop.Runtime.Configs.Gameplay;
using _Project.Develop.Runtime.Gameplay.Dust;
using _Project.Develop.Runtime.Gameplay.GameTime;
using _Project.Develop.Runtime.Gameplay.Hero;
using _Project.Develop.Runtime.Gameplay.Story;
using _Project.Develop.Runtime.Gameplay.Tents;
using _Project.Develop.Runtime.Infrastructure.DI;
using _Project.Develop.Runtime.Infrastructure.Scopes;
using _Project.Develop.Runtime.Utilities.Audio;
using _Project.Develop.Runtime.Utilities.Controllers;
using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Audio
{
    // Эмбиент, тихая музыка, голос и естественные звуки героя. Клипы задаются в GameplayAudioConfig.
    public sealed class GameplayAudioDirector : Controller, IInitializable, IDisposable
    {
        private readonly GameplayAudioConfig _config;
        private readonly AudioService _audio;
        private readonly SceneUnityObjects _sceneObjects;
        private readonly HeroContext _hero;
        private readonly HeroBag _bag;
        private readonly HeroInventory _inventory;
        private readonly HeroLandingEvents _landing;
        private readonly DustCollectionState _dust;
        private readonly TentService _tents;
        private readonly DayScheduleService _schedule;
        private readonly MonologueService _monologues;
        private readonly VictoryService _victory;

        private AudioSource _device;
        private AudioSource _voice;
        private AudioSource _music;
        private AudioSource _ambient;
        private float _stepTimer;
        private int _sighIndex;

        public GameplayAudioDirector(GameplayAudioConfig config, AudioService audio, SceneUnityObjects sceneObjects,
            HeroContext hero, HeroBag bag, HeroInventory inventory, HeroLandingEvents landing,
            DustCollectionState dust, TentService tents, DayScheduleService schedule, MonologueService monologues,
            VictoryService victory)
        {
            _config = config;
            _audio = audio;
            _sceneObjects = sceneObjects;
            _hero = hero;
            _bag = bag;
            _inventory = inventory;
            _landing = landing;
            _dust = dust;
            _tents = tents;
            _schedule = schedule;
            _monologues = monologues;
            _victory = victory;
        }

        public void Initialize()
        {
            var root = new GameObject("Gameplay audio");
            _sceneObjects.Own(root.transform);

            _ambient = CreateSource(root, _audio.SoundGroup, _config.Ambient, _config.AmbientVolume, true);
            _music = CreateSource(root, _audio.MusicGroup, _config.Music, _config.MusicVolume, true);
            _device = CreateSource(root, _audio.SoundGroup, _config.DeviceLoop, 1f, true);
            _voice = CreateSource(root, _audio.SoundGroup, null, 1f, false);

            if (_ambient.clip != null) _ambient.Play();
            if (_music.clip != null) _music.Play();

            _bag.Thrown += OnBagThrown;
            _bag.Landed += OnBagLanded;
            _bag.PickedUp += OnBagPickedUp;
            _bag.ClimbStarted += OnClimbStarted;
            _inventory.Collected += OnItemCollected;
            _landing.HardLanded += OnHardLanded;
            _dust.Started += OnDustStarted;
            _dust.Stopped += OnDustStopped;
            _tents.Placed += OnTentChanged;
            _tents.Packed += OnTentChanged;
            _schedule.PeriodChanged += OnPeriodChanged;
            _monologues.VoiceRequested += OnVoiceRequested;
            _victory.Completed += OnVictory;
        }

        public void PlayJump() => _audio.SoundPlay(_config.JumpBreath, 0.95f, 1.05f);

        protected override void UpdateLogic(float deltaTime)
        {
            var character = _hero.Character;
            if (character == null || !character.HasMoveInput || !character.IsGrounded || character.IsScripted)
            {
                _stepTimer = 0f;
                return;
            }

            _stepTimer -= deltaTime;
            if (_stepTimer > 0f) return;
            _stepTimer = _config.StepInterval;
            _audio.SoundPlay(_config.Step, 0.9f, 1.1f);
        }

        private static AudioSource CreateSource(GameObject root, UnityEngine.Audio.AudioMixerGroup group,
            AudioClip clip, float volume, bool loop)
        {
            AudioSource source = root.AddComponent<AudioSource>();
            source.outputAudioMixerGroup = group;
            source.clip = clip;
            source.volume = volume;
            source.loop = loop;
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            return source;
        }

        private void OnBagThrown() => _audio.SoundPlay(_config.BagThrow);
        private void OnBagLanded() => _audio.SoundPlay(_config.BagLand);
        private void OnBagPickedUp() => _audio.SoundPlay(_config.Exhale);
        private void OnItemCollected(HeroItem item) => _audio.SoundPlay(_config.Pickup);
        private void OnHardLanded() => _audio.SoundPlay(_config.HardLanding);
        private void OnTentChanged() => _audio.SoundPlay(_config.Tent);
        private void OnVictory() => _audio.SoundPlay(_config.Victory);

        private void OnClimbStarted()
        {
            if (_config.ClimbSighs == null || _config.ClimbSighs.Length == 0) return;
            _audio.SoundPlay(_config.ClimbSighs[_sighIndex++ % _config.ClimbSighs.Length]);
        }

        private void OnDustStarted()
        {
            if (_device.clip != null) _device.Play();
        }

        private void OnDustStopped() => _device.Stop();

        private void OnPeriodChanged(DayPeriod period, DateTime time)
        {
            if (period == DayPeriod.Dangerous) _audio.SoundPlay(_config.Warning);
        }

        private void OnVoiceRequested(AudioClip clip)
        {
            _voice.Stop();
            _voice.clip = clip;
            _voice.Play();
        }

        public void Dispose()
        {
            _bag.Thrown -= OnBagThrown;
            _bag.Landed -= OnBagLanded;
            _bag.PickedUp -= OnBagPickedUp;
            _bag.ClimbStarted -= OnClimbStarted;
            _inventory.Collected -= OnItemCollected;
            _landing.HardLanded -= OnHardLanded;
            _dust.Started -= OnDustStarted;
            _dust.Stopped -= OnDustStopped;
            _tents.Placed -= OnTentChanged;
            _tents.Packed -= OnTentChanged;
            _schedule.PeriodChanged -= OnPeriodChanged;
            _monologues.VoiceRequested -= OnVoiceRequested;
            _victory.Completed -= OnVictory;
        }
    }
}
