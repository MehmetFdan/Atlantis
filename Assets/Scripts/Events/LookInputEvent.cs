using UnityEngine;

namespace Events
{
    public readonly struct LookInputEvent : IEvent
    {
        public Vector2 LookEvent { get; }
        
        public LookInputEvent(Vector2 lookEvent)
        {
            LookEvent = lookEvent;
        }
    }
}
