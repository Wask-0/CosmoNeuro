using TMPro;
using UnityEngine;

namespace _Project.Develop.Runtime.UI.Gameplay.Interaction
{
    public sealed class InteractionView : MonoBehaviour
    {
        [SerializeField] private GameObject _container;
        [SerializeField] private RectTransform _panel;
        [SerializeField] private TMP_Text _text;
        private Transform _target;
        private Canvas _canvas;
        private Camera _camera;
        private float _height;

        public void Show(Transform target, string action, bool canExecute, float height, string secondary = null, string primaryKey = "E")
        {
            _target = target;
            _height = height;
            _text.text = $"[{primaryKey}] {action}" + (string.IsNullOrEmpty(secondary) ? "" : $"\n[Q] {secondary}");
            _panel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, string.IsNullOrEmpty(secondary) ? 52f : 88f);
            _text.color = canExecute ? Color.white : new Color(1f, 0.6f, 0.45f);
            _container.SetActive(true);
            UpdatePosition();
        }

        public void Hide() { _target = null; _container.SetActive(false); }
        private void LateUpdate() { if (_target != null) UpdatePosition(); else _container.SetActive(false); }
        private void UpdatePosition()
        {
            if (_target == null) return;
            if (_canvas == null) _canvas = GetComponentInParent<Canvas>();
            if (_camera == null) _camera = Camera.main;
            if (_camera == null || _canvas == null) { _container.SetActive(false); return; }
            Vector3 screen = _camera.WorldToScreenPoint(_target.position + Vector3.up * _height);
            _container.SetActive(screen.z > 0f);
            Camera uiCamera = _canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _canvas.worldCamera;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                (RectTransform)_panel.parent, screen, uiCamera, out Vector2 local))
                _panel.anchoredPosition = local;
        }
    }
}
