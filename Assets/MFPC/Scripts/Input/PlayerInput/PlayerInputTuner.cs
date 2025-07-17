using UnityEngine;

using MFPC.Utils;

namespace MFPC.Input.PlayerInput
{
    public class PlayerInputTuner : MonoBehaviour
    {   
        public IPlayerInput CurrentPlayerInputHandler { get => _proxyPlayerInputHandler; } 
        public bool IsLockInput { set => _proxyPlayerInputHandler.SetLockInput(value); }
        
        [SerializeField, Utils.CenterHeader("Input Data")] 
        private MobilePlayerInputHandler _mobilePlayerInputHandler;
        
        private ProxyPlayerInputHandler _proxyPlayerInputHandler;
        private ReactiveProperty<InputType> _currentInputType;

        public void Initialize(ReactiveProperty<InputType> currentInputType, SensitiveData sensitiveData)
        {
            _proxyPlayerInputHandler = new ProxyPlayerInputHandler(sensitiveData);
            _currentInputType = currentInputType;
            _currentInputType.Subscribe(ChangeInputHandler);
            
            ChangeInputHandler(_currentInputType.Value);
        }

        private void OnDestroy()
        {
            _currentInputType.Unsubscribe(ChangeInputHandler);
        }

        private void ChangeInputHandler(InputType inputType)
        {
            switch (inputType)
            {
                case InputType.Mobile:
                    _proxyPlayerInputHandler.SetPlayerInputHandler(_mobilePlayerInputHandler);
                    _mobilePlayerInputHandler.gameObject.SetActive(true);
                    Cursor.lockState = CursorLockMode.None;
                    break;

                case InputType.KeyboardMouse:
                case InputType.Gamepad:
                    _proxyPlayerInputHandler.SetPlayerInputHandler(GetCurrentInputHandler(inputType));
                    _mobilePlayerInputHandler.gameObject.SetActive(false);
                    Cursor.lockState = CursorLockMode.Locked;
                    break;
            }
        }

        private T GetPlayerInputHandler<T>() where T : PlayerInputHandler
        {
            if (!TryGetComponent(out T playerInputHandler))
            {
                playerInputHandler = gameObject.AddComponent<T>();
            }

            return playerInputHandler;
        }
        
        private PlayerInputHandler GetCurrentInputHandler(InputType inputType)
        {
#if !ENABLE_INPUT_SYSTEM
            OldPlayerInputHandler playerInputHandler = GetPlayerInputHandler<OldPlayerInputHandler>();
#else
            NewPlayerInputHandler playerInputHandler = GetPlayerInputHandler<NewPlayerInputHandler>();
            playerInputHandler.Initialize(_currentInputType);
#endif

            return playerInputHandler;
        } 
    }
}