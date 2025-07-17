using UnityEngine;

namespace MFPC
{
    /// <summary>
    /// The area the playerPosition enters switches their state to ladder movement
    /// </summary>
    [ExecuteInEditMode]
    public class LadderArea : MonoBehaviour
    {
        public Transform LadderTransform { get; private set; }

        /// <summary>
        /// The position where the ray from the top corner of the collider cross the surface from below
        /// </summary>
        public Vector3 BottomLadderPosition { get; private set; }

        /// <summary>
        /// The lowest part of the collider stairs
        /// </summary>
        private Vector3 StartLadderPosition;

        /// <summary>
        /// The topmost part of the collider
        /// </summary>
        private Vector3 EndLadderPosition;
        
        /// <summary>
        /// The lowest part of the collider
        /// </summary>
        private Vector3 EndCenterCornerPosition;

        #region MONO

        private void Awake()
        {
            StartLadderPosition = this.GetComponent<Renderer>().bounds.min;
            EndLadderPosition = this.GetComponent<Renderer>().bounds.max;
            EndCenterCornerPosition = transform.TransformPoint(new Vector3(0.5f, 0.5f, 0.0f)) +
                                      transform.TransformDirection(Vector3.right) / 10f;
            LadderTransform = this.transform;

            FindBottomPosition();
        }

        #endregion

#if UNITY_EDITOR
        private void Update()
        {
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.right), Color.red);
            Awake();
        }
#endif

        /// <summary>
        /// Find the lowest position after which it is impossible to continue descending
        /// </summary>
        private void FindBottomPosition()
        {
            RaycastHit hitInfo;
            Vector3 startRayPosition =
                new Vector3(EndCenterCornerPosition.x, EndLadderPosition.y, EndCenterCornerPosition.z);
            Ray ray = new Ray(startRayPosition, transform.TransformDirection(Vector3.down));

            if (Physics.Raycast(ray, out hitInfo))
            {
                BottomLadderPosition = hitInfo.point;

                // Gets a vector that points from the playerPosition's position to the target's.
                var heading = hitInfo.point - ray.origin;
                var distance = heading.magnitude;
                var direction = heading / distance; // This is now the normalized direction.
#if UNITY_EDITOR
                Debug.DrawRay(ray.origin, direction * distance, Color.red);
#endif
            }
        }

        /// <summary>
        ///The range of position Y at which the playerPosition climb down
        /// </summary>
        public bool IsBottomPosition(Vector3 playerPosition)
        {
            float min = BottomLadderPosition.y - 0.05f;
            float max = BottomLadderPosition.y + 0.05f;

            return playerPosition.y >= min && playerPosition.y <= max;
        }

        /// <summary>
        /// Checks if the player has descended to the start of the ladder position
        /// </summary>
        public bool IsStartLadderPosition(Vector3 playerPosition)
        {
            return playerPosition.y <= StartLadderPosition.y;
        }

        /// <summary>
        /// Checks if the player has climbed to the top of the ladder
        /// </summary>
        public bool IsEndLadderPosition(Vector3 playerPosition)
        {
            return playerPosition.y >= EndLadderPosition.y;
        }

        private void OnTriggerEnter(Collider other)
        {
            Player player = other.GetComponent<Player>();

            if (player)
            {
                if (player.StateMachine.ChangeState(player.StateMachine.LadderMovementState))
                {
                    ((MFPCLadderMovement) player.StateMachine.CurrentState).ClimbUp(this);
                }
            }
        }
    }
}