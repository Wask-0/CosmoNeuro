using TMPro;
using UnityEngine;

namespace _Project.Develop.Runtime.UI.Gameplay.Subtitles
{
    public sealed class SubtitleView : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private TMP_Text _text;

        public void Show(string text)
        {
            _text.text = text;
            _panel.SetActive(true);
        }

        public void Hide() => _panel.SetActive(false);
    }
}
