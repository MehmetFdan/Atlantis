using UnityEngine;

namespace Events
{
    public readonly struct CrouchInputEvent : IEvent
    {
        public bool CrouchPressed { get; }
        
        public CrouchInputEvent(bool crouchPressed)
        {
            CrouchPressed = crouchPressed;
        }
    }
} 