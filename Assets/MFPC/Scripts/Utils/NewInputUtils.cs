#if ENABLE_INPUT_SYSTEM
using System;
using UnityEngine.InputSystem;

namespace MFPC.Utils
{
    public static class NewInputUtils
    {
        public static void SubscribeStartEndAction(this InputAction inputAction, 
            Action<InputAction.CallbackContext> action)
        {
            inputAction.performed += action;
            inputAction.canceled += action;
        }
        
        public static void SubscribeStartAction(this InputAction inputAction, 
            Action<InputAction.CallbackContext> action)
        {
            inputAction.performed += action;
        }
        
        public static void UnsubscribeStartEndAction(this InputAction inputAction, 
            Action<InputAction.CallbackContext> action)
        {
            inputAction.performed -= action;
            inputAction.canceled -= action;
        }
        
        public static void UnsubscribeStartAction(this InputAction inputAction, 
            Action<InputAction.CallbackContext> action)
        {
            inputAction.performed -= action;
        }
    }
}
#endif