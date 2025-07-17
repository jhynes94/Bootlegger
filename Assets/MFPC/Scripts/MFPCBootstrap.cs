using UnityEngine;
using MFPC.Utils;
using MFPC.Input;
using MFPC.Input.PlayerInput;
using MFPC.Input.SettingsInput;

namespace MFPC
{
    public class MFPCBootstrap : MonoBehaviour
    {
        [SerializeField] private InputConfig inputConfigData;
        [SerializeField] private PlayerInputTuner playerInputTuner;
        [SerializeField] private SettingsInputTuner settingsInputTuner;
        [SerializeField] private Player player;
        [SerializeField] private Settings settings;

        private ReactiveProperty<InputType> _currentInputType;
        private AutoInputSelector _autoInputSelector;
        private SensitiveData _sensitiveData;

        private void Awake()
        {
            _currentInputType = new ReactiveProperty<InputType>(inputConfigData.GetCurrentInputType());
            _sensitiveData = inputConfigData.GetSensitiveDataCopy();

            playerInputTuner.Initialize(_currentInputType, _sensitiveData);
            player.Initialize(playerInputTuner, _sensitiveData);
            settings.Initialize(playerInputTuner, settingsInputTuner, _currentInputType, _sensitiveData);

#if ENABLE_INPUT_SYSTEM
            _autoInputSelector = new AutoInputSelector(_currentInputType, inputConfigData, settings);
            if (inputConfigData.DebugMode)
            {
                var debug = this.gameObject.AddComponent<MFPCDebug>();
                debug.Initialize(_currentInputType, _autoInputSelector, inputConfigData);
            }
#endif
        }

        
#if ENABLE_INPUT_SYSTEM
        private void OnDestroy()
        {
            _autoInputSelector.Dispose();
        }
#endif
    }
}