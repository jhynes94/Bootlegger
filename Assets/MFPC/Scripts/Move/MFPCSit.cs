using UnityEngine;

namespace MFPC
{
    /// <summary>
    /// Allows the character to sit down (reducing character height)
    /// </summary>
    public class MFPCSit : PlayerGroundedState
    {
        private bool isSit;
        private float standHeight;
        private float t;

        public MFPCSit(Player player, PlayerStateMachine stateMachine, PlayerData playerData, MFPCPlayerRotation playerRotation) : base(
            player, stateMachine, playerData, playerRotation)
        {
            standHeight = player.CharacterController.height;
        }

        public override void Enter()
        {
            player.Input.OnSitAction += DeactivateSitState;

            Sit();
        }

        public override void Exit()
        {
            isSit = false;
            player.CharacterController.height = standHeight;
            
            player.Input.OnSitAction -= DeactivateSitState;
        }

        public override void Update()
        {
            base.Update();

            if (player.CharacterController.isGrounded)
            {
                player.Movement.MoveHorizontal(new Vector3(player.Input.MoveDirection.x, 0.0f, player.Input.MoveDirection.y),
                    playerData.SitWalkSpeed);
            }
            
            if (!isSit && player.CharacterController.height == standHeight)
                stateMachine.ChangeState(player.StateMachine.MovementState);
            
            t += (isSit ? Time.deltaTime : -Time.deltaTime) * playerData.ChangeSitSpeed;
            player.CharacterController.height = Mathf.Lerp(standHeight, playerData.SitHeight, t = Mathf.Clamp01(t));

#if UNITY_EDITOR
            var bounds = player.CharacterController.bounds;
            Debug.DrawRay(new Vector3(bounds.center.x, bounds.max.y, bounds.center.z),
                player.transform.TransformDirection(Vector3.up) * playerData.RayDistance, Color.green);
#endif
        }

        /// <summary>
        /// Changes character's height
        /// </summary>
        private void Sit()
        {
            if (!IsStandUp()) return;

            isSit = !isSit;

            player.ChangeMoveCondition(MoveConditions.Sit);
            PlaySound(playerData.SitSFX);
        }

        /// <summary>
        /// Checks if space is at the top
        /// </summary>
        /// <returns>True if the top is empty</returns>
        private bool IsStandUp()
        {
            var bounds = player.CharacterController.bounds;
            return !(Physics.Raycast(new Vector3(bounds.center.x, bounds.max.y,
                bounds.center.z), player.transform.TransformDirection(Vector3.up), playerData.RayDistance) && isSit == true);
        }

        public void DeactivateSitState() => Sit();
    }
}