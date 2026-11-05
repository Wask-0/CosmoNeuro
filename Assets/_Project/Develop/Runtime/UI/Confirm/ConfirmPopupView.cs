using System;
using _Project.Develop.Runtime.UI.Core.Popup;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Develop.Runtime.UI.Confirm
{
    public class ConfirmPopupView : PopupViewBase
    {
        public event Action Confirmed;
        public event Action Cancelled;
        
        [SerializeField] private TMP_Text _title;
        [SerializeField] private TMP_Text _message;
        
        [SerializeField] private TMP_Text _confirmText;
        [SerializeField] private TMP_Text _cancelText;
        
        [SerializeField] private Button _confirmButton;
        [SerializeField] private Button _cancelButton;
        
        public GameObject FirstSelected => _cancelButton.gameObject;

        public void SetTitleText(string text) => _title.text = text;
        public void SetMessage(string message) => _message.text = message;
        public void SetConfirmText(string text) => _confirmText.text = text;
        public void SetCancelText(string text) => _cancelText.text = text;
        
        private void OnEnable()
        {
            _confirmButton.onClick.AddListener(OnConfirmButtonClicked);
            _cancelButton.onClick.AddListener(OnCancelButtonClicked);
        }

        private void OnDisable()
        {
            _confirmButton.onClick.RemoveListener(OnConfirmButtonClicked);
            _cancelButton.onClick.RemoveListener(OnCancelButtonClicked);
        }
        
        private void OnConfirmButtonClicked() => Confirmed?.Invoke();

        private void OnCancelButtonClicked() => Cancelled?.Invoke();
    }
}
