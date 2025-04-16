using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyWeapon", menuName = "Game/Enemy/Enemy Weapon")]
public class EnemyWeapon : ScriptableObject
{
    [Header("Silah Bilgileri")]
    [Tooltip("Silah adı")]
    [SerializeField] private string weaponName;
    
    [Tooltip("Silah tipi")]
    [SerializeField] private WeaponType weaponType;
    
    [Tooltip("Silah modeli")]
    [SerializeField] private GameObject weaponPrefab;
    
    [Header("Saldırı Özellikleri")]
    [Tooltip("Hasar miktarı")]
    [SerializeField] private float damage = 10f;
    
    [Tooltip("Saldırı hızı (saniye)")]
    [SerializeField] private float attackRate = 1.5f;
    
    [Tooltip("Saldırı menzili")]
    [SerializeField] private float attackRange = 2f;
    
    [Tooltip("Kritik vuruş şansı (0-1 arası)")]
    [Range(0, 1)]
    [SerializeField] private float criticalChance = 0.1f;
    
    [Tooltip("Kritik vuruş çarpanı")]
    [SerializeField] private float criticalMultiplier = 2f;
    
    [Header("Menzilli Silah Özellikleri")]
    [Tooltip("Menzilli silah mı?")]
    [SerializeField] private bool isRanged = false;
    
    [Tooltip("Mermi/Ok prefab")]
    [SerializeField] private GameObject projectilePrefab;
    
    [Tooltip("Mermi hızı")]
    [SerializeField] private float projectileSpeed = 20f;
    
    [Tooltip("Mermi uçuş süresi (saniye)")]
    [SerializeField] private float projectileLifetime = 5f;
    
    [Header("Efekt Özellikleri")]
    [Tooltip("Vuruş efekti")]
    [SerializeField] private GameObject hitEffectPrefab;
    
    [Tooltip("Saldırı sesi")]
    [SerializeField] private AudioClip attackSound;
    
    [Tooltip("Vuruş sesi")]
    [SerializeField] private AudioClip hitSound;
    
    [Header("Özel Efektler")]
    [Tooltip("Silahın uyguladığı özel durum")]
    [SerializeField] private bool appliesStatusEffect = false;
    
    [Tooltip("Özel durum türü (Zehir, Yanma, Donma, vb.)")]
    [SerializeField] private string statusEffectType;
    
    [Tooltip("Özel durum hasar miktarı")]
    [SerializeField] private float statusEffectDamage = 2f;
    
    [Tooltip("Özel durum süresi (saniye)")]
    [SerializeField] private float statusEffectDuration = 5f;
    
    // Getter properties
    public string WeaponName => weaponName;
    public WeaponType WeaponType => weaponType;
    public GameObject WeaponPrefab => weaponPrefab;
    public float Damage => damage;
    public float AttackRate => attackRate;
    public float AttackRange => attackRange;
    public float CriticalChance => criticalChance;
    public float CriticalMultiplier => criticalMultiplier;
    public bool IsRanged => isRanged;
    public GameObject ProjectilePrefab => projectilePrefab;
    public float ProjectileSpeed => projectileSpeed;
    public float ProjectileLifetime => projectileLifetime;
    public GameObject HitEffectPrefab => hitEffectPrefab;
    public AudioClip AttackSound => attackSound;
    public AudioClip HitSound => hitSound;
    public bool AppliesStatusEffect => appliesStatusEffect;
    public string StatusEffectType => statusEffectType;
    public float StatusEffectDamage => statusEffectDamage;
    public float StatusEffectDuration => statusEffectDuration;
    
    // Hasar hesaplama metodu
    public float CalculateDamage()
    {
        if (Random.value <= criticalChance)
        {
            return damage * criticalMultiplier;
        }
        return damage;
    }
} 