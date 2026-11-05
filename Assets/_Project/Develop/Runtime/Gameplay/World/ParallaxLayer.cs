using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.World
{
    // Дальний план: сдвигается медленнее камеры и незаметно повторяется.
    public sealed class ParallaxLayer : MonoBehaviour
    {
        [SerializeField, Range(0f, 1f), Tooltip("0 — стоит как мир, 1 — едет вместе с камерой")]
        private float _followX = 0.8f;
        [SerializeField, Range(0f, 1f)] private float _followY = 0.75f;
        [SerializeField, Min(0f), Tooltip("Ширина повторяемого спрайта; 0 — слой не повторяется")]
        private float _repeatWidth;
        [SerializeField] private Vector2 _offset;

        private Camera _camera;

        private void LateUpdate()
        {
            if (_camera == null)
            {
                _camera = Camera.main;
                if (_camera == null) return;
            }

            Vector3 camera = _camera.transform.position;
            float x;
            if (_repeatWidth > 0f)
            {
                float scrolled = camera.x * (1f - _followX);
                x = camera.x - Mathf.Repeat(scrolled, _repeatWidth);
            }
            else
            {
                x = camera.x * _followX;
            }

            Vector3 position = transform.position;
            position.x = x + _offset.x;
            position.y = camera.y * _followY + _offset.y;
            transform.position = position;
        }
    }
}
