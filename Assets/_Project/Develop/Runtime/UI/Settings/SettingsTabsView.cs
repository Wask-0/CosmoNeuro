using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace _Project.Develop.Runtime.UI.Settings
{
    public class SettingsTabsView : MonoBehaviour
    {
        [SerializeField] private Tab[] _tabs;
        [SerializeField] private Selectable[] _footerControls;

        private UnityAction[] _handlers;
        private int _currentIndex = -1;

        public int CurrentIndex => _currentIndex;

        public event Action<int> TabChanged;

        private void OnEnable()
        {
            _handlers = new UnityAction[_tabs.Length];

            for (int i = 0; i < _tabs.Length; i++)
            {
                int index = i;

                _handlers[i] = () => ShowTab(index);

                _tabs[i].Button.onClick.AddListener(_handlers[i]);
            }

            if (_tabs.Length > 0)
                ShowTab(0);
        }

        private void OnDisable()
        {
            for (int i = 0; i < _tabs.Length; i++)
                _tabs[i].Button.onClick.RemoveListener(_handlers[i]);

            _currentIndex = -1;
        }

        public void ShowTab(int index)
        {
            if (index < 0 || index >= _tabs.Length)
                throw new ArgumentOutOfRangeException(nameof(index));

            if (_currentIndex == index)
                return;

            for (int i = 0; i < _tabs.Length; i++)
                _tabs[i].Panel.SetActive(i == index);

            _currentIndex = index;

            RefreshNavigation();

            TabChanged?.Invoke(index);
        }

        public void RefreshNavigation()
        {
            if (_currentIndex < 0)
                return;

            Tab current = _tabs[_currentIndex];

            List<Selectable> controls = GetAvailable(current.Controls);
            List<Selectable> footer = GetAvailable(_footerControls);

            Selectable firstFooter = footer.Count > 0 ? footer[0] : current.Button;

            Selectable firstControl = controls.Count > 0 ? controls[0] : firstFooter;

            Selectable lastControl = controls.Count > 0 ? controls[controls.Count - 1] : current.Button;

            for (int i = 0; i < _tabs.Length; i++)
            {
                Navigation navigation = _tabs[i].Button.navigation;

                navigation.mode = Navigation.Mode.Explicit;
                navigation.selectOnLeft = _tabs[(i + _tabs.Length - 1) % _tabs.Length].Button;
                navigation.selectOnRight = _tabs[(i + 1) % _tabs.Length].Button;
                navigation.selectOnUp = null;
                navigation.selectOnDown = firstControl;

                _tabs[i].Button.navigation = navigation;
            }

            for (int i = 0; i < controls.Count; i++)
            {
                Navigation navigation = controls[i].navigation;

                navigation.mode = Navigation.Mode.Explicit;
                navigation.selectOnUp = i > 0 ? controls[i - 1] : current.Button;
                navigation.selectOnDown = i < controls.Count - 1 ? controls[i + 1] : firstFooter;

                controls[i].navigation = navigation;
            }

            for (int i = 0; i < footer.Count; i++)
            {
                Navigation navigation = footer[i].navigation;

                navigation.mode = Navigation.Mode.Explicit;
                navigation.selectOnUp = lastControl;
                navigation.selectOnDown = null;
                navigation.selectOnLeft = i > 0 ? footer[i - 1] : null;
                navigation.selectOnRight = i < footer.Count - 1 ? footer[i + 1] : null;

                footer[i].navigation = navigation;
            }
        }

        private static List<Selectable> GetAvailable(
            Selectable[] items)
        {
            List<Selectable> result = new();

            if (items == null)
                return result;

            foreach (Selectable item in items)
            {
                if (item != null && item.isActiveAndEnabled && item.IsInteractable())
                    result.Add(item);
            }

            return result;
        }

        [Serializable]
        private class Tab
        {
            public Button Button;
            public GameObject Panel;
            public Selectable[] Controls;
        }
    }
}