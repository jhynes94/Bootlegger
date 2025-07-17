using UnityEngine;
using UnityEngine.UI;

namespace MFPC.Input.PlayerInput
{
    public class MobilePlayerInputHandler : PlayerInputHandler
    {
        [SerializeField] private Player _player;

        [SerializeField] private Joystick _joystick;
        [SerializeField] private RunField _runField;
        [SerializeField] private TouchField _touchField;
        [SerializeField] private LeanField _leanRight;
        [SerializeField] private LeanField _leanLeft;
        [SerializeField] private Button _sitButton;
        [SerializeField] private Button _jumpButton;
        private bool IsAvailableRunState;

        #region MONO

        private void Start() => ActiveButtons();

        private void OnEnable()
        {
            _jumpButton.onClick.AddListener(SetJumpInput);
            _sitButton.onClick.AddListener(SetSitInput);
            _leanRight.OnLeanDirectionChange += SetLeanDirection;
            _leanLeft.OnLeanDirectionChange += SetLeanDirection;
            _joystick.OnJoystickDragged += SetMoveInput;
        }

        private void OnDisable()
        {
            _jumpButton.onClick.RemoveAllListeners();
            _sitButton.onClick.RemoveAllListeners();
            _leanRight.OnLeanDirectionChange -= SetLeanDirection;
            _leanLeft.OnLeanDirectionChange -= SetLeanDirection;
            _joystick.OnJoystickDragged -= SetMoveInput;
        }

        #endregion

        private void Update()
        {
            SetLookInput(_touchField.GetSwipeDirection);
            SetSprintInput(_runField.InRunField);

            _runField.gameObject.SetActive(IsAvailableRunState && _player.StateMachine.RunState.IsChanged() &&
                                           _player.StateMachine.CurrentState !=
                                           _player.StateMachine.LadderMovementState);
        }

        public void SetJoystickWithRunField(Joystick joystick, RunField runField)
        {
            _joystick.OnJoystickDragged -= SetMoveInput;

            _joystick = joystick;
            _runField = runField;

            _joystick.OnJoystickDragged += SetMoveInput;
        }

        private void ActiveButtons()
        {
            _sitButton.gameObject.SetActive(_player.StateMachine.SitState != null);
            _jumpButton.gameObject.SetActive(_player.StateMachine.JumpState != null);
            _runField.gameObject.SetActive(IsAvailableRunState = _player.StateMachine.RunState != null);
            ShowLeanButtons(_player.StateMachine.LeanState != null);
        }

        private void ShowLeanButtons(bool isShow)
        {
            _leanRight.gameObject.SetActive(isShow);
            _leanLeft.gameObject.SetActive(isShow);
        }
        
        protected override float DeltaTimeMultiplier => 0.01f; 
    }
}