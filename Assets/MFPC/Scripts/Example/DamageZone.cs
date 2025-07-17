using MFPC.PlayerStats;
using UnityEngine;

namespace MFPC.Example
{
    public class DamageZone : InteractTrigger
    {
        [SerializeField] private float damageValue;

        protected override void TriggerAction(Collider other)
        {
            if (other.TryGetComponent(out PlayerHealth playerHealth))
            {
                playerHealth.SetDamage(damageValue);
            }
        }
    }
}