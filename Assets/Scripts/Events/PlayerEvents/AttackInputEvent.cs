namespace Atlantis.Events
{
    public readonly struct AttackInputEvent : IEvent
    {
        public bool AttackPressed { get; }

        public AttackInputEvent(bool attackPressed)
        {
            AttackPressed = attackPressed;
        }
    }
}
