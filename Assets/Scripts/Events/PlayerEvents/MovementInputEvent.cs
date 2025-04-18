using UnityEngine;

namespace Atlantis.Events
{
    public readonly struct MovementInputEvent : IEvent
    {
        public Vector2 MovementEvent { get; }

        public MovementInputEvent(Vector2 moveEvent) 
        {
            MovementEvent = moveEvent;
        }
    }
}
