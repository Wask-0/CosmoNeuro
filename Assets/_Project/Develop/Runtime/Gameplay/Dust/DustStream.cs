using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Dust
{
    // Поток частиц от источника к МУ.
    public sealed class DustStream
    {
        private const int Count = 14;
        private const float Speed = 1.5f;

        private readonly SpriteRenderer[] _particles = new SpriteRenderer[Count];
        private readonly float[] _phase = new float[Count];
        private readonly Vector2[] _jitter = new Vector2[Count];
        private readonly GameObject _root;

        public DustStream(Transform parent)
        {
            _root = new GameObject("Dust stream");
            _root.transform.SetParent(parent, false);
            Sprite sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0f, 0f, 4f, 4f),
                new Vector2(0.5f, 0.5f), 32f);
            for (int i = 0; i < Count; i++)
            {
                var particle = new GameObject("Particle");
                particle.transform.SetParent(_root.transform, false);
                _particles[i] = particle.AddComponent<SpriteRenderer>();
                _particles[i].sprite = sprite;
                _particles[i].sortingOrder = 5;
                _phase[i] = (float)i / Count;
                _jitter[i] = Random.insideUnitCircle * 1.1f;
            }
            _root.SetActive(false);
        }

        public void Show(Vector3 from, Vector3 to, Color color, float deltaTime)
        {
            if (!_root.activeSelf) _root.SetActive(true);
            for (int i = 0; i < Count; i++)
            {
                _phase[i] = Mathf.Repeat(_phase[i] + deltaTime * Speed, 1f);
                float t = _phase[i];
                Vector3 start = from + (Vector3)_jitter[i];
                _particles[i].transform.position = Vector3.Lerp(start, to, t * t);
                color.a = Mathf.Clamp01(t * 4f);
                _particles[i].color = color;
            }
        }

        public void Hide()
        {
            if (_root != null && _root.activeSelf) _root.SetActive(false);
        }
    }
}
