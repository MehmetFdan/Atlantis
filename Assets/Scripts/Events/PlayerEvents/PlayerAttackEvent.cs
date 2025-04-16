namespace Atlantis.Events
{
    public readonly struct PlayerAttackEvent : IEvent
    {
        /// <summary>
        /// Saldr tipi (0: Normal, 1: Ar, 2:zel)
        /// </summary>
        public int AttackType { get; }
        public float AttackPower { get; }

        /// <summary>
        /// Kombo dizisindeki saldr indeksi (0 tabanl)
        /// </summary>
        public int ComboIndex { get; }

        public PlayerAttackEvent(int attackType = 0, float attackPower = 10f, int comboIndex = 0)
        {
            AttackType = attackType;
            AttackPower = attackPower;
            ComboIndex = comboIndex;
        }
    }
}
