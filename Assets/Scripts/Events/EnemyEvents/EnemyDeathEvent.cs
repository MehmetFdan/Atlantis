using UnityEngine;

namespace Atlantis.Events
{
    public readonly struct EnemyDeathEvent : IEvent
    {

        public GameObject Enemy { get; }

        public EnemyDeathEvent(GameObject enemy)
        {
            Enemy = enemy;
        }
    }
}

