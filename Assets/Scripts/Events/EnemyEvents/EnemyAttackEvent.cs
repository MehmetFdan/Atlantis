using UnityEngine;

namespace Atlantis.Events 
{
    public readonly struct EnemyAttackEvent : IEvent
    {
        public Transform Target { get; }
        public float AttackPower { get; }

        public EnemyAttackEvent(Transform target, float attackPower)
        {
            Target = target;
            AttackPower = attackPower;
        }
    }
}

