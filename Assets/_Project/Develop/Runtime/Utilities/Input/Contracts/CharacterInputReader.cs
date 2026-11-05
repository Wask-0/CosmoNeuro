using System;
using _Project.Develop.Runtime.Infrastructure.DI;
using _Project.Develop.Runtime.Utilities.Input.Common;
using UnityEngine.InputSystem;

namespace _Project.Develop.Runtime.Utilities.Input.Contracts
{
    public class CharacterInputReader : ICharacterInput, IInitializable, IDisposable
    {
        private readonly ICommonInput _commonInput;
        private readonly InputAction _move;
        
        private readonly InputAction _placeTent;
        private readonly InputAction _packTent;
        private readonly InputAction _jump;
        private readonly InputAction _device;
        private readonly InputAction _bag;
        
        private bool _initialized;
        private bool _disposed;
        private int _interactConsumedFrame = -1;
        
        public CommonInputMode CurrentMode => _commonInput.CurrentMode;
        
        public float Move => _initialized && !_disposed && CurrentMode == CommonInputMode.Gameplay
            ? _move.ReadValue<float>() : 0f;
        
        public bool PlaceTentPressed =>
            _initialized &&
            !_disposed &&
            CurrentMode == CommonInputMode.Gameplay &&
            _interactConsumedFrame != UnityEngine.Time.frameCount &&
            _placeTent.WasPressedThisFrame();

        public bool TryConsumeInteract()
        {
            if (!PlaceTentPressed) return false;
            _interactConsumedFrame = UnityEngine.Time.frameCount;
            return true;
        }
        
        public bool PackTentPressed =>
            _initialized &&
            !_disposed &&
            CurrentMode == CommonInputMode.Gameplay &&
            _packTent.WasPressedThisFrame();

        public bool JumpPressed => IsGameplayReady && _jump.WasPressedThisFrame();

        public bool DeviceHeld => IsGameplayReady && _device.IsPressed();

        public bool BagPressed => IsGameplayReady && _bag.WasPressedThisFrame();

        private bool IsGameplayReady => _initialized && !_disposed && CurrentMode == CommonInputMode.Gameplay;

        public CharacterInputReader(ICommonInput commonInput)
        {
            _commonInput = commonInput ?? throw new ArgumentNullException(nameof(commonInput));
            _move = new InputAction("Move", InputActionType.Value, expectedControlType: "Axis");
            _move.AddCompositeBinding("1DAxis").With("Negative", "<Keyboard>/a").With("Positive", "<Keyboard>/d");
            _move.AddCompositeBinding("1DAxis").With("Negative", "<Keyboard>/leftArrow").With("Positive", "<Keyboard>/rightArrow");
            _move.AddBinding("<Gamepad>/leftStick/x").WithProcessor("axisDeadzone");
            _move.AddBinding("<Gamepad>/dpad/x");
            
            _placeTent = new InputAction(
                "PlaceTent",
                InputActionType.Button,
                "<Keyboard>/e");
            
            _packTent = new InputAction(
                "PackTent",
                InputActionType.Button,
                "<Keyboard>/q");

            _jump = new InputAction("Jump", InputActionType.Button, "<Keyboard>/space");
            _jump.AddBinding("<Keyboard>/w");
            _jump.AddBinding("<Keyboard>/upArrow");
            _jump.AddBinding("<Gamepad>/buttonSouth");

            _device = new InputAction("MagneticDevice", InputActionType.Button, "<Mouse>/leftButton");
            _device.AddBinding("<Gamepad>/rightTrigger");

            _bag = new InputAction("Bag", InputActionType.Button, "<Mouse>/rightButton");
            _bag.AddBinding("<Gamepad>/buttonWest");
        }

        public void Initialize()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(CharacterInputReader));
            if (_initialized) return;
            _initialized = true;
            _commonInput.ModeChanged += UpdateMode;
            UpdateMode();
        }

        private void UpdateMode()
        {
            if (CurrentMode == CommonInputMode.Gameplay)
            {
                _move.Enable();
                _placeTent.Enable();
                _packTent.Enable();
                _jump.Enable();
                _device.Enable();
                _bag.Enable();
            }
            else
            {
                _move.Disable();
                _placeTent.Disable();
                _packTent.Disable();
                _jump.Disable();
                _device.Disable();
                _bag.Disable();
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            if (_initialized) _commonInput.ModeChanged -= UpdateMode;
            _move.Disable();
            _move.Dispose();
            
            _placeTent.Disable();
            _placeTent.Dispose();
            
            _packTent.Disable();
            _packTent.Dispose();

            _jump.Disable();
            _jump.Dispose();

            _device.Disable();
            _device.Dispose();

            _bag.Disable();
            _bag.Dispose();
        }
    }
}
