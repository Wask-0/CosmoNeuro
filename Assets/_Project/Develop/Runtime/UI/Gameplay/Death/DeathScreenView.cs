using System;
using _Project.Develop.Runtime.Gameplay.GameTime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Develop.Runtime.UI.Gameplay.Death
{
    public sealed class DeathScreenView : MonoBehaviour
    {
        [SerializeField] private GameObject _container;
        [SerializeField] private GameObject _popup;
        [SerializeField] private TMP_Text _title;
        [SerializeField] private TMP_Text _message;
        [SerializeField] private TMP_Text _warning;
        [SerializeField] private Image _image;
        [SerializeField] private DeathIllustrationGraphic _fallback;
        [SerializeField] private Button _restart;
        [SerializeField] private Button _menu;
        public event Action RestartClicked;
        public event Action MenuClicked;
        public GameObject FirstSelected => _restart.gameObject;

        private void OnEnable() { _restart.onClick.AddListener(Restart); _menu.onClick.AddListener(Menu); }
        private void OnDisable() { _restart.onClick.RemoveListener(Restart); _menu.onClick.RemoveListener(Menu); }
        private void Restart() => RestartClicked?.Invoke();
        private void Menu() => MenuClicked?.Invoke();
        public void Hide() { _container.SetActive(false); _warning.text = ""; }
        public void SetWarning(string text) => _warning.text = text;
        public void ShowDeath(DangerCause cause, Sprite illustration)
        {
            _warning.text = "";
            (_title.text, _message.text) = cause switch
            {
                DangerCause.Cold => ("Вы замёрзли", "Скафандр не выдержал длительного воздействия холода."),
                DangerCause.Heat => ("Критический перегрев", "Температура превысила возможности защиты скафандра."),
                _ => ("Кислород закончился", "До линии видимости Земли дойти не удалось.")
            };
            _image.sprite = illustration;
            _image.gameObject.SetActive(illustration != null);
            _fallback.SetCause(cause);
            _fallback.gameObject.SetActive(illustration == null);
            _container.SetActive(true);
            _popup.SetActive(false);
            SetButtonsInteractable(true);
        }
        public void ShowChoices() => _popup.SetActive(true);
        public void SetButtonsInteractable(bool enabled)
        { _restart.interactable = enabled; _menu.interactable = enabled; }
    }
}
