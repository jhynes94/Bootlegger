using MFPC.Utils;
using UnityEngine;

namespace MFPC.Input
{
    public enum InputType
    {
        Mobile,
        KeyboardMouse,
        Gamepad
    }

    [CreateAssetMenu(fileName = "InputConfig", menuName = "MFPC/InputConfig", order = 0)]
    public class InputConfig : ScriptableObject
    {
#if ENABLE_INPUT_SYSTEM
        [field:CenterHeader("New Input Settings")]
        
        [Docs("Enables display of current input (Only in New Input).")]
        [field:SerializeField] 
        public bool DebugMode { get; private set; }
        
        [Docs("Automatic input switching (From keyboard to gamepad and vice versa) (Only in New Input).")]
        [field:SerializeField]
        public bool AutoInputSwitch { get; private set; } = true;
#endif
        
        [CenterHeader("Common Settings")]
        
        [Docs("Initial Sensitivity settings. On subsequent launches, the settings will be loaded from saves.")]
        [SerializeField] 
        public SensitiveData initialSensitiveData;
        
        [Docs("Initial InputType settings. On subsequent launches, the settings will be loaded from saves (If this option enabled).")]
        [SerializeField] 
        public InputType initialInputType;
        
        [Docs("Controls adjust depending on what device the game was running on.")]
        [SerializeField]
        public bool adaptationInput;

        public InputType GetCurrentInputType()
        {
            if (adaptationInput)
            {
                if (Application.isMobilePlatform) return InputType.Mobile;
                if (Application.isConsolePlatform) return InputType.Gamepad;
                return InputType.KeyboardMouse;
            }

            return initialInputType;
        }

        public SensitiveData GetSensitiveDataCopy()
        { 
            return initialSensitiveData.CopySensitiveData();   
        }
    }
}