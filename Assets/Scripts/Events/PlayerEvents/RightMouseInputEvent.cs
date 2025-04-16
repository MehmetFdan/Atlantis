
namespace Atlantis.Events
{
    public readonly struct RightMouseInputEvent : IEvent
    {
        public bool AimPressed { get; }
        
        public RightMouseInputEvent(bool aimPressed)
        {
            AimPressed = aimPressed;
        }
    }
} 