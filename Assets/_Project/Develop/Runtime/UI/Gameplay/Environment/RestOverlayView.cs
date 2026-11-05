using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Develop.Runtime.UI.Gameplay.Environment
{
    // Таймскип в палатке: экран темнеет, поверх него идут только часы.
    public sealed class RestOverlayView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _group;
        [SerializeField] private TMP_Text _text;
        [SerializeField, Min(0.1f)] private float _fadeSpeed = 3f;

        private float _target;

        public void Show(string text)
        {
            _text.text = text;
            _target = 1f;
        }

        public void Hide() => _target = 0f;

        private void Update()
        {
            _group.alpha = Mathf.MoveTowards(_group.alpha, _target, _fadeSpeed * Time.unscaledDeltaTime);
        }
    }
}
