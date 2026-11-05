using System;
using System.Collections.Generic;
using _Project.Develop.Runtime.Configs.Gameplay;
using _Project.Develop.Runtime.Utilities.Controllers;
using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Story
{
    // Голос героя и субтитры. Не блокирует игру: герой говорит на ходу.
    public sealed class MonologueService : Controller
    {
        private readonly StoryConfig _config;
        private readonly Queue<MonologueLine> _queue = new();
        private readonly HashSet<string> _played = new();
        private float _lineTime;

        public string CurrentText { get; private set; }
        public bool IsPlaying => !string.IsNullOrEmpty(CurrentText);

        public event Action Changed;
        public event Action<AudioClip> VoiceRequested;

        public MonologueService(StoryConfig config) => _config = config;

        public bool Play(string id, bool once = true)
        {
            if (string.IsNullOrEmpty(id))
                return false;
            if (once && _played.Contains(id))
                return false;
            if (!_config.TryGet(id, out MonologueEntry entry) || entry.Lines == null || entry.Lines.Length == 0)
            {
                Debug.LogWarning($"Монолог '{id}' не найден в StoryConfig.");
                return false;
            }

            _played.Add(id);
            foreach (MonologueLine line in entry.Lines)
                _queue.Enqueue(line);

            if (!IsPlaying) Next();
            return true;
        }

        // Важная реплика не должна ждать очереди из нескольких монологов.
        public void PlayNow(string id)
        {
            _queue.Clear();
            CurrentText = null;
            Play(id, once: false);
        }

        protected override void UpdateLogic(float deltaTime)
        {
            if (!IsPlaying) return;
            _lineTime -= deltaTime;
            if (_lineTime <= 0f) Next();
        }

        private void Next()
        {
            if (_queue.Count == 0)
            {
                CurrentText = null;
                Changed?.Invoke();
                return;
            }

            MonologueLine line = _queue.Dequeue();
            CurrentText = line.Text;
            _lineTime = line.Voice != null
                ? line.Voice.length + 0.25f
                : Mathf.Max(_config.MinimumLineSeconds, line.Text.Length * _config.SecondsPerCharacter);
            if (line.Voice != null) VoiceRequested?.Invoke(line.Voice);
            Changed?.Invoke();
        }
    }
}
