namespace Atlantis.Events
{

    public readonly struct DashInputEvent : IEvent
    {
        public bool IsDashPressed { get; }

        public DashInputEvent(bool isDashPressed)
        {
            IsDashPressed = isDashPressed;
        }
    }
}