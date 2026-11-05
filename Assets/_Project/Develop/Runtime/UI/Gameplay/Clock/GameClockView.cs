using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Develop.Runtime.UI.Gameplay.Clock
{
    public sealed class GameClockView : MonoBehaviour
    {
        [SerializeField] private RectTransform _hand;
        [SerializeField] private TMP_Text _dateText;
        [SerializeField] private Image _dialImage;
        [SerializeField] private Image _handImage;
        [SerializeField, Tooltip("Угол стрелки в 00:00. Позволяет совместить время с разметкой своего циферблата.")]
        private float _midnightAngle;
        [SerializeField] private bool _clockwise = true;

        public void SetDayProgress(double progress)
        {
            float angle = _midnightAngle + (float)(progress * 360d) * (_clockwise ? -1f : 1f);

            _hand.localRotation = Quaternion.Euler(
                0f,
                0f,
                angle);
        }

        public void SetDialSprite(Sprite sprite) => _dialImage.sprite = sprite;
        public void SetHandSprite(Sprite sprite) => _handImage.sprite = sprite;

        public void SetDate(string date)
        {
            _dateText.text = date;
        }
    }
}
