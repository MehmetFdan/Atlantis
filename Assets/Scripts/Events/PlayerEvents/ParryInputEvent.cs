
namespace Atlantis.Events
{
    public readonly struct ParryInputEvent : IEvent
    {
        public bool IsParryPressed { get; }
        public ParryInputEvent(bool isParryPressed = false)
        {
            IsParryPressed = isParryPressed;
        }
    }
}
