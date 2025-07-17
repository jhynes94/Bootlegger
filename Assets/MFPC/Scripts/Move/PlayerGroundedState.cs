namespace MFPC
{
    public class PlayerGroundedState : PlayerState
    {
        public PlayerGroundedState(Player player, PlayerStateMachine stateMachine, PlayerData playerData, MFPCPlayerRotation playerRotation) : base(
            player, stateMachine, playerData, playerRotation)
        {
        }

        public override void Enter()
        {
            player.Input.OnSitAction += OnSitEvent;
        }

        public override void Update()
        {
            playerRotation.UpdatePlayerRotation();
            
            if (player.CharacterController.isGrounded)
            {
                if (player.CurrentMoveCondition == MoveConditions.Fall) player.ChangeMoveCondition(MoveConditions.Fell);
            }
            else player.ChangeMoveCondition(MoveConditions.Fall);
        }

        public override void Exit()
        {
            player.Input.OnSitAction -= OnSitEvent;
        }
        
        #region Callback

        protected void OnJumpEvent()
        {
            stateMachine.ChangeState(stateMachine.JumpState);
        }

        private void OnSitEvent()
        {
            stateMachine.ChangeState(stateMachine.SitState);
        }

        #endregion
    }
}