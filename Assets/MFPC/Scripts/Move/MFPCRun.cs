using MFPC.Input;
using MFPC.PlayerStats;
using UnityEngine;

namespace MFPC
{
    /// <summary>
    /// Allows the player to move forward only
    /// </summary>
    public class MFPCRun : PlayerGroundedState
    {
        private PlayerStamina _playerStamina;

        public MFPCRun(Player player, PlayerStateMachine stateMachine, PlayerData playerData, MFPCPlayerRotation playerRotation) : base(
            player, stateMachine, playerData, playerRotation)
        {
            player.TryGetComponent(out _playerStamina);
        }

        public override bool IsChanged()
        {
            return TryCheckRunAbility();
        }

        public override void Enter()
        {
            base.Enter();

            player.Input.OnJumpAction += OnJumpEvent;
        }

        public override void Update()
        {
            base.Update();

            RunPlayer();
            
            if (!player.Input.IsSprint || !TryCheckRunAbility()) stateMachine.ChangeState(stateMachine.MovementState);
            
            if (player.CharacterController.isGrounded)
            {
                player.ChangeMoveCondition(MoveConditions.Run);
            }
        }

        public override void Exit()
        {
            base.Exit();

            player.Input.OnJumpAction -= OnJumpEvent;
        }

        private void RunPlayer()
        {
            player.Movement.MoveHorizontal(Vector3.forward, playerData.RunSpeed);
        }

        /// <summary>
        /// Check if the player has a stamina component, if so, checks if it does
        /// </summary>
        /// <returns></returns>
        private bool TryCheckRunAbility()
        {
            if (_playerStamina != null && _playerStamina.enabled)
            {
                return _playerStamina.RunAbility;
            }

            return true;
        }
    }
}