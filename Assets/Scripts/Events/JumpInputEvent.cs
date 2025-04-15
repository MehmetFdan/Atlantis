using UnityEngine;

namespace Events
{
    public readonly struct JumpInputEvent : IEvent
    {
        public bool JumpPressed { get; }
        
        public JumpInputEvent(bool jumpPressed)
        {
            JumpPressed = jumpPressed;
        }
    }
} 