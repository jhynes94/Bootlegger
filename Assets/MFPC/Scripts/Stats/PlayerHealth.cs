using UnityEngine;
using System;

namespace MFPC.PlayerStats
{
    public class PlayerHealth : PlayerStat
    {
        #region EVENTS

        public static Action PlayerDeath;

        #endregion

        #region CONSTANT

        private const float maxHealth = 100f;

        #endregion

        [SerializeField]
        private Animation damageScreenAnimation;

        [SerializeField]
        private Animation healScreenAnimation;

        [SerializeField, Range(0f, 100f)]
        private float health = 100f;

        #region MONO

        protected void Awake()
        {
            MaxStatValue = maxHealth;
        }

        private void Start() { }

        #endregion

        /// <summary>
        /// Decreases amount of health
        /// </summary>
        /// <param name="damage">The number that breaks from health</param>
        public void SetDamage(float damage)
        {
            if (damage < 0) return;

            UpdateHealthValue(-damage);
            PlayScreenAnimation(damageScreenAnimation);
        }

        /// <summary>
        /// Increases amount of health
        /// </summary>
        /// <param name="heal">The number by how much health is restored</param>
        public void SetHeal(float heal)
        {
            if (heal < 0) return;
            if (health < maxHealth) PlayScreenAnimation(healScreenAnimation);

            UpdateHealthValue(heal);
        }

        private void UpdateHealthValue(float updateValue)
        {
            health = UpdateStatValue(health, updateValue);
            UpdateStatBar(health);

            if (health == 0f) PlayerDeath?.Invoke();
        }

        private void PlayScreenAnimation(Animation screenAnimation)
        {
            if (!screenAnimation.isPlaying) screenAnimation.Play();
        }
    }
}