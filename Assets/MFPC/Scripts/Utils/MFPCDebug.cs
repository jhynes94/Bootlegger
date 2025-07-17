using MFPC.Input;
using MFPC.Utils;
using UnityEngine;

namespace MFPC
{
    public class MFPCDebug : MonoBehaviour
    {
        private ReactiveProperty<InputType> _currentInputType;
        private AutoInputSelector _autoInputSelector;
        private InputConfig _inputConfig;

#if ENABLE_INPUT_SYSTEM
        private void OnGUI()
        {
            GUIStyle style = new GUIStyle();
            style.fontSize = 24;
            style.normal.textColor = Color.white;

            GUILayout.Space(70);

            GUILayout.Label("CurrentInputDevice: " + _autoInputSelector.CurrentInputDevice, style);
            GUILayout.Label("ChooseInputType: " + _currentInputType.Value, style);
            GUILayout.Label("AutoInputSwitch: " + _inputConfig.AutoInputSwitch, style);
        }
#endif

        public void Initialize(ReactiveProperty<InputType> currentInputType, AutoInputSelector autoInputSelector,
            InputConfig inputConfig)
        {
            _currentInputType = currentInputType;
            _autoInputSelector = autoInputSelector;
            _inputConfig = inputConfig;
        }
    }
}