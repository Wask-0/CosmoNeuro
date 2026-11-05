using System;
using _Project.Develop.Runtime.UI.Core.Popup;
using _Project.Develop.Runtime.Utilities.CoroutinesManagement;
using _Project.Develop.Runtime.Utilities.Input.Common;

namespace _Project.Develop.Runtime.UI.Confirm
{
    public class ConfirmPopupPresenter : PopupPresenterBase
    {
        private readonly ConfirmPopupView _view;

        private Action _onConfirmed;
        
        private readonly ICommonInput _input;
        private bool _decisionMade;
        
        public ConfirmPopupPresenter(
            ICoroutinesPerformer coroutinesPerformer,
            ConfirmPopupView view, ICommonInput input) : base(coroutinesPerformer)
        {
            _view = view;
            _input = input;
        }

        protected override PopupViewBase PopupView => _view;
        
        public UnityEngine.GameObject FirstSelected => _view.FirstSelected;

        public override void Initialize()
        {
            base.Initialize();
            
            _view.Confirmed += OnConfirmed;
            _view.Cancelled += OnCancelled;
            _input.CancelPressed += OnCancelled;
        }

        public override void Dispose()
        {
            _view.Confirmed -= OnConfirmed;
            _view.Cancelled -= OnCancelled;
            _input.CancelPressed -= OnCancelled;

            base.Dispose();
        }
        
        public void Configure(
            string title, string message,
            string confirmText, string cancelText, Action confirmAction)
        {
            _view.SetTitleText(title);
            _view.SetMessage(message);
            
            _view.SetConfirmText(confirmText);
            _view.SetCancelText(cancelText);

            _onConfirmed = confirmAction;
        }

        private void OnCancelled()
        {
            if (_decisionMade)
                return;

            _decisionMade = true;

            OnCloseRequest();
        }

        private void OnConfirmed()
        {
            if (_decisionMade)
                return;

            _decisionMade = true;

            _onConfirmed?.Invoke();

            OnCloseRequest();
        }
    }
}