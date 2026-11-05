using _Project.Develop.Runtime.UI.Core;
using TMPro;
using UnityEngine;

namespace _Project.Develop.Runtime.UI.Gameplay.Resources
{
    public sealed class ResourceGaugeView : MonoBehaviour, IView
    {
        [SerializeField] private LiquidGaugeGraphic _liquid;
        [SerializeField] private TMP_Text _label;

        public void SetValue(float normalized) => _liquid.SetFill(normalized);
        public void SetLabel(string text) => _label.text = text;
        public void Slosh(float amount) => _liquid.Slosh(amount);
    }
}
