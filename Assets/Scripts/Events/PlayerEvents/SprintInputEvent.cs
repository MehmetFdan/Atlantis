namespace Atlantis.Events
{
    public readonly struct SprintInputEvent : IEvent
    {
        public bool SprintPressed { get; }
        
        public SprintInputEvent(bool sprintPressed)
        {
            SprintPressed = sprintPressed;
        }
    }
} 