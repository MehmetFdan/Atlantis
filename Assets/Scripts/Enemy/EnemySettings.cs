using UnityEngine;

/// <summary>
/// Düşman özellikleri için temel ayarlar
/// </summary>
[CreateAssetMenu(fileName = "NewEnemySettings", menuName = "Game/Enemy Settings")]
public class EnemySettings : ScriptableObject
{
    [Header("Hareket Ayarları")]
    [Tooltip("Normal takip hızı")]
    [SerializeField] private float moveSpeed = 3.5f;
    
    [Tooltip("Hızlı takip hızı")]
    [SerializeField] private float chaseSpeed = 5f;
    
    [Tooltip("Dönüş hızı")]
    [SerializeField] private float rotationSpeed = 5f;
    
    [Tooltip("Devriye hızı")]
    [SerializeField] private float patrolSpeed = 2f;
    
    [Header("Algılama Ayarları")]
    [Tooltip("Görüş menzili")]
    [SerializeField] private float detectionRange = 10f;
    
    [Tooltip("Görüş açısı")]
    [SerializeField] private float detectionAngle = 60f;
    
    [Tooltip("Saldırı menzili")]
    [SerializeField] private float attackRange = 2f;
    
    [Header("Saldırı Ayarları")]
    [Tooltip("Saldırı gücü")]
    [SerializeField] private float attackPower = 10f;
    
    [Tooltip("Saldırı hızı (saniye)")]
    [SerializeField] private float attackRate = 1.5f;
    
    [Tooltip("Takip kaybı süresi")]
    [SerializeField] private float targetLostTime = 5f;
    
    [Header("Sağlık Ayarları")]
    [Tooltip("Maksimum sağlık")]
    [SerializeField] private float maxHealth = 100f;
    
    [Header("Duyma Ayarları")]
    [Tooltip("Ses algılama menzili")]
    [SerializeField] private float hearingRange = 15f;
    
    [Tooltip("Adım sesi duyma menzili")]
    [SerializeField] private float footstepHearingRange = 8f;
    
    [Tooltip("Silah sesi duyma menzili")]
    [SerializeField] private float weaponHearingRange = 25f;
    
    [Header("Taktik Ayarları")]
    [Tooltip("Düşman menzile dayalı saldırı kullanabilir mi?")]
    [SerializeField] private bool canUseRangedAttack = false;
    
    [Tooltip("Menzil saldırısı mesafesi")]
    [SerializeField] private float rangedAttackDistance = 15f;
    
    [Tooltip("Menzil saldırısı gücü")]
    [SerializeField] private float rangedAttackPower = 8f;
    
    [Tooltip("Menzil saldırısı hızı")]
    [SerializeField] private float rangedAttackRate = 3f;
    
    [Tooltip("Düşman çukur/uçurum gibi tehlikeleri fark edebilir mi?")]
    [SerializeField] private bool canDetectHazards = true;
    
    [Tooltip("Düşman kaçıp saklanabilir mi?")]
    [SerializeField] private bool canTakeCover = false;
    
    [Tooltip("Düşman sağlık değeri belli bir oranın altına düştüğünde kaçar mı?")]
    [SerializeField] private bool canFlee = false;
    
    [Tooltip("Kaçış için gerekli minimum sağlık yüzdesi")]
    [SerializeField] private float fleeHealthPercentage = 0.25f;
    
    [Header("Grup Davranışı Ayarları")]
    [Tooltip("Düşman başkalarını yardıma çağırabilir mi?")]
    [SerializeField] private bool canCallForHelp = true;
    
    [Tooltip("Yardım çağrısı menzili")]
    [SerializeField] private float helpCallRange = 20f;
    
    [Tooltip("Düşman görüldüğünde diğer düşmanlarla koordine olabilir mi?")]
    [SerializeField] private bool canCoordinateAttacks = false;
    
    [Header("Adaptif Davranış Ayarları")]
    [Tooltip("Düşman davranışları oyuncu saldırılarına uyum sağlayabilir mi?")]
    [SerializeField] private bool canAdaptToPlayerAttacks = false;
    
    [Tooltip("Düşman dash kullanan oyuncuya karşı önlem alabilir mi?")]
    [SerializeField] private bool canCounterPlayerDash = false;
    
    [Tooltip("Düşman oyuncunun tekrarlanan saldırılarını öğrenebilir mi?")]
    [SerializeField] private bool canLearnPlayerPatterns = false;
    
    [Tooltip("Öğrenme hızı (0-1 arası, 1 = en hızlı)")]
    [SerializeField] private float learningRate = 0.5f;
    
    // Getter properties
    public float MoveSpeed => moveSpeed;
    public float ChaseSpeed => chaseSpeed;
    public float RotationSpeed => rotationSpeed;
    public float PatrolSpeed => patrolSpeed;
    public float DetectionRange => detectionRange;
    public float DetectionAngle => detectionAngle;
    public float AttackRange => attackRange;
    public float AttackPower => attackPower;
    public float AttackRate => attackRate;
    public float TargetLostTime => targetLostTime;
    public float MaxHealth => maxHealth;
    
    // Yeni getter properties
    public float HearingRange => hearingRange;
    public float FootstepHearingRange => footstepHearingRange;
    public float WeaponHearingRange => weaponHearingRange;
    public bool CanUseRangedAttack => canUseRangedAttack;
    public float RangedAttackDistance => rangedAttackDistance;
    public float RangedAttackPower => rangedAttackPower;
    public float RangedAttackRate => rangedAttackRate;
    public bool CanDetectHazards => canDetectHazards;
    public bool CanTakeCover => canTakeCover;
    public bool CanFlee => canFlee;
    public float FleeHealthPercentage => fleeHealthPercentage;
    public bool CanCallForHelp => canCallForHelp;
    public float HelpCallRange => helpCallRange;
    public bool CanCoordinateAttacks => canCoordinateAttacks;
    public bool CanAdaptToPlayerAttacks => canAdaptToPlayerAttacks;
    public bool CanCounterPlayerDash => canCounterPlayerDash;
    public bool CanLearnPlayerPatterns => canLearnPlayerPatterns;
    public float LearningRate => learningRate;
} 