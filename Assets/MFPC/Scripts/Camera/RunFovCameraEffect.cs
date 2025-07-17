using MFPC.Input.PlayerInput;
using UnityEngine;

namespace MFPC.Camera
{
    public class RunFovCameraEffect : ICameraModule
    {
        private UnityEngine.Camera _camera;
        private IPlayerInput _playerInput;
        private PlayerData _playerData;
        private Player _player;
        private float _initialFov;
        private float _time;

        public RunFovCameraEffect(UnityEngine.Camera camera, IPlayerInput playerInput, PlayerData playerData,
            Player player)
        {
            _camera = camera;
            _playerInput = playerInput;
            _playerData = playerData;
            _player = player;

            _initialFov = _camera.fieldOfView;
        }

        public void Update()
        {
            if (_camera == null) return;
            
            _time += (Time.deltaTime * _playerData.SpeedChangeFOV) * (IsIncreaseFOV() ? 1 : -1);
            _time = Mathf.Clamp01(_time);

            _camera.fieldOfView = Mathf.Lerp(_initialFov, _playerData.RunFOV, _time);
        }

        private bool IsIncreaseFOV() => _playerInput.IsSprint && _player.CurrentMoveCondition != MoveConditions.Climb 
                                                              && _player.CurrentMoveCondition != MoveConditions.Fell
                                                              && _player.CurrentMoveCondition != MoveConditions.Lean;
    }
}