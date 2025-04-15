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
} 