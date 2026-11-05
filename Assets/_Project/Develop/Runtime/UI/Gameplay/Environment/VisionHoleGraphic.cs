using UnityEngine;
using UnityEngine.UI;

namespace _Project.Develop.Runtime.UI.Gameplay.Environment
{
    // Темнота с просветом вокруг героя: в тоннеле костюм глушит излучение, и обзор падает.
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class VisionHoleGraphic : MaskableGraphic
    {
        private const int Segments = 48;
        private const float FarDistance = 5000f;

        [SerializeField, Min(1f)] private float _softness = 130f;

        private Vector2 _center;
        private float _radius = 2000f;

        public void SetHole(Vector2 center, float radius)
        {
            _center = center;
            _radius = radius;
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            Color clear = color;
            clear.a = 0f;

            for (int i = 0; i <= Segments; i++)
            {
                float angle = i * Mathf.PI * 2f / Segments;
                Vector2 direction = new(Mathf.Cos(angle), Mathf.Sin(angle));
                mesh.AddVert(_center + direction * _radius, clear, Vector2.zero);
                mesh.AddVert(_center + direction * (_radius + _softness), color, Vector2.zero);
                mesh.AddVert(_center + direction * FarDistance, color, Vector2.zero);
            }

            for (int i = 0; i < Segments; i++)
            {
                int a = i * 3;
                int b = a + 3;
                mesh.AddTriangle(a, a + 1, b + 1);
                mesh.AddTriangle(a, b + 1, b);
                mesh.AddTriangle(a + 1, a + 2, b + 2);
                mesh.AddTriangle(a + 1, b + 2, b + 1);
            }
        }
    }
}
