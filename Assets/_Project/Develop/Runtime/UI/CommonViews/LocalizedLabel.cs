using TMPro;
using UnityEngine;
using UnityEngine.Localization;

namespace _Project.Develop.Runtime.UI.CommonViews
{
    [RequireComponent(typeof(TMP_Text))]
    public class LocalizedLabel : MonoBehaviour
    {
        [SerializeField] private LocalizedString _text;

        private TMP_Text _label;

        private void Awake()
        {
            _label = GetComponent<TMP_Text>();
        }

        private void OnEnable()
        {
            _text.StringChanged += OnStringChanged;
        }

        private void OnDisable()
        {
            _text.StringChanged -= OnStringChanged;
        }

        private void OnStringChanged(string value)
        {
            _label.text = value;
        }
    }
}