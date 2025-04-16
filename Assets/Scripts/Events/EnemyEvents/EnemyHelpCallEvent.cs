using UnityEngine;

namespace Atlantis.Events 
{
    public readonly struct EnemyHelpCallEvent : IEvent
    {

        public GameObject Caller { get; }
        public Vector3 CallPosition { get; }
        public Transform Target { get; }

        public EnemyHelpCallEvent(GameObject caller, Vector3 callPosition, Transform target)
        {
            Caller = caller;
            CallPosition = callPosition;
            Target = target;
        }
    }
}
