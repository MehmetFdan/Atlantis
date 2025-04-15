using UnityEngine;

/// <summary>
/// Oyuncu saldırı özelliklerini yöneten ScriptableObject.
/// Saldırı menzili, hasarı, açısı gibi ayarları içerir.
/// </summary>
[CreateAssetMenu(fileName = "CombatSettings", menuName = "Game/Player/Combat Settings")]
public class CombatSettings : ScriptableObject
{
    [Header("Temel Saldırı Ayarları")]
    [Tooltip("Temel saldırı hasarı")]
    [SerializeField] private float baseDamage = 10f;
    
    [Tooltip("Saldırı menzili")]
    [SerializeField] private float attackRange = 2f;
    
    [Tooltip("Saldırı açısı (derece)")]
    [Range(0, 180)]
    [SerializeField] private float attackAngle = 60f;
    
    [Header("Zaman Ayarları")]
    [Tooltip("Saldırı süresi")]
    [SerializeField] private float attackDuration = 0.5f;
    
    [Tooltip("Saldırı gecikmesi (hasar uygulanmadan önceki süre)")]
    [SerializeField] private float attackDelay = 0.2f;
    
    [Tooltip("Saldırılar arası minimum bekleme süresi")]
    [SerializeField] private float attackCooldown = 0.1f;
    
    [Header("Kombo Ayarları")]
    [Tooltip("Maksimum kombo sayısı")]
    [SerializeField] private int maxComboCount = 3;
    
    [Tooltip("Kombo zaman penceresi")]
    [SerializeField] private float comboTimeWindow = 0.8f;
    
    [Tooltip("Her kombo için hasar çarpanı")]
    [SerializeField] private float[] comboDamageMultipliers = new float[] { 1.0f, 1.2f, 1.5f };
    
    [Header("Hareket Etkileri")]
    [Tooltip("Saldırı sırasında hareket hızı çarpanı")]
    [Range(0, 1)]
    [SerializeField] private float movementSpeedMultiplier = 0.7f;
    
    [Header("Özel Efektler")]
    [Tooltip("Darbe efekti")]
    [SerializeField] private GameObject hitEffectPrefab;
    
    [Tooltip("Saldırı sesi")]
    [SerializeField] private AudioClip[] attackSounds;
    
    [Tooltip("Vuruş sesi")]
    [SerializeField] private AudioClip[] hitSounds;
    
    [Header("Katman Ayarları")]
    [Tooltip("Hasar verilebilecek katmanlar")]
    [SerializeField] private LayerMask targetLayers;
    
    /// <summary>
    /// Temel saldırı hasarı
    /// </summary>
    public float BaseDamage => baseDamage;
    
    /// <summary>
    /// Saldırı menzili
    /// </summary>
    public float AttackRange => attackRange;
    
    /// <summary>
    /// Saldırı açısı
    /// </summary>
    public float AttackAngle => attackAngle;
    
    /// <summary>
    /// Saldırı süresi
    /// </summary>
    public float AttackDuration => attackDuration;
    
    /// <summary>
    /// Saldırı gecikmesi
    /// </summary>
    public float AttackDelay => attackDelay;
    
    /// <summary>
    /// Saldırılar arası minimum bekleme süresi
    /// </summary>
    public float AttackCooldown => attackCooldown;
    
    /// <summary>
    /// Maksimum kombo sayısı
    /// </summary>
    public int MaxComboCount => maxComboCount;
    
    /// <summary>
    /// Kombo zaman penceresi
    /// </summary>
    public float ComboTimeWindow => comboTimeWindow;
    
    /// <summary>
    /// Kombo hasar çarpanları
    /// </summary>
    public float[] ComboDamageMultipliers => comboDamageMultipliers;
    
    /// <summary>
    /// Saldırı sırasında hareket hızı çarpanı
    /// </summary>
    public float MovementSpeedMultiplier => movementSpeedMultiplier;
    
    /// <summary>
    /// Darbe efekti
    /// </summary>
    public GameObject HitEffectPrefab => hitEffectPrefab;
    
    /// <summary>
    /// Saldırı sesleri
    /// </summary>
    public AudioClip[] AttackSounds => attackSounds;
    
    /// <summary>
    /// Vuruş sesleri
    /// </summary>
    public AudioClip[] HitSounds => hitSounds;
    
    /// <summary>
    /// Hasar verilebilecek katmanlar
    /// </summary>
    public LayerMask TargetLayers => targetLayers;
    
    /// <summary>
    /// Belirli bir kombo indeksi için hasar çarpanını döndürür
    /// </summary>
    /// <param name="comboIndex">Kombo indeksi</param>
    /// <returns>Hasar çarpanı</returns>
    public float GetDamageMultiplier(int comboIndex)
    {
        if (comboIndex < 0 || comboIndex >= comboDamageMultipliers.Length)
        {
            return 1.0f;
        }
        
        return comboDamageMultipliers[comboIndex];
    }
    
    /// <summary>
    /// Belirli bir kombo indeksi için toplam hasarı hesaplar
    /// </summary>
    /// <param name="comboIndex">Kombo indeksi</param>
    /// <returns>Hesaplanan hasar</returns>
    public float CalculateDamage(int comboIndex)
    {
        return BaseDamage * GetDamageMultiplier(comboIndex);
    }
} 