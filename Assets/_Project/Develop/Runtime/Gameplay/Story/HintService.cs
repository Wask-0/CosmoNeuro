using System;
using System.Collections.Generic;

namespace _Project.Develop.Runtime.Gameplay.Story
{
    // Короткие подсказки по управлению. Показывается последняя из активных.
    public sealed class HintService
    {
        private readonly List<KeyValuePair<string, string>> _active = new();

        public string Current => _active.Count > 0 ? _active[_active.Count - 1].Value : null;

        public event Action Changed;

        public void Show(string source, string text)
        {
            int index = _active.FindIndex(pair => pair.Key == source);
            if (index >= 0)
            {
                if (_active[index].Value == text) return;
                _active[index] = new KeyValuePair<string, string>(source, text);
            }
            else
            {
                _active.Add(new KeyValuePair<string, string>(source, text));
            }

            Changed?.Invoke();
        }

        public void Clear(string source)
        {
            if (_active.RemoveAll(pair => pair.Key == source) > 0)
                Changed?.Invoke();
        }
    }
}
