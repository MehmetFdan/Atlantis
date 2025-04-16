using UnityEngine;

namespace Atlantis.Events 
{
    public readonly struct PlayerDeathEvent : IEvent
    {
        /// <summary>
        /// Ölüm sebebi (0: Hasar, 1: Düþme, 2: Boðulma...)
        /// </summary>
        public int DeathReason { get; }

        /// <summary>
        /// Ölüm pozisyonu
        /// </summary>
        public Vector3 DeathPosition { get; }

        public PlayerDeathEvent(int deathReason = 0, Vector3 deathPosition = default)
        {
            DeathReason = deathReason;
            DeathPosition = deathPosition;
        }
    }
}
