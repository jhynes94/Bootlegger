using MFPC.Utils;
using UnityEngine;

namespace MFPC
{
    [RequireComponent(typeof(AudioSource), typeof(Player))]
    public class MFPCFootstepSFX : MonoBehaviour
    {
        [Range(0.5f, 5.0f), SerializeField, Tooltip("Distance to play the next sound")]
        private float stepInterval = 0.5f;

        [Range(0.1f, 3.0f), SerializeField, Tooltip("Ray length at which texture is detected")]
        private float rayDistance = 0.5f;

        [Utils.CenterHeader("DataSFX"), SerializeField, Tooltip("Play if not found StepData with texture")]
        private StepData baseStepData;

        [SerializeField]
        private StepData[] stepsData;

        private Player player;
        private AudioSource audioSource;
        private Vector3 lastStepPosition;
        private float distanceTraveled = 0.0f;

        #region MONO

        private void Awake()
        {
            audioSource = this.GetComponent<AudioSource>();
            player = this.GetComponent<Player>();
        }

        private void OnEnable() => player.OnMoveCondition += OnChangeMoveCondition;
        private void OnDisable() => player.OnMoveCondition -= OnChangeMoveCondition;

        #endregion

        private void FixedUpdate()
        {
            Vector3 currentStepPosition = player.CharacterController.GetUnderPosition();

            if (CalculateDistanceTraveled(currentStepPosition) >= stepInterval) PlayFootstepSFX(currentStepPosition);

#if UNITY_EDITOR
            Debug.DrawRay(currentStepPosition, Vector3.down * rayDistance, Color.red);
#endif
        }

        /// <summary>
        /// Plays a footstep sound depending on the texture
        /// </summary>
        /// <param name="currentStepPosition"></param>
        private void PlayFootstepSFX(Vector3 currentStepPosition)
        {
            if (audioSource.isPlaying) return;

            Ray ray = new Ray(currentStepPosition, Vector3.down);
            if (Physics.Raycast(ray, out RaycastHit raycastHit, rayDistance))
            {
                Texture currentTexture = new Utils.TextureReader().GetTextureFromRaycast(raycastHit);
                StepData stepData = GetTargetStepData(currentTexture);
                AudioClip currentStepSFX = null;

                if (stepData != null) currentStepSFX = stepData.GetStepSFX();
                else if (baseStepData != null) currentStepSFX = baseStepData.GetStepSFX();

                if (currentStepSFX != null) audioSource.PlayOneShot(currentStepSFX);
            }

            distanceTraveled = 0.0f;
        }

        /// <summary>
        /// Calculates how far the player has traveled
        /// </summary>
        /// <returns>Distance traveled by the player</returns>
        private float CalculateDistanceTraveled(Vector3 currentStepPosition)
        {
            float distance = Vector3.Distance(currentStepPosition, lastStepPosition);
            lastStepPosition = currentStepPosition;
            return distanceTraveled += distance;
        }

        /// <summary>
        /// Finds the desired StepData by texture
        /// </summary>
        /// <param name="currentTexture">The texture on which the StepData will be located</param>
        /// <returns>Desired sound data</returns>
        private StepData GetTargetStepData(Texture currentTexture)
        {
            if (currentTexture == null) return null;

            foreach (var data in stepsData)
            {
                if (data.CompareTexture(currentTexture)) return data;
            }

            return null;
        }


        #region CALLBACK

        /// <summary>
        /// Play a step sound for desired movement states
        /// </summary>
        /// <param name="currentMoveCondition">Current state of movement</param>
        private void OnChangeMoveCondition(MoveConditions currentMoveCondition)
        {
            Vector3 currentStepPosition = lastStepPosition = player.CharacterController.GetUnderPosition();
            if ((currentMoveCondition.Equals(MoveConditions.Fell)
            || currentMoveCondition.Equals(MoveConditions.Walk)
            || currentMoveCondition.Equals(MoveConditions.Run))) PlayFootstepSFX(currentStepPosition);
        }

        #endregion
    }
}