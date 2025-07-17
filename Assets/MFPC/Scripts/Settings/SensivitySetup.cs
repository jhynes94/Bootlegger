using UnityEngine;
using UnityEngine.UI;
using MFPC.Input;
using MFPC.Input.PlayerInput;
using MFPC.Utils.SaveLoad;

namespace MFPC
{
    [System.Serializable]
    public class SensivitySetup
    {
        [SerializeField] private Slider sensivitySlider;
        [SerializeField] private Slider cameraSpeedSmoothHorizontalSlider;
        [SerializeField] private Slider cameraSpeedSmoothVerticalSlider;

        private SensitiveData _sensitiveData;
        private PlayerInputTuner _playerInputTuner;
        private ISaver _saver;

        public void Initialize(SensitiveData sensitiveData)
        {
            _sensitiveData = sensitiveData;
            _saver = new PlayerPrefsSaver("SensivitySetup");

            sensivitySlider.maxValue = SensitiveData.MaxSensitivity;
            cameraSpeedSmoothHorizontalSlider.maxValue = SensitiveData.MaxRotateSpeedSmooth;
            cameraSpeedSmoothVerticalSlider.maxValue = SensitiveData.MaxRotateSpeedSmooth;

            sensivitySlider.value = _sensitiveData.Sensitivity;
            cameraSpeedSmoothHorizontalSlider.value = _sensitiveData.RotateSpeedSmoothHorizontal;
            cameraSpeedSmoothVerticalSlider.value = _sensitiveData.RotateSpeedSmoothVertical;

            Load();
            
            sensivitySlider.onValueChanged.AddListener(ChangeSensivity);
            cameraSpeedSmoothHorizontalSlider.onValueChanged.AddListener(ChangeCameraSpeedSmoothHorizontal);
            cameraSpeedSmoothVerticalSlider.onValueChanged.AddListener(ChangeCameraSpeedSmoothVertical);
        }

        ~SensivitySetup()
        {
            cameraSpeedSmoothHorizontalSlider.onValueChanged.RemoveAllListeners();
            cameraSpeedSmoothVerticalSlider.onValueChanged.RemoveAllListeners();
        }

        private void ChangeSensivity(float value)
        {
            _sensitiveData.SetSensitivity(value);
            Save();
        }

        private void ChangeCameraSpeedSmoothHorizontal(float value)
        {
            _sensitiveData.SetRotateSpeedSmoothHorizontal(value);
            Save();
        }

        private void ChangeCameraSpeedSmoothVertical(float value)
        {
            _sensitiveData.SetRotateSpeedSmoothVertical(value);
            Save();
        }

        #region Save&Load
        
        private void Save()
        {
            _saver.Save("sensivitySlider", sensivitySlider.value)
                .Save("cameraSpeedSmoothHorizontalSlider", cameraSpeedSmoothHorizontalSlider.value)
                .Save("cameraSpeedSmoothVerticalSlider", cameraSpeedSmoothVerticalSlider.value);
        }

        private void Load()
        {
            _saver.Load<float>("sensivitySlider", _ => { sensivitySlider.value = _; })
                .Load<float>("cameraSpeedSmoothHorizontalSlider", _ => { cameraSpeedSmoothHorizontalSlider.value = _; })
                .Load<float>("cameraSpeedSmoothVerticalSlider", _ => { cameraSpeedSmoothVerticalSlider.value = _; });

            _sensitiveData.SetSensitivity(sensivitySlider.value);
            _sensitiveData.SetRotateSpeedSmoothHorizontal(cameraSpeedSmoothHorizontalSlider.value);
            _sensitiveData.SetRotateSpeedSmoothVertical(cameraSpeedSmoothVerticalSlider.value);
        }       
        
        #endregion
    }
}