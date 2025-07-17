using UnityEngine;
using MFPC.Utils;

namespace MFPC
{
    [System.Flags]
    [Docs("Defines player states.")]
    public enum PlayerStates
    {
        Move = 1,
        Jump = 2,
        Sit = 4,
        Run = 8,
        Ladder = 16,
        Lean = 32
    }

    [Docs("Stores player config.")]
    [CreateAssetMenu(fileName = "PlayerData", menuName = "MFPC/PlayerData")]
    public class PlayerData : ScriptableObject
    {
        [Docs("Determines available player states.")]
        [field: SerializeField]
        public PlayerStates AvailablePlayerStates { get; private set; } = PlayerStates.Move;

        #region Move State

        [HeaderData("Move")]
        [Docs("Range of vertical camera rotation.")]
        [field: SerializeField]
        public Vector2 RangeCameraRotationVertical { get; private set; } = new Vector2(-90, 90);

        [Docs("Walk speed of the player.")]
        [field: SerializeField]
        public float WalkSpeed { get; private set; }

        [Docs("Gravity affecting the player.")]
        [field: SerializeField]
        public float Gravity { get; private set; }

        [Docs("When AirControl is enabled, then the player can control the direction of flight.")]
        [field: SerializeField]
        public bool AirControl { get; private set; }

        [Docs("When inertia is enabled, the player starts and ends the movement smoothly.")]
        [field: SerializeField]
        public bool MoveInertia { get; private set; }

        [Docs("The speed at which the player stops. (Work with MoveInertia)")]
        [field: SerializeField]
        public float Deceleration { get; private set; } = 4f;

        [Docs("The speed at which the player starts moving. (Work with MoveInertia)")]
        [field: SerializeField]
        public float Acceleration { get; private set; } = 4f;

        #endregion

        #region Run State

        [HeaderData("Run")]
        [Docs("Running speed of the player.")]
        [field: SerializeField]
        public float RunSpeed { get; private set; }

        [Docs("When IncreaseFOV is enabled, the FOV (field of view) is increased while running.")]
        [field: SerializeField]
        public bool IncreaseFOV { get; private set; }

        [Docs("FOV value while running.")]
        [field: SerializeField]
        public float RunFOV { get; private set; } = 65f;

        [Docs("Speed of FOV change.")]
        [field: SerializeField]
        public float SpeedChangeFOV { get; private set; } = 3.5f;

        #endregion

        #region Jump State

        [HeaderData("Jump")]
        [Docs("Jump force applied to the player.")]
        [field: SerializeField]
        public float JumpForce { get; private set; }

        [Docs("Sound effect played when jumping.")]
        [field: SerializeField]
        public AudioClip JumpSFX { get; private set; }

        [Docs("Distance of the ray under the player to check ground.")]
        [field: SerializeField]
        public float UnderRayDistance { get; private set; } = 0.5f;

        #endregion

        #region Sit State

        [HeaderData("Sit")]
        [Docs("Crouch walk speed of the player.")]
        [field: SerializeField]
        public float SitWalkSpeed { get; private set; }

        [Docs("CharacterController height when character is crouched.")]
        [field: SerializeField]
        public float SitHeight { get; private set; }

        [Docs("Rate of altitude change.")]
        [field: SerializeField]
        public float ChangeSitSpeed { get; private set; }

        [Docs("Length of the ray checking space above the player.")]
        [field: SerializeField]
        public float RayDistance { get; private set; }

        [Docs("Sound effect played when sitting and standing up.")]
        [field: SerializeField]
        public AudioClip SitSFX { get; private set; }

        #endregion

        #region Ladder State

        [HeaderData("Ladder")]
        [Docs("The speed at which a character climbs ladder.")]
        [field: SerializeField]
        public float ClimbSpeed { get; private set; }

        [Docs("Character movement speed to the center of the ladder.")]
        [field: SerializeField]
        public float NormalizationSpeed { get; private set; } = 5f;

        [Docs("Character rotate speed to the center of the ladder.")]
        [field: SerializeField]
        public float NormalizationRotateSpeed { get; private set; } = 5f;

        [Docs("Range of camera vertical rotation while climbing the ladder.")]
        [field: SerializeField]
        public Vector2 RangeCameraRotation { get; private set; } = new Vector2(-60, 60);

        [Docs("Sound effect played when climbing the ladder.")]
        [field: SerializeField]
        public AudioClip ClimbSFX { get; private set; }

        #endregion

        #region Lean State

        [HeaderData("Lean")]
        [Docs("Angle at which the player leans.")]
        [field: SerializeField]
        public float LeanAngle { get; private set; } = 30f;

        [Docs("Position offset while leaning.")]
        [field: SerializeField]
        public float LeanOffsetPositionX { get; private set; } = 0.3f;

        [Docs("Speed of leaning.")]
        [field: SerializeField]
        public float LeanSpeed { get; private set; } = 0.5f;

        [Docs("Speed of player movement while leaning.")]
        [field: SerializeField]
        public float LeanMoveSpeed { get; private set; } = 2f;

        #endregion

        [Docs("Checks if a given player state is available.")]
        public bool IsAvailableState(PlayerStates state)
        {
            return AvailablePlayerStates.HasFlag(state);
        }
    }
}
