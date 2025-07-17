using UnityEngine;
using MFPC.PlayerStats;
using MFPC.Utils;

namespace MFPC
{
    /// <summary>
    /// Allows the character to jump
    /// </summary>
    public class MFPCJump : PlayerGroundedState
    {
        private PlayerStamina _playerStamina;
        private float oldPlayerPositionY;
        private bool playerFall;

        public MFPCJump(Player player, PlayerStateMachine stateMachine, PlayerData playerData, MFPCPlayerRotation playerRotation) : base(
            player, stateMachine, playerData, playerRotation)
        {
            player.TryGetComponent(out _playerStamina);
        }

        public override bool IsChanged()
        {
            return TryCheckJumpAbility();
        }

        public override void Enter()
        {
            base.Enter();
            oldPlayerPositionY = -player.transform.position.y;
            playerFall = false;

            Jump();
        }

        public override void Update()
        {
            base.Update();

            // Checking if we can jump higher
            if (player.transform.position.y == oldPlayerPositionY) player.Movement.MoveVertical(Vector3.zero);
            if (player.CharacterController.isGrounded && playerFall)
                stateMachine.ChangeState(stateMachine.MovementState);
            if (!player.CharacterController.isGrounded) playerFall = true;

            oldPlayerPositionY = player.transform.position.y;
        }

        /// <summary>
        /// Directs the character up (jump)
        /// </summary>
        private void Jump()
        {
            if (IsGround())
            {
                player.Movement.MoveVertical(Vector3.up, playerData.JumpForce);
                player.ChangeMoveCondition(MoveConditions.Jump);
                PlaySound(playerData.JumpSFX);
            }
        }

        private bool TryCheckJumpAbility()
        {
            if (_playerStamina != null && _playerStamina.enabled)
            {
                return _playerStamina.JumpAbility;
            }

            return true;
        }

        private bool IsGround()
        {
            Ray ray = new Ray(player.CharacterController.GetUnderPosition(), Vector3.down);
            return Physics.Raycast(ray, out RaycastHit raycastHit, playerData.UnderRayDistance);
        }
    }
}