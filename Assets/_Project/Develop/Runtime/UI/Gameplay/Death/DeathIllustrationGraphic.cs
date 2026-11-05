using _Project.Develop.Runtime.Gameplay.GameTime;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Develop.Runtime.UI.Gameplay.Death
{
    public sealed class DeathIllustrationGraphic : MaskableGraphic
    {
        private DangerCause _cause;
        public void SetCause(DangerCause cause) { _cause = cause; SetVerticesDirty(); }
        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            Rect rect = GetPixelAdjustedRect();
            Vector2 center = rect.center;
            float r = Mathf.Min(rect.width, rect.height) * 0.42f;
            Color accent = _cause switch
            {
                DangerCause.Cold => new Color(0.35f, 0.8f, 1f),
                DangerCause.Heat => new Color(1f, 0.3f, 0.12f),
                _ => new Color(0.75f, 0.75f, 0.85f)
            };
            Disk(mesh, center, r, accent);
            Disk(mesh, center, r * 0.88f, new Color(0.08f, 0.14f, 0.2f));
            Disk(mesh, center + Vector2.up * r * 0.08f, r * 0.66f, new Color(0.02f, 0.035f, 0.055f));
            Line(mesh, center + new Vector2(-0.35f, 0.32f) * r, center + new Vector2(0.1f, -0.06f) * r, 3f, accent);
            Line(mesh, center + new Vector2(0.1f, -0.06f) * r, center + new Vector2(-0.02f, -0.5f) * r, 3f, accent);
            Line(mesh, center + new Vector2(0.1f, -0.06f) * r, center + new Vector2(0.45f, -0.2f) * r, 2f, accent);
            for (int i = 0; i < 8; i++)
            {
                float a = i * Mathf.PI / 4f;
                Vector2 d = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
                Line(mesh, center + d * r * 1.16f, center + d * r * 1.35f, 3f, accent);
            }
        }
        private static void Disk(VertexHelper mesh, Vector2 center, float radius, Color tint)
        {
            int start = mesh.currentVertCount;
            mesh.AddVert(center, tint, Vector2.zero);
            for (int i = 0; i <= 64; i++)
            {
                float a = i * Mathf.PI * 2f / 64f;
                mesh.AddVert(center + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * radius, tint, Vector2.zero);
            }
            for (int i = 0; i < 64; i++) mesh.AddTriangle(start, start + i + 1, start + i + 2);
        }
        private static void Line(VertexHelper mesh, Vector2 from, Vector2 to, float width, Color tint)
        {
            Vector2 direction = (to - from).normalized;
            Vector2 side = new Vector2(-direction.y, direction.x) * width * 0.5f;
            int start = mesh.currentVertCount;
            mesh.AddVert(from - side, tint, Vector2.zero); mesh.AddVert(to - side, tint, Vector2.zero);
            mesh.AddVert(to + side, tint, Vector2.zero); mesh.AddVert(from + side, tint, Vector2.zero);
            mesh.AddTriangle(start, start + 1, start + 2); mesh.AddTriangle(start, start + 2, start + 3);
        }
    }
}
