using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace _Project.Develop.Editor.Interaction
{
    internal sealed class InteractionComponentDropdown : AdvancedDropdown
    {
        private readonly Type[] _types;
        private readonly Action<Type> _selected;
        private readonly Dictionary<int, Type> _entries = new();

        public InteractionComponentDropdown(Type contract, Action<Type> selected) : base(new AdvancedDropdownState())
        { _types = InteractionAuthoring.GetTypes(contract); _selected = selected; minimumSize = new Vector2(360, 320); }

        protected override AdvancedDropdownItem BuildRoot()
        {
            var root = new AdvancedDropdownItem("Компоненты взаимодействия");
            var categories = new Dictionary<string, AdvancedDropdownItem>();
            foreach (Type type in _types)
            {
                string category = type.Namespace?.Contains("Integrations") == true
                    ? type.Namespace.Substring(type.Namespace.LastIndexOf('.') + 1) : "Interaction";
                if (!categories.TryGetValue(category, out var parent))
                { parent = new AdvancedDropdownItem(category); categories.Add(category, parent); root.AddChild(parent); }
                var item = new AdvancedDropdownItem(ObjectNames.NicifyVariableName(type.Name)) { id = _entries.Count + 1 };
                parent.AddChild(item); _entries[item.id] = type;
            }
            if (_types.Length == 0) root.AddChild(new AdvancedDropdownItem("Нет доступных компонентов") { enabled = false });
            return root;
        }

        protected override void ItemSelected(AdvancedDropdownItem item)
        { if (_entries.TryGetValue(item.id, out Type type)) _selected(type); }
    }
}
