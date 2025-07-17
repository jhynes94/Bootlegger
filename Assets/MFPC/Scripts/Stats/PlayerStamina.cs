using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MFPC.PlayerStats
{
    [RequireComponent(typeof(Player))]
    public class PlayerStamina : PlayerStat
    {
        #region CONSTANT

        private const float maxStamina = 100f;

        #endregion

        public bool RunAbility { get; private set; } // True if it is possible to continue running
        public bool JumpAbility { get; private set; } // True if it is possible to jump

        [SerializeField, Range(0f, maxStamina)]
        private float stamina = maxStamina;

        [SerializeField, Range(1f, 10f), Tooltip("The intensity of the use of Stamina while running")]
        private float intencityRunUse = 5f;

        [SerializeField, Range(10f, 30f), Tooltip("The number that subtracts from the Stamina value when jumping")]
        private float jumpUse = 20f;

        [SerializeField, Range(1f, 10f), Tooltip("Stamina recovery rate")]
        private float intencityRecoveryStamina = 5f;

        [SerializeField, Range(5f, maxStamina), Tooltip("The value at which the run ability will be turned back on")]
        private float minRecoveryValue = 30f;

        private Player player;
        private bool activeRunCooldown = false;

        #region MONO

        protected void Awake()
        {
            player = this.GetComponent<Player>();
            MaxStatValue = maxStamina;
        }

        private void OnEnable() => player.OnMoveCondition += OnMoveConditionChanged;
        private void OnDisable() => player.OnMoveCondition -= OnMoveConditionChanged;

        #endregion

        private void Update()
        {
            if (player.CurrentMoveCondition == MoveConditions.Run)
            {
                UpdateStaminaValue(-intencityRunUse * Time.deltaTime);
            }
            else
            {
                UpdateStaminaValue(intencityRecoveryStamina * Time.deltaTime);
            }

            if (activeRunCooldown)
            {
                if (stamina >= minRecoveryValue)
                {
                    RunAbility = true;
                    activeRunCooldown = false;
                }
            }
            else
            {
                RunAbility = true;
            }

            JumpAbility = stamina >= jumpUse;
        }

        private void UpdateStaminaValue(float updateValue)
        {
            stamina = UpdateStatValue(stamina, updateValue);
            UpdateStatBar(stamina);

            if (stamina <= 0f)
            {
                RunAbility = false;
                activeRunCooldown = true;
            }
        }

        #region CALLBACK

        private void OnMoveConditionChanged(MoveConditions currentMoveCondition)
        {
            if (currentMoveCondition == MoveConditions.Jump) UpdateStaminaValue(-jumpUse);
        }

        #endregion
    }
}