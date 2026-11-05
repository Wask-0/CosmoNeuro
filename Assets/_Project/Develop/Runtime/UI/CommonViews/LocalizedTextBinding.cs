using _Project.Develop.Runtime.Utilities.Foundation.Text;
using TMPro;
using UnityEngine;

namespace _Project.Develop.Runtime.UI.CommonViews
{
    public sealed class LocalizedTextBinding : MonoBehaviour
    {
        private TMP_Text _text;
        private ITextLocalizer _localizer;
        private string _source;
        private string _area;
        private string _prefix = "";
        public void Initialize(TMP_Text text, ITextLocalizer localizer)
        {
            Release();
            _text = text;
            _localizer = localizer;
            _source = text.text;
            _area = "UI";
            localizer.Changed += Refresh;
            Refresh();
        }
        public void SetSource(string source, string area)
        { _source = source; _area = area; Refresh(); }
        public void SetPrefix(string prefix) { _prefix = prefix; Refresh(); }
        private void Refresh() { if (_text != null) _text.text = _prefix + _localizer.Get(_area, _source); }
        private void Release() { if (_localizer != null) _localizer.Changed -= Refresh; }
        private void OnDestroy() => Release();
    }

    public static class LocalizedViewText
    {
        public static void SetSource(this TMP_Text text, string source, string area = "UI")
        {
            var binding = text.GetComponent<LocalizedTextBinding>();
            if (binding != null) binding.SetSource(source, area);
            else text.text = source;
        }
        public static void Bind(GameObject root, ITextLocalizer localizer)
        {
            foreach (var text in root.GetComponentsInChildren<TMP_Text>(true))
            {
                // Dropdown captions/items are controlled by the dropdown binding.
                if (text.GetComponentInParent<TMP_Dropdown>() != null) continue;
                var binding = text.GetComponent<LocalizedTextBinding>() ?? text.gameObject.AddComponent<LocalizedTextBinding>();
                binding.Initialize(text, localizer);
            }
            foreach (var dropdown in root.GetComponentsInChildren<TMP_Dropdown>(true))
            {
                var binding = dropdown.GetComponent<LocalizedDropdownBinding>() ?? dropdown.gameObject.AddComponent<LocalizedDropdownBinding>();
                binding.Initialize(dropdown, localizer);
            }
        }
    }
}
