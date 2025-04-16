using UnityEngine;

namespace Atlantis.Events 
{
    public readonly struct DashEvent : IEvent
    {
        public Vector3 DashDirection { get; }
        public DashEvent(Vector3 dashDirection)
        {
            DashDirection = dashDirection;
        }
    }
}
