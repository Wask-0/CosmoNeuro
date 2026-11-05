using System;
using UnityEngine;
using System.Collections.Generic;
using _Project.Develop.Runtime.UI.Confirm;
using _Project.Develop.Runtime.UI.Settings;

namespace _Project.Develop.Runtime.UI.Core.Popup
{
    public abstract class PopupService : IDisposable
    {
        protected readonly ViewsFactory ViewsFactory;

        private readonly ProjectPresentersFactory _presentersFactory;

        private readonly Dictionary<PopupPresenterBase, PopupInfo> _presenterToInfo = new();

        protected PopupService(
            ViewsFactory viewsFactory, 
            ProjectPresentersFactory presentersFactory)
        {
            ViewsFactory = viewsFactory;
            _presentersFactory = presentersFactory;
        }

        protected abstract Transform PopupLayer { get; }
        
        public ConfirmPopupPresenter OpenConfirmation(
            string titleKey, string messageKey,
            Action confirmed, Action closed = null)
        {
            ConfirmPopupView view = ViewsFactory.Create<ConfirmPopupView>(ViewIDs.ConfirmPopup, PopupLayer);

            ConfirmPopupPresenter presenter =
                _presentersFactory.CreateConfirmPopupPresenter(view, titleKey, messageKey, confirmed);

            OnPopupCreated(presenter, view, closed);

            return presenter;
        }
        
        public ConfirmPopupPresenter OpenConfirmation(
            string titleKey, string messageKey, string confirmKey, string cancelKey,
            Action confirmed, Action closed = null)
        {
            ConfirmPopupView view = ViewsFactory.Create<ConfirmPopupView>(ViewIDs.ConfirmPopup, PopupLayer);

            ConfirmPopupPresenter presenter =
                _presentersFactory.CreateConfirmPopupPresenter(
                    view, titleKey, messageKey, confirmKey, cancelKey, confirmed);

            OnPopupCreated(presenter, view, closed);

            return presenter;
        }
        
        public SettingsPopupPresenter OpenSettings(Action closed = null)
        {
            SettingsPopupView view = ViewsFactory.Create<SettingsPopupView>(ViewIDs.SettingsPopup, PopupLayer);

            SettingsPopupPresenter presenter = _presentersFactory.CreateSettingsPopupPresenter(view);

            OnPopupCreated(presenter, view, closed);

            return presenter;
        }

        public void ClosePopup(PopupPresenterBase popup)
        {
            popup.CloseRequest -= ClosePopup;

            popup.Hide(() =>
            {
                _presenterToInfo[popup].ClosedCallback?.Invoke();

                DisposeFor(popup);
                _presenterToInfo.Remove(popup);
            });
        }

        public void Dispose()
        {
            foreach (PopupPresenterBase popup in _presenterToInfo.Keys)
            {
                popup.CloseRequest -= ClosePopup;
                DisposeFor(popup);
            }

            _presenterToInfo.Clear();
        }

        protected void OnPopupCreated(
            PopupPresenterBase popup,
            PopupViewBase view,
            Action closedCallback = null)
        {
            PopupInfo popupInfo = new PopupInfo(view, closedCallback);

            _presenterToInfo.Add(popup, popupInfo);
            popup.Initialize();
            popup.Show();

            popup.CloseRequest += ClosePopup;
        }

        private void DisposeFor(PopupPresenterBase popup)
        {
            popup.Dispose();
            ViewsFactory.Release(_presenterToInfo[popup].View);
        }

        private class PopupInfo
        {
            public PopupInfo(PopupViewBase view, Action closedCallback)
            {
                View = view;
                ClosedCallback = closedCallback;
            }

            public PopupViewBase View { get; }
            public Action ClosedCallback { get; }
        }
    }
}
