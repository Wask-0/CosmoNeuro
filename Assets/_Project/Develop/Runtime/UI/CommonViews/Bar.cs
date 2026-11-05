using _Project.Develop.Runtime.UI.Core;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Develop.Runtime.UI.CommonViews
{
    public class Bar : MonoBehaviour, IView
    {
        [SerializeField] private Slider _slider;
        [SerializeField] private Image _filler;

        public void UpdateValue(float sliderValue) => _slider.value = sliderValue;
        public void SetFillerColor(Color color) => _filler.color = color;
    }
}