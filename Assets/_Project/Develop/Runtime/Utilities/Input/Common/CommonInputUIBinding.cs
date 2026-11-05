using System;
using System.Collections.Generic;
using _Project.Develop.Runtime.Utilities.Input.Unity;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

namespace _Project.Develop.Runtime.Utilities.Input.Common
{
    [RequireComponent(typeof(InputSystemUIInputModule))]
    public class CommonInputUIBinding : MonoBehaviour
    {
        private readonly List<InputActionReference> _references = new();

        private CommonInputReader _reader;
        private InputSystemUIInputModule _module;

        public void Initialize(CommonInputReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            Release();

            _reader = reader;
            _module = GetComponent<InputSystemUIInputModule>();

            _module.enabled = false;

            CommonInput actions = reader.Actions;

            _module.actionsAsset = actions.asset;
            _module.point = Reference(actions.UI.Point);
            _module.leftClick = Reference(actions.UI.Click);
            _module.scrollWheel = Reference(actions.UI.ScrollWheel);
            _module.move = Reference(actions.UI.Navigate);
            _module.submit = Reference(actions.UI.Submit);
            _module.cancel = Reference(actions.UI.Cancel);

            _module.rightClick = null;
            _module.middleClick = null;
            _module.trackedDevicePosition = null;
            _module.trackedDeviceOrientation = null;

            _reader.ModeChanged += Refresh;
            Refresh();
        }

        private InputActionReference Reference(InputAction action)
        {
            InputActionReference reference =
                InputActionReference.Create(action);

            _references.Add(reference);

            return reference;
        }

        private void Refresh()
        {
            if (_module != null)
            {
                _module.enabled =
                    _reader.CurrentMode == CommonInputMode.UI;
            }
        }

        private void OnDestroy()
        {
            Release();
        }

        private void Release()
        {
            if (_reader == null)
                return;

            _reader.ModeChanged -= Refresh;

            if (_module != null)
            {
                _module.enabled = false;

                _module.point = null;
                _module.leftClick = null;
                _module.scrollWheel = null;
                _module.move = null;
                _module.submit = null;
                _module.cancel = null;
                _module.actionsAsset = null;
            }

            foreach (InputActionReference reference in _references)
            {
                if (reference != null)
                    Destroy(reference);
            }

            _references.Clear();

            _reader = null;
            _module = null;
        }
    }
}