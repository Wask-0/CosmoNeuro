using UnityEngine;
using UnityEngine.UI;

namespace _Project.Develop.Runtime.UI.Gameplay.Environment
{
    public sealed class ScreenTintView : MonoBehaviour
    {
        [SerializeField] private Image _image;
        [SerializeField, Min(0.1f)] private float _speed = 1.2f;

        private Color _target = new(0f, 0f, 0f, 0f);

        public void SetTarget(Color color) => _target = color;

        private void Update()
        {
            Color next = Color.Lerp(_image.color, _target, 1f - Mathf.Exp(-_speed * Time.unscaledDeltaTime));
            _image.color = next;
            bool visible = next.a > 0.004f;
            if (_image.enabled != visible) _image.enabled = visible;
        }
    }
}
