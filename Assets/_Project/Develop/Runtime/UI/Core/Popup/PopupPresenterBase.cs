using System;
using System.Collections;
using _Project.Develop.Runtime.Utilities.CoroutinesManagement;
using DG.Tweening;
using UnityEngine;

namespace _Project.Develop.Runtime.UI.Core.Popup
{
    public abstract class PopupPresenterBase : IPresenter
    {
        public event Action<PopupPresenterBase> CloseRequest;

        private readonly ICoroutinesPerformer _coroutinesPerformer;

        private Coroutine _process;

        protected PopupPresenterBase(ICoroutinesPerformer coroutinesPerformer)
        {
            _coroutinesPerformer = coroutinesPerformer;
        }

        protected abstract PopupViewBase PopupView { get; }

        public virtual void Initialize()
        {
        }

        public virtual void Dispose()
        {
            KillProcess();

            PopupView.CloseRequest -= OnCloseRequest;
        }

        public void Show()
        {
            KillProcess();

            _process = _coroutinesPerformer.StartPerform(ProcessShow());
        }

        public void Hide(Action callback = null)
        {
            KillProcess();

            _process = _coroutinesPerformer.StartPerform(ProcessHide(callback));
        }

        protected virtual void OnPostShow() { }

        protected virtual void OnPreShow()
        {
            PopupView.CloseRequest += OnCloseRequest;
        }

        protected virtual void OnPostHide() { }

        protected virtual void OnPreHide()
        {
            PopupView.CloseRequest -= OnCloseRequest;
        }

        protected void OnCloseRequest() => CloseRequest?.Invoke(this);

        private IEnumerator ProcessShow()
        {
            OnPreShow();

            yield return PopupView.Show().WaitForCompletion();

            OnPostShow();
        }

        private IEnumerator ProcessHide(Action callback)
        {
            OnPreHide();

            yield return PopupView.Hide().WaitForCompletion();

            OnPostHide();

            callback?.Invoke();
        }

        private void KillProcess()
        {
            if (_process != null)
                _coroutinesPerformer.StopPerform(_process);
        }
    }
}