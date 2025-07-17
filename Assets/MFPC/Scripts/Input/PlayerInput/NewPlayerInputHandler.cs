using MFPC.Utils;
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem;
#endif

namespace MFPC.Input.PlayerInput
{
    public class NewPlayerInputHandler : PlayerInputHandler
    {
#if ENABLE_INPUT_SYSTEM
        private PlayerInputActions _playerInputActions;
        private ReactiveProperty<InputType> _inputType;

        private bool IsGamepad => _inputType.Value == InputType.Gamepad;
        protected override float DeltaTimeMultiplier => !IsGamepad ? 1.0f : Time.deltaTime;
        
        private void Awake()
        {
            _playerInputActions = new PlayerInputActions();
            _playerInputActions.Enable();
            
            _playerInputActions.Player.Move.SubscribeStartEndAction(OnMove);
            _playerInputActions.Player.Look.SubscribeStartEndAction(OnLook);
            _playerInputActions.Player.Sprint.SubscribeStartEndAction(OnSprint);
            _playerInputActions.Player.Lean.SubscribeStartEndAction(OnLean);
            _playerInputActions.Player.Jump.SubscribeStartAction(OnJump);
            _playerInputActions.Player.Sit.SubscribeStartAction(OnSit);
        }

        private void OnDisable()
        {
            _playerInputActions.Player.Move.UnsubscribeStartEndAction(OnMove);
            _playerInputActions.Player.Look.UnsubscribeStartEndAction(OnLook);
            _playerInputActions.Player.Sprint.UnsubscribeStartEndAction(OnSprint);
            _playerInputActions.Player.Lean.UnsubscribeStartEndAction(OnLean);
            _playerInputActions.Player.Jump.UnsubscribeStartAction(OnJump);
            _playerInputActions.Player.Sit.UnsubscribeStartAction(OnSit);
        }
        
        public void Initialize(ReactiveProperty<InputType> inputType)
        {
            _inputType = inputType;
        }

        #region Input

        private void OnMove(InputAction.CallbackContext context)
        {
            if (IsRequireDevice(context)) SetMoveInput(context.ReadValue<Vector2>());
        }

        private void OnLook(InputAction.CallbackContext context)
        {
            if (IsRequireDevice(context)) SetLookInput(context.ReadValue<Vector2>());
        }

        private void OnJump(InputAction.CallbackContext context)
        {
            if (IsRequireDevice(context)) SetJumpInput();
        }

        private void OnSit(InputAction.CallbackContext context)
        {
            if (IsRequireDevice(context)) SetSitInput();
        }

        private void OnSprint(InputAction.CallbackContext context)
        {
            if (IsRequireDevice(context)) SetSprintInput(context.performed);
        }

        private void OnLean(InputAction.CallbackContext context)
        {
            if (IsRequireDevice(context)) SetLeanDirection(context.ReadValue<float>());
        }

        #endregion
        
        private bool IsRequireDevice(InputAction.CallbackContext context)
        {
            return (context.control.device == Keyboard.current 
                   || context.control.device == Mouse.current) 
                   == (_inputType.Value == InputType.KeyboardMouse);
        }
#endif
    }
}