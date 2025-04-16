using UnityEngine;

namespace Events
{
    /// <summary>
    /// Düşman hasar alma olayı
    /// </summary>
    public class EnemyDamageEvent : IEvent
    {
        /// <summary>
        /// Hasar miktarı
        /// </summary>
        public float DamageAmount { get; private set; }
        
        /// <summary>
        /// Hasarın geldiği yön
        /// </summary>
        public Vector3 DamageDirection { get; private set; }
        
        /// <summary>
        /// Hasarı veren kaynak
        /// </summary>
        public GameObject DamageSource { get; private set; }
        
        public EnemyDamageEvent(float damageAmount, Vector3 damageDirection, GameObject damageSource)
        {
            DamageAmount = damageAmount;
            DamageDirection = damageDirection;
            DamageSource = damageSource;
        }
    }
    
    /// <summary>
    /// Düşman ölüm olayı
    /// </summary>
    public class EnemyDeathEvent : IEvent
    {
        /// <summary>
        /// Ölen düşman
        /// </summary>
        public GameObject Enemy { get; private set; }
        
        public EnemyDeathEvent(GameObject enemy)
        {
            Enemy = enemy;
        }
    }
    
    /// <summary>
    /// Düşman hedef takip olayı
    /// </summary>
    public class EnemyTargetDetectedEvent : IEvent
    {
        /// <summary>
        /// Tespit edilen hedef
        /// </summary>
        public Transform Target { get; private set; }
        
        /// <summary>
        /// Hedefe olan mesafe
        /// </summary>
        public float Distance { get; private set; }
        
        public EnemyTargetDetectedEvent(Transform target, float distance)
        {
            Target = target;
            Distance = distance;
        }
    }
    
    /// <summary>
    /// Düşman saldırı olayı
    /// </summary>
    public class EnemyAttackEvent : IEvent
    {
        /// <summary>
        /// Saldırı hedefi
        /// </summary>
        public Transform Target { get; private set; }
        
        /// <summary>
        /// Saldırı gücü
        /// </summary>
        public float AttackPower { get; private set; }
        
        public EnemyAttackEvent(Transform target, float attackPower)
        {
            Target = target;
            AttackPower = attackPower;
        }
    }
    
    /// <summary>
    /// Düşman menzilli saldırı olayı
    /// </summary>
    public class EnemyRangedAttackEvent : IEvent
    {
        /// <summary>
        /// Saldırı hedefi
        /// </summary>
        public Transform Target { get; private set; }
        
        /// <summary>
        /// Saldırı gücü
        /// </summary>
        public float AttackPower { get; private set; }
        
        /// <summary>
        /// Saldırı yönü
        /// </summary>
        public Vector3 Direction { get; private set; }
        
        public EnemyRangedAttackEvent(Transform target, float attackPower, Vector3 direction)
        {
            Target = target;
            AttackPower = attackPower;
            Direction = direction;
        }
    }
    
    /// <summary>
    /// Düşman yardım çağırma olayı
    /// </summary>
    public class EnemyHelpCallEvent : IEvent
    {
        /// <summary>
        /// Yardım çağıran düşman
        /// </summary>
        public GameObject Caller { get; private set; }
        
        /// <summary>
        /// Yardım çağırma pozisyonu
        /// </summary>
        public Vector3 CallPosition { get; private set; }
        
        /// <summary>
        /// Hedef oyuncu
        /// </summary>
        public Transform Target { get; private set; }
        
        public EnemyHelpCallEvent(GameObject caller, Vector3 callPosition, Transform target)
        {
            Caller = caller;
            CallPosition = callPosition;
            Target = target;
        }
    }
} 