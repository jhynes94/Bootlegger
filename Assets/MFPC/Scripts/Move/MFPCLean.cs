using MFPC.Camera;
using UnityEngine;

namespace MFPC
{
    public class MFPCLean : PlayerGroundedState
    {
        private MFPCCameraRotation _cameraRotation;
        private LeanCamera _leanCamera;
        
        public MFPCLean(Player player, PlayerStateMachine stateMachine, PlayerData playerData,  MFPCPlayerRotation playerRotation, MFPCCameraRotation cameraRotation)
            : base(player, stateMachine, playerData, playerRotation)
        {
            _cameraRotation = cameraRotation;
            _leanCamera = new LeanCamera(_cameraRotation.transform, playerData, player.CharacterController);
            _cameraRotation.CameraModuleManager.AddModule(_leanCamera);
        }

        public override void Enter()
        {
            base.Enter();
            
            player.ChangeMoveCondition(MoveConditions.Lean);
        }

        public override void Update()
        {
            base.Update();
            
            if(player.Input.LeanDirection == 0) stateMachine.ChangeState(stateMachine.MovementState);
            
            _leanCamera.SetLeanDirection(player.Input.LeanDirection);

            Move();
        }

        public override void Exit()
        {
            base.Exit();
            
            _leanCamera.SetLeanDirection(0);
        }

        private void Move()
        {
            player.Movement.MoveHorizontal(
                new Vector3(player.Input.MoveDirection.x, 0.0f, player.Input.MoveDirection.y),
                playerData.LeanMoveSpeed);
        }
    }
}