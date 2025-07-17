using UnityEngine;

namespace MFPC.Camera
{
    public class LeanCamera : ICameraModule
    {
        private const float RayAdditionalDistance = 0.25f; 
        
        private readonly CharacterController _characterController;
        private readonly PlayerData _playerData;
        private Transform _cameraTransform;
        private float _blendLeanDirection;
        private float _leanDirection;
        private float _time;
        private bool _isZeroLean;

        public LeanCamera(Transform cameraTransform, PlayerData playerData, CharacterController characterController)
        {
            _playerData = playerData;
            _characterController = characterController;
            _cameraTransform = cameraTransform;
        }

        public void Update()
        {
            if (_cameraTransform == null) return;

            if (CanLean() && !_isZeroLean) Lean(_leanDirection);
            else Lean(0f);
            
            if (_isZeroLean && Mathf.Abs(_blendLeanDirection) < 0.01f)
            {
                _isZeroLean = false;
                _time = 0f;
            }
        }

        public void SetLeanDirection(float leanDirection)
        {
            leanDirection = Mathf.Clamp(leanDirection, -1, 1);

            if (_leanDirection == leanDirection) return;

            _leanDirection = leanDirection;
            _time = 0f;
        }

        private void Lean(float leanDirection)
        {
            _blendLeanDirection = Mathf.Lerp(_blendLeanDirection, leanDirection,
                Mathf.Clamp01(_time += _playerData.LeanSpeed * Time.deltaTime));

            _cameraTransform.localPosition = new Vector3(_playerData.LeanOffsetPositionX * _blendLeanDirection,
                _cameraTransform.localPosition.y, _cameraTransform.localPosition.z);

            _cameraTransform.localRotation = Quaternion.Euler(new Vector3(_cameraTransform.localRotation.eulerAngles.x,
                _cameraTransform.localRotation.eulerAngles.y, _playerData.LeanAngle * -_blendLeanDirection));
        }

        private bool CanLean()
        {
            if (_leanDirection == 0) return false;

            Bounds bounds = _characterController.bounds;
            Vector3 rayPosition = new Vector3(bounds.center.x, bounds.max.y, bounds.center.z);
            Vector3 rayDirection = _characterController.transform.TransformDirection(new Vector3(_leanDirection, 0f, 0f));
            float rayDistance = _playerData.LeanOffsetPositionX + RayAdditionalDistance;
            bool isObstacle = Physics.Raycast(rayPosition, rayDirection, rayDistance);

#if UNITY_EDITOR
            Debug.DrawRay(rayPosition, rayDirection * rayDistance, isObstacle ? Color.red : Color.green);
#endif

            if (isObstacle && !_isZeroLean)
            {
                _time = 0f;
                _isZeroLean = true;
            }

            return !isObstacle;
        }
    }
}