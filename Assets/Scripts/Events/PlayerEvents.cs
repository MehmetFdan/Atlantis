using UnityEngine;

namespace Events
{
    /// <summary>
    /// Oyuncu saldırı olayı
    /// </summary>
    public class PlayerAttackEvent : IEvent
    {
        /// <summary>
        /// Saldırı tipi (0: Normal, 1: Ağır, 2: Özel)
        /// </summary>
        public int AttackType { get; private set; }
        
        /// <summary>
        /// Saldırı gücü
        /// </summary>
        public float AttackPower { get; private set; }
        
        /// <summary>
        /// Kombo dizisindeki saldırı indeksi (0 tabanlı)
        /// </summary>
        public int ComboIndex { get; set; } = 0;
        
        public PlayerAttackEvent(int attackType = 0, float attackPower = 10f, int comboIndex = 0)
        {
            AttackType = attackType;
            AttackPower = attackPower;
            ComboIndex = comboIndex;
        }
    }
    
    /// <summary>
    /// Oyuncu ölüm olayı
    /// </summary>
    public class PlayerDeathEvent : IEvent
    {
        /// <summary>
        /// Ölüm sebebi (0: Hasar, 1: Düşme, 2: Boğulma...)
        /// </summary>
        public int DeathReason { get; private set; }
        
        /// <summary>
        /// Ölüm pozisyonu
        /// </summary>
        public Vector3 DeathPosition { get; private set; }
        
        public PlayerDeathEvent(int deathReason = 0, Vector3 deathPosition = default)
        {
            DeathReason = deathReason;
            DeathPosition = deathPosition;
        }
    }
} 