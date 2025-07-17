using UnityEngine;
using UnityEngine.EventSystems;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;

namespace MFPC.Input.SettingsInput
{
    public class GamepadSettingsInputHandler : NewSettingsInputHandler
    {
        private GameObject _firstSelectedObject;

        public GamepadSettingsInputHandler(GameObject firstSelectedObject) : base()
        {
            _firstSelectedObject = firstSelectedObject;
        }

        public override void UpdateUIInput()
        {
            if (!_settingsField.activeSelf) EventSystem.current.SetSelectedGameObject(_firstSelectedObject);
            
            if(Gamepad.current == null) Cursor.lockState = CursorLockMode.None;
        }

        protected override void PerformedSettingsInput(InputAction.CallbackContext obj)
        {
            if (obj.control.device == Gamepad.current)
            {
                if (!IsEmptyAction) UpdateUIInput();
                
                CallOpenSettings();
            }
        }
    }
}
#endif