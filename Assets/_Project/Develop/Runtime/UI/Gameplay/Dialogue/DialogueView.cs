using TMPro;
using UnityEngine;

namespace _Project.Develop.Runtime.UI.Gameplay.Dialogue
{
    public sealed class DialogueView : MonoBehaviour
    {
        [SerializeField] private GameObject _container;
        [SerializeField] private TMP_Text _speaker;
        [SerializeField] private TMP_Text _text;
        public void Show(string speaker, string text)
        {
            _speaker.text = speaker;
            _speaker.gameObject.SetActive(!string.IsNullOrWhiteSpace(speaker));
            _text.text = text;
            _container.SetActive(true);
        }
        public void Hide() => _container.SetActive(false);
    }
}
