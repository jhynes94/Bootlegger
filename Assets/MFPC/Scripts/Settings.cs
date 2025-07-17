using UnityEngine;
using MFPC.Input;
using MFPC.Input.SettingsInput;
using MFPC.Input.PlayerInput;
using MFPC.Utils;

namespace MFPC
{
    public class Settings : MonoBehaviour
    {
        public bool IsOpen => settingsField.activeSelf;

        [Utils.CenterHeader("Setups")] 
        [SerializeField] private SensivitySetup sensivitySetup;
        [SerializeField] private JoystickSetup joystickSetup;
        [SerializeField] private MobileUISetup mobileUISetup;
        [SerializeField] private InputTypeSetup inputTypeSetup;

        [Utils.CenterHeader("UI")] 
        [SerializeField] private GameObject settingsField;

        private PlayerInputTuner _playerInputTuner;
        private SettingsInputTuner _settingsInputTuner;

        public void Initialize(PlayerInputTuner playerInputTuner, SettingsInputTuner settingsInputTuner,
            ReactiveProperty<InputType> currentInputType, SensitiveData sensitiveData)
        {
            _playerInputTuner = playerInputTuner;
            _settingsInputTuner = settingsInputTuner;

            _settingsInputTuner.Initialize(currentInputType, settingsField);
            sensivitySetup.Initialize(sensitiveData);
            joystickSetup.Initialize();
            mobileUISetup.Initialize(settingsField);
            inputTypeSetup.Initialize(currentInputType);

            _settingsInputTuner.CurrentSettingsInput.OnOpenSettings += OnOpenSettings;
        }

        private void OnDestroy() => _settingsInputTuner.CurrentSettingsInput.OnOpenSettings -= OnOpenSettings;

        private void OnOpenSettings()
        {
            _playerInputTuner.IsLockInput = !settingsField.activeSelf;
            settingsField.SetActive(!settingsField.activeSelf);
        }
    }
}