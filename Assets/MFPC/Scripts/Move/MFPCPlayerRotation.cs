using MFPC.Input;
using MFPC.Input.PlayerInput;
using MFPC.Utils;
using UnityEngine;

namespace MFPC
{
    public class MFPCPlayerRotation
    {
        public float SetRotation {set => LookDirection = value; }
        
        /// <summary>
        /// The direction the player is facing (Vertical)
        /// </summary>
        private float LookDirection;

        private IPlayerInput _playerInput;
        private PlayerInputTuner _playerInputTuner;
        private SensitiveData _sensitiveData;
        private Transform _playerTransform;

        public MFPCPlayerRotation(Transform playerTransform, IPlayerInput playerInput,
            PlayerInputTuner playerInputTuner, SensitiveData sensitiveData)
        {
            _playerTransform = playerTransform;
            _playerInput = playerInput;
            _playerInputTuner = playerInputTuner;
            _sensitiveData = sensitiveData;

            LookDirection = _playerTransform.rotation.eulerAngles.y;
        }

        public void UpdatePlayerRotation()
        {
            LookDirection += _playerInput.CalculatedHorizontalLookDirection;
            _playerTransform.localRotation = RotateHelper.SmoothRotateHorizontal(_playerTransform.localRotation,
                _sensitiveData.RotateSpeedSmoothHorizontal, LookDirection);
        }
    }
}