using UnityEngine;

namespace Atlantis.Events
{
    public readonly struct EnemyDamageEvent : IEvent
    {

        public float DamageAmount { get; }
        public Vector3 DamageDirection { get; }
        public GameObject DamageSource { get; }

        public EnemyDamageEvent(float damageAmount, Vector3 damageDirection, GameObject damageSource)
        {
            DamageAmount = damageAmount;
            DamageDirection = damageDirection;
            DamageSource = damageSource;
        }
    }
}

