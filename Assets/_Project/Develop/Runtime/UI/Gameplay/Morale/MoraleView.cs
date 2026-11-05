using _Project.Develop.Runtime.Gameplay.Hero;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Develop.Runtime.UI.Gameplay.Morale
{
    public sealed class MoraleView : MonoBehaviour
    {
        [SerializeField] private Image _face;
        [SerializeField] private RectTransform _arrow;
        [SerializeField] private Sprite _excellent;
        [SerializeField] private Sprite _good;
        [SerializeField] private Sprite _bad;

        public void SetLevel(MoraleLevel level)
        {
            _face.sprite = level switch
            {
                MoraleLevel.Excellent => _excellent,
                MoraleLevel.Good => _good,
                _ => _bad
            };
        }

        public void SetRising(bool rising) => _arrow.localRotation = Quaternion.Euler(0f, 0f, rising ? 0f : 180f);
    }
}
