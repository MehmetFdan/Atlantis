using UnityEngine;

namespace Atlantis.Events 
{
    public readonly struct PlayerDeathEvent : IEvent
    {
        /// <summary>
        /// �l�m sebebi (0: Hasar, 1: D��me, 2: Bo�ulma...)
        /// </summary>
        public int DeathReason { get; }

        /// <summary>
        /// �l�m pozisyonu
        /// </summary>
        public Vector3 DeathPosition { get; }

        public PlayerDeathEvent(int deathReason = 0, Vector3 deathPosition = default)
        {
            DeathReason = deathReason;
            DeathPosition = deathPosition;
        }
    }
}
