using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ClassAbilities", menuName = "Game/Enemy/Class Abilities")]
public class EnemyClassAbilities : ScriptableObject
{
    [System.Serializable]
    public class ClassAbility
    {
        [Tooltip("Yetenek adı")]
        public string abilityName;
        
        [Tooltip("Yetenek açıklaması")]
        [TextArea(2, 5)]
        public string description;
        
        [Tooltip("Yetenek bekleme süresi (saniye)")]
        public float cooldown = 15f;
        
        [Tooltip("Yetenek efekt prefabı")]
        public GameObject effectPrefab;
        
        [Tooltip("Yetenek ses efekti")]
        public AudioClip soundEffect;
        
        [Tooltip("Gereken minimum beceri seviyesi")]
        [Range(1, 10)]
        public int requiredSkillLevel = 1;
    }
    
    [System.Serializable]
    public class ClassAbilitySet
    {
        [Tooltip("Karakter sınıfı")]
        public EnemyClass enemyClass;
        
        [Tooltip("Sınıfa özgü yetenekler")]
        public List<ClassAbility> abilities = new List<ClassAbility>();
    }
    
    [Header("Sınıf Yetenekleri")]
    [SerializeField] private List<ClassAbilitySet> classAbilitySets = new List<ClassAbilitySet>();
    
    // Sınıfa göre yetenek kümesi getir
    public List<ClassAbility> GetAbilitiesForClass(EnemyClass enemyClass)
    {
        foreach (ClassAbilitySet abilitySet in classAbilitySets)
        {
            if (abilitySet.enemyClass == enemyClass)
            {
                return abilitySet.abilities;
            }
        }
        
        return new List<ClassAbility>();
    }
    
    // Belirli bir beceri seviyesine göre erişilebilir yetenekleri getir
    public List<ClassAbility> GetAvailableAbilities(EnemyClass enemyClass, int skillLevel)
    {
        List<ClassAbility> allAbilities = GetAbilitiesForClass(enemyClass);
        List<ClassAbility> availableAbilities = new List<ClassAbility>();
        
        foreach (ClassAbility ability in allAbilities)
        {
            if (ability.requiredSkillLevel <= skillLevel)
            {
                availableAbilities.Add(ability);
            }
        }
        
        return availableAbilities;
    }
    
    // Yetenek desteği kontrol et
    public bool HasAbility(EnemyClass enemyClass, string abilityName)
    {
        List<ClassAbility> abilities = GetAbilitiesForClass(enemyClass);
        
        foreach (ClassAbility ability in abilities)
        {
            if (ability.abilityName == abilityName)
            {
                return true;
            }
        }
        
        return false;
    }
    
    // Varsayılan yetenek kümelerini oluştur
    private void OnEnable()
    {
        if (classAbilitySets == null || classAbilitySets.Count == 0)
        {
            InitializeDefaultAbilities();
        }
    }
    
    // Varsayılan yetenek kümelerini tanımla
    private void InitializeDefaultAbilities()
    {
        classAbilitySets = new List<ClassAbilitySet>();
        
        // Savaşçı yetenekleri
        ClassAbilitySet warriorSet = new ClassAbilitySet { enemyClass = EnemyClass.Savaşçı };
        warriorSet.abilities.Add(new ClassAbility 
        { 
            abilityName = "Güçlü Darbe", 
            description = "Güçlü bir darbe vurarak normal hasarın 2,5 katını verir.",
            cooldown = 15f,
            requiredSkillLevel = 1
        });
        warriorSet.abilities.Add(new ClassAbility 
        { 
            abilityName = "Savunma Duruşu", 
            description = "Kısa bir süre için savunmayı artırır ve gelen hasarı azaltır.",
            cooldown = 20f,
            requiredSkillLevel = 3
        });
        warriorSet.abilities.Add(new ClassAbility 
        { 
            abilityName = "Savaş Narası", 
            description = "Çevredeki müttefiklere güç verir ve düşmanların moralini düşürür.",
            cooldown = 30f,
            requiredSkillLevel = 6
        });
        classAbilitySets.Add(warriorSet);
        
        // Okçu yetenekleri
        ClassAbilitySet archerSet = new ClassAbilitySet { enemyClass = EnemyClass.Okçu };
        archerSet.abilities.Add(new ClassAbility 
        { 
            abilityName = "Hızlı Atış", 
            description = "Kısa sürede birden fazla ok atışı yapar.",
            cooldown = 12f,
            requiredSkillLevel = 1
        });
        archerSet.abilities.Add(new ClassAbility 
        { 
            abilityName = "Zehirli Ok", 
            description = "Hedefin zamanla hasar almasını sağlayan zehir etkisi uygular.",
            cooldown = 25f,
            requiredSkillLevel = 4
        });
        archerSet.abilities.Add(new ClassAbility 
        { 
            abilityName = "Delici Atış", 
            description = "Savunmayı delen güçlü bir ok atar.",
            cooldown = 20f,
            requiredSkillLevel = 7
        });
        classAbilitySets.Add(archerSet);
        
        // Büyücü yetenekleri
        ClassAbilitySet mageSet = new ClassAbilitySet { enemyClass = EnemyClass.Büyücü };
        mageSet.abilities.Add(new ClassAbility 
        { 
            abilityName = "Büyü Patlaması", 
            description = "Çevreye yayılan bir enerji patlaması yaratır.",
            cooldown = 18f,
            requiredSkillLevel = 1
        });
        mageSet.abilities.Add(new ClassAbility 
        { 
            abilityName = "Ateş Duvarı", 
            description = "Geçilmesi zor bir ateş duvarı oluşturur.",
            cooldown = 25f,
            requiredSkillLevel = 4
        });
        mageSet.abilities.Add(new ClassAbility 
        { 
            abilityName = "Klon Yaratma", 
            description = "Kısa süreliğine yanıltıcı bir klon oluşturur.",
            cooldown = 35f,
            requiredSkillLevel = 8
        });
        classAbilitySets.Add(mageSet);
        
        // Diğer sınıflar için benzer şekilde yetenekler tanımlanabilir
        // ...
    }
} 