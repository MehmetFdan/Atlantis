using UnityEngine;

namespace Events
{
    public readonly struct InteractInputEvent : IEvent
    {
        public bool InteractPressed { get; }
        
        public InteractInputEvent(bool interactPressed)
        {
            InteractPressed = interactPressed;
        }
    }
} 