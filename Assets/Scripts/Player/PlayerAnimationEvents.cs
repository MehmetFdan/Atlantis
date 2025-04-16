using UnityEngine;
using Atlantis.Events;

/// <summary>
/// Animator'den gelen animasyon olaylarını dinler ve işler.
/// Bu sınıf animasyon zamanlaması için Animator'e bağlanmalıdır.
/// </summary>
public class PlayerAnimationEvents : MonoBehaviour
{
    [Tooltip("Event Bus referansı")]
    [SerializeField] private EventBus eventBus;
    
    [Tooltip("Oyuncu kontrolcüsü referansı")]
    [SerializeField] private PlayerController playerController;
    
    [Tooltip("Saldırı sesleri (alternatif)")]
    [SerializeField] private AudioClip[] attackSounds;
    
    [Tooltip("Adım sesleri")]
    [SerializeField] private AudioClip[] footstepSounds;
    
    [Header("Efektler")]
    [Tooltip("Saldırı trail efekti")]
    [SerializeField] private GameObject attackTrailPrefab;
    
    [Tooltip("Silah izinin aktif kalma süresi (saniye)")]
    [SerializeField] private float trailDuration = 0.5f;
    
    // Aktif silah izi efekti
    private GameObject activeTrailEffect;
    
    private void Awake()
    {
        // Referansları otomatik bulma
        if (eventBus == null)
        {
            eventBus = Object.FindFirstObjectByType<EventBus>();
        }
        
        if (playerController == null)
        {
            playerController = GetComponent<PlayerController>();
        }
    }
    
    // Animator tarafından tetiklenir - Saldırı başlangıcı
    public void OnAttackStart()
    {
        Debug.Log("Animation Event: Attack Start");
        
        // Saldırı başlatma efekti göster
        ShowAttackTrail();
    }
    
    // Animator tarafından tetiklenir - Saldırı vuruşu anı
    public void OnAttackImpact()
    {
        Debug.Log("Animation Event: Attack Impact");
        
        // EventBus üzerinden saldırı etki olayı gönder
        if (eventBus != null)
        {
            eventBus.Publish(new PlayerAttackEvent());
        }
        
        // Saldırı sesi çal
        PlayAttackSound();
    }
    
    // Animator tarafından tetiklenir - Saldırı bitişi
    public void OnAttackEnd()
    {
        Debug.Log("Animation Event: Attack End");
        
        // Efekti kapat
        HideAttackTrail();
    }
    
    // Animator tarafından tetiklenir - Adım sesi
    public void OnFootstep()
    {
        if (footstepSounds != null && footstepSounds.Length > 0)
        {
            AudioClip footstepSound = footstepSounds[Random.Range(0, footstepSounds.Length)];
            AudioSource.PlayClipAtPoint(footstepSound, transform.position, 0.5f);
        }
        else
        {
            // Ayak sesi olay için AudioManager gibi bir sınıfa erişebilirsiniz
            var audioManager = Object.FindAnyObjectByType<AudioManager>();
            if (audioManager != null)
            {
                audioManager.PlaySound("Footstep", transform.position);
            }
        }
    }
    
    // Silah izi efektini göster
    private void ShowAttackTrail()
    {
        // Eğer halihazırda aktif bir efekt varsa temizle
        HideAttackTrail();
        
        // Yeni efekt oluştur
        if (attackTrailPrefab != null)
        {
            // Silah modelini bul
            Transform weaponTip = FindWeaponTip();
            
            if (weaponTip != null)
            {
                activeTrailEffect = Instantiate(attackTrailPrefab, weaponTip);
                activeTrailEffect.transform.localPosition = Vector3.zero;
                activeTrailEffect.transform.localRotation = Quaternion.identity;
                
                // Belirli bir süre sonra efekti kaldır
                Invoke(nameof(HideAttackTrail), trailDuration);
            }
            else
            {
                // Silah ucu bulunamazsa oyuncu pozisyonunda oluştur
                activeTrailEffect = Instantiate(
                    attackTrailPrefab,
                    transform.position + transform.forward * 1.0f,
                    Quaternion.LookRotation(transform.forward)
                );
                
                // Belirli bir süre sonra efekti kaldır
                Invoke(nameof(HideAttackTrail), trailDuration);
            }
        }
    }
    
    // Silah izi efektini kaldır
    private void HideAttackTrail()
    {
        if (activeTrailEffect != null)
        {
            Destroy(activeTrailEffect);
            activeTrailEffect = null;
        }
        
        // Bekleyen tüm Invoke çağrılarını iptal et
        CancelInvoke(nameof(HideAttackTrail));
    }
    
    // Silah ucu transformunu bul
    private Transform FindWeaponTip()
    {
        if (playerController != null && playerController.EquippedWeaponObject != null)
        {
            // Silah objesinde "Tip" etiketli bir transform ara
            Transform weaponTip = playerController.EquippedWeaponObject.transform.Find("Tip");
            
            if (weaponTip != null)
            {
                return weaponTip;
            }
            else
            {
                // Eğer özel bir uç noktası bulunamazsa silah modelinin kendisini döndür
                return playerController.EquippedWeaponObject.transform;
            }
        }
        
        return null;
    }
    
    // Saldırı sesi çal
    private void PlayAttackSound()
    {
        AudioClip soundToPlay = null;
        
        // Önce silah seslerini kontrol et
        if (playerController != null && playerController.CurrentWeapon != null &&
            playerController.CurrentWeapon.AttackSounds != null && 
            playerController.CurrentWeapon.AttackSounds.Length > 0)
        {
            soundToPlay = playerController.CurrentWeapon.AttackSounds[
                Random.Range(0, playerController.CurrentWeapon.AttackSounds.Length)
            ];
        }
        // Yerel sesleri kontrol et
        else if (attackSounds != null && attackSounds.Length > 0)
        {
            soundToPlay = attackSounds[Random.Range(0, attackSounds.Length)];
        }
        
        // Sesi çal
        if (soundToPlay != null)
        {
            AudioSource.PlayClipAtPoint(soundToPlay, transform.position);
        }
    }
} 