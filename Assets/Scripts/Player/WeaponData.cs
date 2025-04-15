using UnityEngine;

/// <summary>
/// Silah özelliklerini yöneten ScriptableObject.
/// Silah tipleri, hasarları, efektleri gibi ayarları içerir.
/// </summary>
[CreateAssetMenu(fileName = "WeaponData", menuName = "Game/Player/Weapon Data")]
public class WeaponData : ScriptableObject
{
    public enum WeaponType
    {
        Fist,       // Yumruk
        Sword,      // Kılıç
        Axe,        // Balta
        Hammer,     // Çekiç
        Spear,      // Mızrak
        Dagger,     // Hançer
        Staff,      // Asa
        Bow,        // Yay
        Wand,       // Değnek
        Custom      // Özel
    }
    
    [Header("Temel Bilgiler")]
    [Tooltip("Silahın ismi")]
    [SerializeField] private string weaponName = "Basic Sword";
    
    [Tooltip("Silahın tipi")]
    [SerializeField] private WeaponType weaponType = WeaponType.Sword;
    
    [Tooltip("Silahın açıklaması")]
    [SerializeField, TextArea(3, 5)] private string description = "A basic sword.";
    
    [Header("Silah Modeli")]
    [Tooltip("Silah prefab modeli")]
    [SerializeField] private GameObject weaponPrefab;
    
    [Tooltip("Silahın tutulduğu el (0: Sağ, 1: Sol, 2: Her iki el)")]
    [Range(0, 2)]
    [SerializeField] private int handSlot = 0;
    
    [Header("Saldırı Özellikleri")]
    [Tooltip("Temel hasar miktarı")]
    [SerializeField] private float baseDamage = 10f;
    
    [Tooltip("Saldırı hızı çarpanı (1.0 = normal hız)")]
    [Range(0.5f, 2.0f)]
    [SerializeField] private float attackSpeedMultiplier = 1.0f;
    
    [Tooltip("Saldırı menzili")]
    [SerializeField] private float attackRange = 2.0f;
    
    [Tooltip("Saldırı açısı (derece)")]
    [Range(0, 180)]
    [SerializeField] private float attackAngle = 60f;
    
    [Header("Kombo Ayarları")]
    [Tooltip("Her kombo için hasar çarpanı")]
    [SerializeField] private float[] comboDamageMultipliers = new float[] { 1.0f, 1.2f, 1.5f };
    
    [Header("Özel Efektler")]
    [Tooltip("Saldırı efekti")]
    [SerializeField] private GameObject attackEffectPrefab;
    
    [Tooltip("Darbe efekti")]
    [SerializeField] private GameObject hitEffectPrefab;
    
    [Tooltip("Saldırı sesleri")]
    [SerializeField] private AudioClip[] attackSounds;
    
    [Tooltip("Vuruş sesleri")]
    [SerializeField] private AudioClip[] hitSounds;
    
    [Tooltip("Hasar verilebilecek katmanlar")]
    [SerializeField] private LayerMask targetLayers;
    
    [Header("Özel Silah Efektleri")]
    [Tooltip("Elemental hasar tipi (0: Yok, 1: Ateş, 2: Buz, 3: Elektrik, 4: Zehir)")]
    [Range(0, 4)]
    [SerializeField] private int elementalType = 0;
    
    [Tooltip("Elemental hasar miktarı")]
    [SerializeField] private float elementalDamage = 0f;
    
    [Tooltip("Kritik vuruş şansı (0-1 arası)")]
    [Range(0, 1)]
    [SerializeField] private float criticalChance = 0.05f;
    
    [Tooltip("Kritik vuruş hasarı çarpanı")]
    [Range(1, 5)]
    [SerializeField] private float criticalDamageMultiplier = 2.0f;
    
    // Getter metotları
    public string WeaponName => weaponName;
    public WeaponType Type => weaponType;
    public string Description => description;
    public GameObject WeaponPrefab => weaponPrefab;
    public int HandSlot => handSlot;
    public float BaseDamage => baseDamage;
    public float AttackSpeedMultiplier => attackSpeedMultiplier;
    public float AttackRange => attackRange;
    public float AttackAngle => attackAngle;
    public float[] ComboDamageMultipliers => comboDamageMultipliers;
    public GameObject AttackEffectPrefab => attackEffectPrefab;
    public GameObject HitEffectPrefab => hitEffectPrefab;
    public AudioClip[] AttackSounds => attackSounds;
    public AudioClip[] HitSounds => hitSounds;
    public LayerMask TargetLayers => targetLayers;
    public int ElementalType => elementalType;
    public float ElementalDamage => elementalDamage;
    public float CriticalChance => criticalChance;
    public float CriticalDamageMultiplier => criticalDamageMultiplier;
    
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
        float damage = BaseDamage * GetDamageMultiplier(comboIndex);
        
        // Kritik vuruş şansı
        if (Random.value < CriticalChance)
        {
            damage *= CriticalDamageMultiplier;
            Debug.Log($"Critical Hit! Damage: {damage}");
        }
        
        return damage;
    }
    
    /// <summary>
    /// Toplam elemental hasarı döndürür
    /// </summary>
    /// <returns>Elemental hasar</returns>
    public float GetElementalDamage()
    {
        return elementalType > 0 ? ElementalDamage : 0f;
    }
    
    /// <summary>
    /// Elemental hasar tipini string olarak döndürür
    /// </summary>
    public string GetElementalTypeName()
    {
        switch (ElementalType)
        {
            case 1:
                return "Fire";
            case 2:
                return "Ice";
            case 3:
                return "Electric";
            case 4:
                return "Poison";
            default:
                return "None";
        }
    }
} 