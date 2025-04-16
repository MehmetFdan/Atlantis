using UnityEngine;

namespace Atlantis.Events 
{
    public readonly struct EnemyRangedAttackEvent : IEvent
    {

        public Transform Target { get; }
        public float AttackPower { get; }
        public Vector3 Direction { get; }

        public EnemyRangedAttackEvent(Transform target, float attackPower, Vector3 direction)
        {
            Target = target;
            AttackPower = attackPower;
            Direction = direction;
        }
    }
}
