using UnityEngine;
using UnityEngine.UI;

namespace MFPC.PlayerStats
{
    public abstract class PlayerStat : MonoBehaviour
    {
        [SerializeField] private Image statBar;
        
        protected float MaxStatValue { private get; set; }
        
        /// <summary>
        /// Updates the stat in a range
        /// </summary>
        /// <param name="currentValue">Stat value</param>
        /// <param name="updateValue">By how much will the state change</param>
        /// <returns>New stat vulue</returns>
        protected float UpdateStatValue(float currentValue, float updateValue)
        {
            return currentValue = Mathf.Clamp(currentValue + updateValue, 0f, MaxStatValue);
        }

        protected void UpdateStatBar(float currentValue)
        {
            statBar.fillAmount = currentValue / MaxStatValue;
        }
    }
}
