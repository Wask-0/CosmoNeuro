using System;
using _Project.Develop.Runtime.Infrastructure.DI;
using _Project.Develop.Runtime.Utilities.Input.Unity;
using UnityEngine.InputSystem;

namespace _Project.Develop.Runtime.Utilities.Input.Common
{
    public class CommonInputReader : ICommonInput, IInitializable, IDisposable
    {
        private readonly CommonInput _actions = new();

        private bool _initialized;
        private bool _disposed;

        public CommonInputMode CurrentMode { get; private set; }

        public event Action ModeChanged;
        public event Action PausePressed;
        public event Action CancelPressed;

        internal CommonInput Actions => _actions;

        public void Initialize()
        {
            ThrowIfDisposed();

            if (_initialized)
                return;

            _actions.Disable();

            _actions.Global.Pause.performed += OnPause;
            _actions.UI.Cancel.performed += OnCancel;

            _initialized = true;
        }

        public void SetMode(CommonInputMode mode)
        {
            ThrowIfDisposed();

            if (!_initialized)
                throw new InvalidOperationException("Common input must be initialized before use.");

            if (!Enum.IsDefined(typeof(CommonInputMode), mode))
                throw new ArgumentOutOfRangeException(nameof(mode));

            if (CurrentMode == mode)
                return;

            _actions.Disable();

            CurrentMode = mode;

            switch (mode)
            {
                case CommonInputMode.UI:
                    _actions.UI.Enable();
                    break;

                case CommonInputMode.Gameplay:
                    _actions.Global.Enable();
                    break;
            }

            ModeChanged?.Invoke();
        }

        private void OnPause(InputAction.CallbackContext context)
        {
            if (CurrentMode == CommonInputMode.Gameplay)
                PausePressed?.Invoke();
        }

        private void OnCancel(InputAction.CallbackContext context)
        {
            if (CurrentMode == CommonInputMode.UI)
                CancelPressed?.Invoke();
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            if (_initialized)
            {
                _actions.Global.Pause.performed -= OnPause;
                _actions.UI.Cancel.performed -= OnCancel;
            }

            _actions.Disable();
            _actions.Dispose();

            CurrentMode = CommonInputMode.Disabled;

            ModeChanged = null;
            PausePressed = null;
            CancelPressed = null;
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(CommonInputReader));
        }
    }
}