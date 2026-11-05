using UnityEngine;
using UnityEngine.UI;

namespace _Project.Develop.Runtime.UI.Gameplay.Clock
{
    // Vector UI geometry stays sharp at different Canvas scales.
    [AddComponentMenu("UI/Game Clock Dial")]
    public sealed class ClockDialGraphic : MaskableGraphic
    {
        [SerializeField] private Color _rimColor = new Color(0.24f, 0.42f, 0.49f, 1f);
        [SerializeField] private Color _tickColor = new Color(0.7f, 0.84f, 0.86f, 1f);
        [SerializeField, Min(0f)] private float _rimWidth = 3f;
        [SerializeField] private bool _showTicks = true;

        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            Rect rect = GetPixelAdjustedRect();
            Vector2 center = rect.center;
            float radius = Mathf.Min(rect.width, rect.height) * 0.5f;
            if (radius <= 0f) return;
            Disk(mesh, center, radius, _rimColor);
            Disk(mesh, center, Mathf.Max(0f, radius - _rimWidth), color);
            if (!_showTicks) return;
            for (int i = 0; i < 96; i++)
            {
                bool hour = i % 4 == 0;
                float angle = i * Mathf.PI * 2f / 96f;
                Vector2 direction = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle));
                Vector2 tangent = new Vector2(direction.y, -direction.x);
                float outer = radius - _rimWidth - 6f;
                float inner = outer - (hour ? 11f : 4f);
                float width = hour ? 1.6f : 0.8f;
                int start = mesh.currentVertCount;
                mesh.AddVert(center + direction * inner - tangent * width * 0.5f, _tickColor, Vector2.zero);
                mesh.AddVert(center + direction * outer - tangent * width * 0.5f, _tickColor, Vector2.zero);
                mesh.AddVert(center + direction * outer + tangent * width * 0.5f, _tickColor, Vector2.zero);
                mesh.AddVert(center + direction * inner + tangent * width * 0.5f, _tickColor, Vector2.zero);
                mesh.AddTriangle(start, start + 1, start + 2);
                mesh.AddTriangle(start, start + 2, start + 3);
            }
        }

        private static void Disk(VertexHelper mesh, Vector2 center, float radius, Color tint)
        {
            const int segments = 96;
            int start = mesh.currentVertCount;
            mesh.AddVert(center, tint, Vector2.zero);
            for (int i = 0; i <= segments; i++)
            {
                float angle = i * Mathf.PI * 2f / segments;
                mesh.AddVert(center + new Vector2(Mathf.Sin(angle), Mathf.Cos(angle)) * radius,
                    tint, Vector2.zero);
            }
            for (int i = 0; i < segments; i++)
                mesh.AddTriangle(start, start + i + 1, start + i + 2);
        }
    }
}
