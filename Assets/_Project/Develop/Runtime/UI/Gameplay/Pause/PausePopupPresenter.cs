using System;
using _Project.Develop.Runtime.UI.Core.Popup;
using _Project.Develop.Runtime.Utilities.CoroutinesManagement;
using _Project.Develop.Runtime.Utilities.Input.Common;
using UnityEngine;

namespace _Project.Develop.Runtime.UI.Gameplay.Pause
{
    public class PausePopupPresenter : PopupPresenterBase
    {
        private readonly PausePopupView _view;
        private readonly ICommonInput _input;

        private bool _blocked;
        private bool _closing;

        public event Action SettingsRequested;
        public event Action MainMenuRequested;

        public PausePopupPresenter(ICoroutinesPerformer coroutines, PausePopupView view,
            ICommonInput input) : base(coroutines)
        {
            _view = view;
            _input = input;
        }

        protected override PopupViewBase PopupView => _view;

        public GameObject FirstSelected => _view.FirstSelected;
        public GameObject SettingsSelected => _view.SettingsSelected;

        public override void Initialize()
        {
            base.Initialize();

            _view.ResumeClicked += OnResumeClicked;
            _view.SettingsClicked += OnSettingsClicked;
            _view.MainMenuClicked += OnMainMenuClicked;

            _input.CancelPressed += OnResumeClicked;
        }

        public override void Dispose()
        {
            _view.ResumeClicked -= OnResumeClicked;
            _view.SettingsClicked -= OnSettingsClicked;
            _view.MainMenuClicked -= OnMainMenuClicked;

            _input.CancelPressed -= OnResumeClicked;

            base.Dispose();
        }

        public void SetInteractable(bool value)
        {
            _blocked = !value;
            _view.SetInteractable(value);
        }

        private void OnResumeClicked()
        {
            if (_blocked || _closing)
                return;

            _closing = true;
            SetInteractable(false);

            OnCloseRequest();
        }

        private void OnSettingsClicked()
        {
            if (_blocked || _closing)
                return;

            SettingsRequested?.Invoke();
        }

        private void OnMainMenuClicked()
        {
            if (_blocked || _closing)
                return;

            MainMenuRequested?.Invoke();
        }
    }
}