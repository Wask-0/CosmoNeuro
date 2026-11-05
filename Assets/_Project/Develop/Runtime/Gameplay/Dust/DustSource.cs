using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Dust
{
    // Источник для МУ: облако намагниченных частиц или куча лунной пыли.
    public sealed class DustSource : MonoBehaviour
    {
        [SerializeField] private DustKind _kind;
        [SerializeField, Min(1f)] private float _capacity = 25f;
        [SerializeField] private SpriteRenderer _renderer;
        [SerializeField] private Sprite[] _frames = System.Array.Empty<Sprite>();
        [SerializeField, Min(0.1f)] private float _framesPerSecond = 5f;
        [SerializeField] private Color _warmTint = new(1f, 0.82f, 0.62f, 1f);
        [SerializeField] private Color _coldTint = new(0.62f, 0.72f, 0.9f, 1f);

        private float _remaining;
        private Vector3 _baseScale;
        private Vector3 _basePosition;
        private float _time;
        private bool _groundIsWarm = true;

        public DustKind Kind => _kind;
        public bool IsDepleted => _remaining <= 0f;
        public Vector3 Center => _renderer != null ? _renderer.bounds.center : transform.position;
        private float Fraction => Mathf.Clamp01(_remaining / _capacity);

        private void Awake()
        {
            _remaining = _capacity;
            _baseScale = _renderer.transform.localScale;
            _basePosition = _renderer.transform.localPosition;
            _time = transform.position.x;
            RefreshStage();
        }

        public void SetGroundWarm(bool warm)
        {
            if (_kind != DustKind.Warm) return;
            _groundIsWarm = warm;
            _renderer.color = warm ? _warmTint : _coldTint;
        }

        public float Drain(float amount)
        {
            if (IsDepleted) return 0f;
            float taken = Mathf.Min(amount, _remaining);
            _remaining -= taken;
            RefreshStage();
            if (IsDepleted) _renderer.enabled = false;
            return taken;
        }

        private void Update()
        {
            if (_kind != DustKind.Magnetic || IsDepleted || _frames.Length == 0)
                return;

            _time += Time.deltaTime;
            _renderer.sprite = _frames[(int)Mathf.Repeat(_time * _framesPerSecond, _frames.Length)];
            _renderer.transform.localPosition = _basePosition + Vector3.up * (Mathf.Sin(_time * 1.3f) * 0.18f);
        }

        private void RefreshStage()
        {
            if (_kind == DustKind.Magnetic)
            {
                // Облако редеет по мере сбора.
                _renderer.transform.localScale = _baseScale * Mathf.Lerp(0.5f, 1f, Fraction);
                return;
            }

            if (_frames.Length == 0) return;
            int index = Mathf.Clamp((int)((1f - Fraction) * _frames.Length), 0, _frames.Length - 1);
            _renderer.sprite = _frames[index];
            _renderer.color = _groundIsWarm ? _warmTint : _coldTint;
        }
    }
}
