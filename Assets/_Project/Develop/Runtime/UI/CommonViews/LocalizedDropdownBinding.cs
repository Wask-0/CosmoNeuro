using System.Collections.Generic;
using System.Linq;
using _Project.Develop.Runtime.Utilities.Foundation.Text;
using TMPro;
using UnityEngine;

namespace _Project.Develop.Runtime.UI.CommonViews
{
    public sealed class LocalizedDropdownBinding : MonoBehaviour
    {
        private TMP_Dropdown _dropdown;
        private ITextLocalizer _localizer;
        private List<string> _sources;
        public void Initialize(TMP_Dropdown dropdown, ITextLocalizer localizer)
        {
            if (_localizer != null) _localizer.Changed -= Refresh;
            _dropdown = dropdown; _localizer = localizer;
            _sources = dropdown.options.Select(option => option.text).ToList();
            localizer.Changed += Refresh;
            Refresh();
        }
        public void SetOptions(List<string> sources, int selected)
        { _sources = new List<string>(sources); _dropdown.SetValueWithoutNotify(selected); Refresh(selected); }
        private void Refresh() => Refresh(_dropdown.value);
        private void Refresh(int selected)
        {
            if (_dropdown == null) return;
            _dropdown.ClearOptions();
            _dropdown.AddOptions(_sources.Select(source => _localizer.Get("UI", source)).ToList());
            _dropdown.SetValueWithoutNotify(selected);
            _dropdown.RefreshShownValue();
        }
        private void OnDestroy() { if (_localizer != null) _localizer.Changed -= Refresh; }
    }
}
