using System;
using _Project.Develop.Runtime.Infrastructure.DI;
using _Project.Develop.Runtime.UI.Confirm;
using _Project.Develop.Runtime.UI.Core.Popup.TestPopup;
using _Project.Develop.Runtime.UI.Settings;
using _Project.Develop.Runtime.Utilities.CoroutinesManagement;
using _Project.Develop.Runtime.Utilities.DataManagement.DataProviders;
using _Project.Develop.Runtime.Utilities.Input.Common;
using _Project.Develop.Runtime.Utilities.Settings;
using UnityEngine.Localization.Settings;

namespace _Project.Develop.Runtime.UI
{
    public class ProjectPresentersFactory
    {
        private readonly DIContainer _container;

        public ProjectPresentersFactory(DIContainer container)
        {
            _container = container;
        }

        public TestPopupPresenter CreateTestPopupPresenter(TestPopupView view)
        {
            return new TestPopupPresenter(
                view, _container.Resolve<ICoroutinesPerformer>());
        }
        
        public ConfirmPopupPresenter CreateConfirmPopupPresenter(
            ConfirmPopupView view, string titleKey, string messageKey, Action confirmed)
        {
            return CreateConfirmPopupPresenter(view, titleKey, messageKey,
                "Common.Confirm", "Common.Cancel", confirmed);
        }
        
        public ConfirmPopupPresenter CreateConfirmPopupPresenter(ConfirmPopupView view,
            string titleKey, string messageKey, string confirmKey, string cancelKey, Action confirmed)
        {
            string title = GetUIText(titleKey);
            string message = GetUIText(messageKey);
            string confirmText = GetUIText(confirmKey);
            string cancelText = GetUIText(cancelKey);

            ConfirmPopupPresenter presenter = new(
                _container.Resolve<ICoroutinesPerformer>(), view, _container.Resolve<ICommonInput>());

            presenter.Configure(title, message, confirmText, cancelText, confirmed);

            return presenter;
        }
        
        public SettingsPopupPresenter CreateSettingsPopupPresenter(SettingsPopupView view)
        {
            return new SettingsPopupPresenter(
                _container.Resolve<ICoroutinesPerformer>(),
                view,
                _container.Resolve<SettingsService>(),
                _container.Resolve<SettingsDataProvider>(),
                _container.Resolve<ICommonInput>());
        }
        
        private static string GetUIText(string key)
        {
            return LocalizationSettings.StringDatabase
                .GetLocalizedString("UI", key);
        }
    }
}
