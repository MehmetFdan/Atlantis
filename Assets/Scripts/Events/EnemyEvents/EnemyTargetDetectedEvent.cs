using UnityEngine;

namespace Atlantis.Events 
{
    public readonly struct EnemyTargetDetectedEvent : IEvent
    {
        public Transform Target { get; }
        public float Distance { get; }

        public EnemyTargetDetectedEvent(Transform target, float distance)
        {
            Target = target;
            Distance = distance;
        }
    }
}

