using UnityEngine;
using Atlantis.Events;

/// <summary>
/// Oyuncu sağlık yönetimi
/// </summary>
public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [Tooltip("Maksimum sağlık değeri")]
    [SerializeField] private float maxHealth = 100f;
    
    [Tooltip("Mevcut sağlık değeri")]
    [SerializeField] private float currentHealth;
    
    [Tooltip("Hasar alırken çalmak için ses efekti")]
    [SerializeField] private AudioClip hitSound;
    
    [Tooltip("Ölüm anında çalmak için ses efekti")]
    [SerializeField] private AudioClip deathSound;
    
    [Tooltip("Hasar alırken ekranda gösterilecek kan efekti")]
    [SerializeField] private GameObject bloodEffect;
    
    [Header("Dependencies")]
    [Tooltip("Olay yöneticisi")]
    [SerializeField] private EventBus eventBus;
    
    // Components
    private AudioSource audioSource;
    
    // Properties
    public float MaxHealth => maxHealth;
    public float CurrentHealth => currentHealth;
    
    private void Awake()
    {
        // Bileşenleri al
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Sağlık değerini ayarla
        currentHealth = maxHealth;
    }
    
    /// <summary>
    /// Hasar alma fonksiyonu
    /// </summary>
    /// <param name="damageAmount">Hasar miktarı</param>
    public void TakeDamage(float damageAmount)
    {
        // Zaten ölüyse işlem yapma
        if (currentHealth <= 0f) return;
        
        // Hasarı uygula
        currentHealth -= damageAmount;
        
        // Hasar efektleri
        if (hitSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(hitSound);
        }
        
        // Kan efekti
        if (bloodEffect != null)
        {
            GameObject effect = Instantiate(bloodEffect, transform.position, Quaternion.identity);
            Destroy(effect, 2f);
        }
        
        // Ölüm kontrolü
        if (currentHealth <= 0f)
        {
            currentHealth = 0f;
            Die();
        }
    }
    
    /// <summary>
    /// İyileşme fonksiyonu
    /// </summary>
    /// <param name="healAmount">İyileşme miktarı</param>
    public void Heal(float healAmount)
    {
        // Zaten ölüyse iyileşme yapma
        if (currentHealth <= 0f) return;
        
        // İyileşmeyi uygula ve maksimumla sınırla
        currentHealth = Mathf.Min(currentHealth + healAmount, maxHealth);
    }
    
    /// <summary>
    /// Ölüm fonksiyonu
    /// </summary>
    private void Die()
    {
        // Ölüm sesi çal
        if (deathSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(deathSound);
        }
        
        // Oyun Sonu Event gönder
        if (eventBus != null)
        {
            var deathEvent = new PlayerDeathEvent(0, transform.position);
            eventBus.Publish(deathEvent);
        }
        
        // Karakteri devre dışı bırak veya oyunu yeniden başlat
        // Bu örnekte basitçe karakter kontrolünü devre dışı bırakıyoruz
        var playerController = GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.enabled = false;
        }
    }
} 