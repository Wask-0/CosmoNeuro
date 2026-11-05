using _Project.Develop.Runtime.Utilities.Foundation.Text;
using UnityEngine.ResourceManagement.AsyncOperations;
using _Project.Develop.Runtime.Infrastructure.DI;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.Localization;
using System;

namespace _Project.Develop.Runtime.Utilities.Localization
{
    public class GameTextLocalizer : ITextLocalizer, IInitializable, IDisposable
    {
        private StringTable _table;
        private int _request;
        private bool _disposed;
        public event Action Changed;

        public void Initialize()
        {
            LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
            LoadTable();
        }

        private void OnLocaleChanged(Locale locale) => LoadTable();

        private void LoadTable()
        {
            int request = ++_request;
            _table = null;
            var operation = LocalizationSettings.StringDatabase.GetTableAsync("Game");
            operation.Completed += completed =>
            {
                if (_disposed || request != _request) return;
                if (completed.Status == AsyncOperationStatus.Succeeded) _table = completed.Result;
                Changed?.Invoke();
            };
        }

        public string Get(string area, string source)
            => string.IsNullOrEmpty(source) ? source : _table?.GetEntry(area + "." + source)?.Value ?? source;

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
            _table = null;
            Changed = null;
        }
    }
}
