using UnityEngine;
using UnityEngine.UI;

namespace _Project.Develop.Runtime.UI.Gameplay.Resources
{
    // Шкала с плещущейся жидкостью: поверхность колеблется и сильнее играет от рывков героя.
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class LiquidGaugeGraphic : MaskableGraphic
    {
        private const int Segments = 18;

        [SerializeField] private bool _horizontal;
        [SerializeField] private Color _surfaceColor = new(0.8f, 0.95f, 1f, 1f);
        [SerializeField, Min(0f)] private float _surfaceThickness = 3f;
        [SerializeField, Min(0f)] private float _idleAmplitude = 1.2f;
        [SerializeField, Min(0.1f)] private float _waveSpeed = 2.6f;
        [SerializeField, Min(0.1f)] private float _waveFrequency = 1.6f;
        [SerializeField, Min(0.1f)] private float _fillSpeed = 1.5f;

        private float _targetFill = 1f;
        private float _fill = 1f;
        private float _slosh;
        private float _time;

        public void SetFill(float fill) => _targetFill = Mathf.Clamp01(fill);

        public void Slosh(float amount) => _slosh = Mathf.Min(_slosh + amount, 9f);

        private void Update()
        {
            if (!Application.isPlaying) return;
            float deltaTime = Time.unscaledDeltaTime;
            _time += deltaTime;
            _fill = Mathf.MoveTowards(_fill, _targetFill, _fillSpeed * deltaTime);
            _slosh = Mathf.Lerp(_slosh, 0f, deltaTime * 1.4f);
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            if (_fill <= 0.0001f) return;

            Rect rect = GetPixelAdjustedRect();
            float length = _horizontal ? rect.width : rect.height;
            float span = _horizontal ? rect.height : rect.width;
            float level = length * _fill;
            float amplitude = (_idleAmplitude + _slosh) * Mathf.Clamp01(Mathf.Min(_fill, 1f - _fill) * 12f + 0.1f);

            for (int i = 0; i <= Segments; i++)
            {
                float t = (float)i / Segments;
                float wave = Mathf.Sin(t * Mathf.PI * 2f * _waveFrequency + _time * _waveSpeed) * amplitude +
                             Mathf.Sin(t * Mathf.PI * 2f * _waveFrequency * 0.5f - _time * _waveSpeed * 0.7f) *
                             amplitude * 0.5f;
                float surface = Mathf.Clamp(level + wave, 0f, length);
                float under = Mathf.Max(0f, surface - _surfaceThickness);
                float across = t * span;

                AddVertex(mesh, rect, across, 0f, color);
                AddVertex(mesh, rect, across, under, color);
                AddVertex(mesh, rect, across, under, _surfaceColor);
                AddVertex(mesh, rect, across, surface, _surfaceColor);
            }

            for (int i = 0; i < Segments; i++)
            {
                int a = i * 4;
                int b = a + 4;
                mesh.AddTriangle(a, a + 1, b + 1);
                mesh.AddTriangle(a, b + 1, b);
                mesh.AddTriangle(a + 2, a + 3, b + 3);
                mesh.AddTriangle(a + 2, b + 3, b + 2);
            }
        }

        private void AddVertex(VertexHelper mesh, Rect rect, float across, float along, Color tint)
        {
            Vector2 position = _horizontal
                ? new Vector2(rect.xMin + along, rect.yMin + across)
                : new Vector2(rect.xMin + across, rect.yMin + along);
            mesh.AddVert(position, tint, Vector2.zero);
        }
    }
}
