using System;
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;

namespace MFPC.Input.SettingsInput
{
    public class NewSettingsInputHandler : ISettingsInput, IDisposable
    {
        public event Action OnOpenSettings;

        protected bool IsEmptyAction => OnOpenSettings == null;
        protected GameObject _settingsField;
        private SettingsInputActions _settingsInputActions;

        public NewSettingsInputHandler()
        {
            _settingsInputActions = new SettingsInputActions();
            _settingsInputActions.Enable();
            _settingsInputActions.Settings.Open.performed += PerformedSettingsInput;
        }
        
        public void Dispose()
        {
            _settingsInputActions.Settings.Open.performed -= PerformedSettingsInput;
            _settingsInputActions.Dispose();
        }

        public void SetSettingsField(GameObject settingsField)
        {
            _settingsField = settingsField;
        }

        public virtual void UpdateUIInput()
        {
            Cursor.lockState = (_settingsField.activeSelf)
                ? CursorLockMode.None
                : CursorLockMode.Locked;
        }

        protected virtual void PerformedSettingsInput(InputAction.CallbackContext obj)
        {
            if (obj.control.device == Keyboard.current)
            {
                CallOpenSettings();
                
                if (!IsEmptyAction) UpdateUIInput();
            }
        }

        protected void CallOpenSettings()
        {
            OnOpenSettings?.Invoke();
        }
    }
}

#endif