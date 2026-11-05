using UnityEngine;

namespace _Project.Develop.Runtime.UI.Gameplay.Environment
{
    // Стрелка «направление движения» из макета интерфейса. Показывается, когда герой стоит без дела.
    public sealed class DirectionHintView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _group;
        [SerializeField, Min(0.1f)] private float _fadeSpeed = 1.5f;
        [SerializeField, Range(0f, 1f)] private float _visibleAlpha = 0.55f;

        private float _target;

        public void SetVisible(bool visible) => _target = visible ? _visibleAlpha : 0f;

        private void Update()
        {
            _group.alpha = Mathf.MoveTowards(_group.alpha, _target, _fadeSpeed * Time.unscaledDeltaTime);
        }
    }
}
