using UnityEngine;
using Atlantis.Events;

/// <summary>
/// Oyuncu saldırılarını görselleştirmek için yardımcı sınıf.
/// </summary>
public class AttackVisualizer : MonoBehaviour
{
    [Header("Dependencies")]
    [Tooltip("Event bus reference")]
    [SerializeField] private EventBus eventBus;
    
    [Header("Visualization")]
    [Tooltip("Saldırı efekti")]
    [SerializeField] private GameObject attackEffectPrefab;
    
    [Tooltip("Saldırı efekti süresi (saniye)")]
    [SerializeField] private float attackEffectDuration = 0.5f;
    
    [Tooltip("Efektin oyuncudan offset mesafesi")]
    [SerializeField] private float attackEffectOffset = 1f;
    
    // Unity olayları
    private void Awake()
    {
        if (eventBus == null)
        {
            Debug.LogError("EventBus reference is missing in AttackVisualizer!");
            return;
        }
        
        // PlayerAttackEvent'e abone ol
        eventBus.Subscribe<PlayerAttackEvent>(OnPlayerAttack);
    }
    
    private void OnDestroy()
    {
        // Aboneliği sonlandır
        if (eventBus != null)
        {
            eventBus.Unsubscribe<PlayerAttackEvent>(OnPlayerAttack);
        }
    }
    
    // Oyuncu saldırı olayı
    private void OnPlayerAttack(PlayerAttackEvent eventData)
    {
        // Saldırı efektini göster
        ShowAttackEffect();
    }
    
    // Saldırı efektini göster
    private void ShowAttackEffect()
    {
        if (attackEffectPrefab != null)
        {
            // Saldırı yönünü hesapla (oyuncunun bakış yönü)
            Vector3 attackDirection = transform.forward;
            
            // Efektin konumunu hesapla
            Vector3 effectPosition = transform.position + attackDirection * attackEffectOffset;
            
            // Efekti oluştur
            GameObject effectInstance = Instantiate(attackEffectPrefab, effectPosition, transform.rotation);
            
            // Efekti belirli süre sonra yok et
            Destroy(effectInstance, attackEffectDuration);
        }
    }
} 