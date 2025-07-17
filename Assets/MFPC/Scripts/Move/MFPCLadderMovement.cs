using UnityEngine;
using MFPC.Utils;

namespace MFPC
{
    /// <summary>
    /// Allows the player to climb ladder
    /// </summary>
    public class MFPCLadderMovement : PlayerState
    {
        private LadderArea ladderArea;
        private Quaternion playerStartClimbUpRotation;
        private Vector3 playerStartClimbUpPosition;

        private float clingPlayerRotation; 
        private float timeToPlayerSetRotation;
        private float timeToPlayerSetPosition;
        private float localLookDirection;

        public MFPCLadderMovement(Player player, PlayerStateMachine stateMachine, PlayerData playerData, MFPCPlayerRotation playerRotation) : base(
            player, stateMachine, playerData, playerRotation)
        { }

        public override void Update()
        {
            Climb();
            RotatePlayer();
        }

        /// <summary>
        /// Ladder start interaction action
        /// </summary>
        public void ClimbUp(LadderArea ladderArea)
        {
            this.ladderArea = ladderArea;
            
            timeToPlayerSetPosition = timeToPlayerSetRotation = localLookDirection = 0.0f;
            playerStartClimbUpPosition = player.transform.position;
            playerStartClimbUpRotation = player.transform.rotation;
            player.Movement.IsLockGravity = true;

            player.Movement.MoveHorizontal(Vector3.zero);
            FindCameraAngleToLadder();
        }

        /// <summary>
        /// Character movement process
        /// </summary>
        private void Climb()
        {
            float vertical = player.Input.MoveDirection.y;
            
            player.Movement.MoveVertical(new Vector3(0.0f, player.Input.MoveDirection.y, 0.0f), playerData.ClimbSpeed);
            player.ChangeMoveCondition(MoveConditions.Climb);
            PlayClimbSound(vertical);

            //Moving the player to the middle of the ladder
            Vector3 position = player.transform.position;
            position = Vector3.LerpUnclamped(
                new Vector3(playerStartClimbUpPosition.x, position.y, playerStartClimbUpPosition.z),
                new Vector3(ladderArea.BottomLadderPosition.x, position.y,
                    ladderArea.BottomLadderPosition.z),
                Mathf.Clamp01(timeToPlayerSetPosition += Time.deltaTime * playerData.NormalizationSpeed));
            player.CharacterController.Transfer(position);

            //If the character has climbed to the top of the ladder, then he climb down.
            if (ladderArea.IsEndLadderPosition(player.transform.position)) ClimbDown(Vector3.forward, 3.5f);

            //If the character cannot move down, then he climb down.
            if (vertical < 0 && ladderArea.IsBottomPosition(player.transform.position)) ClimbDown(Vector3.zero, 0f);

            //If the character climbs to the bottom of the top of the ladder, then he climb down.
            if (vertical < 0 && ladderArea.IsStartLadderPosition(player.transform.position)) ClimbDown(Vector3.zero, 0f);
        }

        /// <summary>
        /// Character stops interacting with ladder
        /// </summary>
        /// <param name="climbDownDirection">direction for the character to climb down the ladder</param>
        private void ClimbDown(Vector3 climbDownDirection, float speed)
        {
            //Sets the direction for the character to climb down the ladder
            player.Movement.MoveVertical(Vector3.zero);
            player.Movement.MoveHorizontal(climbDownDirection, speed);
            player.Movement.IsLockGravity = false;
            playerRotation.SetRotation = localLookDirection + clingPlayerRotation;

            player.AudioSource.Stop();
            player.StateMachine.ChangeState(player.StateMachine.MovementState);
        }

        /// <summary>
        /// Rotate the character for the direction of movement
        /// </summary>
        private void RotatePlayer()
        {
            localLookDirection += player.Input.CalculatedHorizontalLookDirection;
            localLookDirection = Mathf.Clamp(localLookDirection, playerData.RangeCameraRotation.x,
                playerData.RangeCameraRotation.y);

            Quaternion rt = RotateHelper.SmoothRotateHorizontal(player.transform.localRotation,
                player.SensitiveData.RotateSpeedSmoothHorizontal, localLookDirection, clingPlayerRotation);

            player.transform.rotation = Quaternion.LerpUnclamped(
                playerStartClimbUpRotation,
                rt,
                Mathf.Clamp01(timeToPlayerSetRotation += Time.deltaTime * playerData.NormalizationRotateSpeed));
        }

        /// <summary>
        /// Finding an angle so that the camera looks exactly at the ladder
        /// </summary>
        private void FindCameraAngleToLadder()
        {
            Vector3 targetDiraction = ladderArea.LadderTransform.position - (ladderArea.LadderTransform.position +
                                                                             ladderArea.LadderTransform
                                                                                 .TransformDirection(Vector3.right));
            clingPlayerRotation = Mathf.Atan2(targetDiraction.x, targetDiraction.z) * 180 / Mathf.PI;
        }

        private void PlayClimbSound(float vertical)
        {
            if (vertical == 0) player.AudioSource.Stop();
            if (!player.AudioSource.isPlaying) PlaySound(playerData.ClimbSFX);
        }
    }
}