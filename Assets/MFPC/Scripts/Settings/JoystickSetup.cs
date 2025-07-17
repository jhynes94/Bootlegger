using System;
using UnityEngine;
using UnityEngine.UI;
using MFPC.Input.PlayerInput;
using MFPC.Utils.SaveLoad;

namespace MFPC
{
    [Serializable]
    public class JoystickSetup
    {
        [Serializable]
        private struct JoystickField
        {
            [field: SerializeField] public Joystick Joystick { get; private set; }
            [field: SerializeField] public RunField RunField { get; private set; }

            [SerializeField] private GameObject group;

            public void Draw(bool isActive)
            {
                group.SetActive(isActive);
            }
        }

        private enum JoystickType
        {
            WithRunField = 0,
            Standard = 1
        }

        [SerializeField] private JoystickType startJoystickType = JoystickType.Standard;
        [SerializeField] private MobilePlayerInputHandler mobilePlayerInputHandler;
        [SerializeField] private Toggle fixedJoystickStateToggle;
        [SerializeField] private Toggle fadingJoystickStateToggle;
        [SerializeField] private Button firstJoystickTypeButton;
        [SerializeField] private Button secondJoystickTypeButton;
        [SerializeField] private JoystickField[] joystickFields = new JoystickField[2];

        private JoystickField _currentJoystickField;
        private ISaver _saver;

        public void Initialize()
        {
            _saver = new PlayerPrefsSaver("JoystickSetup");

            Load();
            
            fixedJoystickStateToggle.onValueChanged.AddListener(ChangeFixedJoystickState);
            fadingJoystickStateToggle.onValueChanged.AddListener(ChangeFadingJoystickState);
            firstJoystickTypeButton.onClick.AddListener(() => SetJoystickWithRunField(0));
            secondJoystickTypeButton.onClick.AddListener(() => SetJoystickWithRunField(1));

            if (joystickFields.Length != Enum.GetNames(typeof(JoystickType)).Length)
                throw new Exception("The number of Joystick Type does not match the number of selected joysticks");
            
            SetStartJoystickField(startJoystickType);
        }

        ~JoystickSetup()
        {
            fixedJoystickStateToggle.onValueChanged.RemoveAllListeners();
            fadingJoystickStateToggle.onValueChanged.RemoveAllListeners();
            firstJoystickTypeButton.onClick.RemoveAllListeners();
            secondJoystickTypeButton.onClick.RemoveAllListeners();
        }

        private void ChangeFixedJoystickState(bool isOn)
        {
            Array.ForEach(joystickFields, x => x.Joystick.SetFixedJoystickState = isOn);
            Save();
        }

        private void ChangeFadingJoystickState(bool isOn)
        {
            Array.ForEach(joystickFields, x => x.Joystick.SetFadingJoystickState = isOn);
            Save();
        }

        private void SetJoystickWithRunField(uint index)
        {
            if (index >= joystickFields.Length) throw new IndexOutOfRangeException();

            DrawJoystickField(index);

            startJoystickType = (JoystickType)index;
            mobilePlayerInputHandler.SetJoystickWithRunField(joystickFields[index].Joystick,
                joystickFields[index].RunField);
            
            Save();
        }

        private void DrawJoystickField(uint index)
        {
            _currentJoystickField.Draw(false);

            _currentJoystickField = joystickFields[index];

            _currentJoystickField.Draw(true);
        }

        private void SetStartJoystickField(JoystickType joystickType)
        {
            _currentJoystickField = joystickFields[0];

            foreach (var joystickField in joystickFields)
            {
                joystickField.Draw(false);
            }

            SetJoystickWithRunField((uint) startJoystickType);
        }

        #region Save&Load

        private void Save()
        {
            _saver.Save("fixedJoystickStateToggle", fixedJoystickStateToggle.isOn)
                .Save("fadingJoystickStateToggle", fadingJoystickStateToggle.isOn)
                .Save("startJoystickType", (int) startJoystickType);
        }

        private void Load()
        {
            _saver.Load<bool>("fixedJoystickStateToggle", _ => { fixedJoystickStateToggle.isOn = _; })
                .Load<bool>("fadingJoystickStateToggle", _ => { fadingJoystickStateToggle.isOn = _; })
                .Load<int>("startJoystickType", _ => { startJoystickType = (JoystickType) _; });
            
            ChangeFixedJoystickState(fixedJoystickStateToggle.isOn);
            ChangeFadingJoystickState(fadingJoystickStateToggle.isOn);
        }

        #endregion
    }
}