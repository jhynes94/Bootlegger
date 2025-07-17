using System.Collections.Generic;
using MFPC.Input.PlayerInput;
using UnityEngine;

namespace MFPC.Camera
{
    public class CameraModuleManager
    {
        private List<ICameraModule> _cameraModules = new List<ICameraModule>();

        public CameraModuleManager(Transform cameraTransform, PlayerData playerData, Player player)
        {
            if (playerData.IncreaseFOV)
            {
                AddModule(new RunFovCameraEffect(cameraTransform.GetComponentInChildren<UnityEngine.Camera>(),
                    player.Input, playerData, player));
            }
        }

        public void Update()
        {
            foreach (var module in _cameraModules) module.Update();
        }

        public void AddModule(ICameraModule newModule)
        {
            _cameraModules.Add(newModule);
        }
    }
}