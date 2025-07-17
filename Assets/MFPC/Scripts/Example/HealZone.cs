using MFPC.PlayerStats;
using UnityEngine;

namespace MFPC.Example
{
    public class HealZone : InteractTrigger
    {
        [SerializeField] private float healValue;

        protected override void TriggerAction(Collider other)
        {
            if (other.TryGetComponent(out PlayerHealth playerHealth))
            {
                playerHealth.SetHeal(healValue);
            }
        }
    }
}