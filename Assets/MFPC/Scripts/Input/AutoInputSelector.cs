using System;
using MFPC.Utils;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
#endif

namespace MFPC.Input
{
    public class AutoInputSelector
    {
#if ENABLE_INPUT_SYSTEM
        public InputDevice CurrentInputDevice { get; private set; }

        private InputConfig _inputConfig;
        private ReactiveProperty<InputType> _inputType;
        private Settings _settings;
        
        public AutoInputSelector(ReactiveProperty<InputType> inputType, InputConfig inputConfig, Settings settings)
        {
            _inputType = inputType;
            _inputConfig = inputConfig;
            _settings = settings;
            
            InputSystem.onEvent += OnInputEvent;
        }
        
        public void Dispose()
        {
            InputSystem.onEvent -= OnInputEvent;
        }

        private void OnInputEvent(InputEventPtr eventPtr, InputDevice inputDevice)
        {
            InputType inputType = _inputConfig.GetCurrentInputType();
            CurrentInputDevice = inputDevice;

            if (_inputConfig.AutoInputSwitch && !_settings.IsOpen && _inputType.Value != InputType.Mobile)
            {
                if(inputDevice == Gamepad.current) inputType = InputType.Gamepad;
                if(inputDevice == Keyboard.current || inputDevice == Mouse.current) inputType = InputType.KeyboardMouse;
                
                if (_inputType.Value != inputType) _inputType.Value = inputType;
            }
        }
#endif
    }
}