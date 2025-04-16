using UnityEngine;
using Atlantis.Events;

/// <summary>
/// Oyuncunun parry (savuşturma) durumunu yöneten sınıf
/// </summary>
public class PlayerParryState : PlayerBaseState
{
    private float parryTimer = 0f;
    private bool parrySuccessful = false;
    private float parryActiveWindow = 0.2f; // Parry aktif pencere süresi
    private float parryRecoveryTime = 0.5f; // Parry sonrası toparlanma süresi
    
    // Saldırı ayarları referansı için kısaltma
    private CombatSettings settings => playerController.CombatSettings;
    
    public PlayerParryState(PlayerController controller) : base(controller)
    {
    }
    
    public override void Enter()
    {
        base.Enter();
        
        // Parry durumuna girince timer'ı sıfırla
        parryTimer = 0f;
        parrySuccessful = false;
        
        // Debug mesajı
        Debug.Log("Parry durumuna girildi!");
        
        // Parry animasyonu için event gönder
        if (playerController.EventBus != null)
        {
            // Burada Parry event'i gönderilebilir
            // playerController.EventBus.Publish(new ParryEvent());
        }
        
        // Parry sesi çal
        PlayParrySound();
    }
    
    public override void Update()
    {
        base.Update();
        
        // Parry timerını arttır
        parryTimer += Time.deltaTime;
        
        // Parry penceresi içindeyse parry başarılı olabilir
        if (parryTimer <= parryActiveWindow)
        {
            // Bu süre içinde gelen saldırıları savuşturabilir
            CheckForParryCollision();
        }
        
        // Toparlanma süresi bittiyse parry durumundan çık
        if (parryTimer >= parryRecoveryTime)
        {
            ReturnToPreviousState();
        }
    }
    
    public override void FixedUpdate()
    {
        base.FixedUpdate();
        
        // Parry sırasında hareket kısıtlaması
        if (playerController.IsMovementPressed)
        {
            // Parry sırasında daha yavaş hareket
            float speedMultiplier = 0.3f;
            MovePlayer(playerController.WalkSpeed * speedMultiplier);
        }
    }
    
    /// <summary>
    /// Parry çarpışma kontrolü
    /// </summary>
    private void CheckForParryCollision()
    {
        // Karakterin önünde parry alanında bir düşman saldırısı var mı kontrol et
        float parryRadius = 1.5f; // Parry menzili
        Vector3 parryDirection = playerController.transform.forward;
        LayerMask enemyLayers = LayerMask.GetMask("Enemy", "EnemyAttack");
        
        Collider[] hitColliders = Physics.OverlapSphere(
            playerController.transform.position + parryDirection * parryRadius / 2f,
            parryRadius,
            enemyLayers
        );
        
        foreach (var hitCollider in hitColliders)
        {
            // Düşman saldırısını tespit et ve parry yap
            // Bu kısımda düşmanın saldırı durumunu kontrol etmek gerekiyor
            Debug.Log("Potansiyel parry hedefi tespit edildi: " + hitCollider.name);
            
            // Düşman saldırı durumunda mı kontrol et (düşman controller'dan)
            // ...
            
            // Eğer parry başarılıysa
            parrySuccessful = true;
            
            // Parry efekti ve sesi
            PlayParrySuccessSound();
            SpawnParryEffect(hitCollider.transform.position);
            
            // Düşmanı geri tepebilir veya dengesiz hale getirebilir
            // ...
            
            // Parry sonrası counter attack fırsatı verebilir
            // ...
        }
    }
    
    /// <summary>
    /// Parry sesi çalma
    /// </summary>
    private void PlayParrySound()
    {
        // Parry başlangıç sesi
        AudioSource audioSource = playerController.GetComponent<AudioSource>();
        if (audioSource != null)
        {
            // audioSource.PlayOneShot(parrySound);
        }
    }
    
    /// <summary>
    /// Başarılı parry sesi çalma
    /// </summary>
    private void PlayParrySuccessSound()
    {
        // Başarılı parry sesi
        AudioSource audioSource = playerController.GetComponent<AudioSource>();
        if (audioSource != null)
        {
            // audioSource.PlayOneShot(parrySuccessSound);
        }
    }
    
    /// <summary>
    /// Parry efekti oluşturma
    /// </summary>
    private void SpawnParryEffect(Vector3 position)
    {
        // Parry efekti prefab'ı
        // GameObject parryEffect = Instantiate(parryEffectPrefab, position, Quaternion.identity);
        // Destroy(parryEffect, 2f);
    }
    
    /// <summary>
    /// Parry durumundan çıkış
    /// </summary>
    public override void Exit()
    {
        base.Exit();
        
        // Debug mesajı
        Debug.Log("Parry durumundan çıkıldı! Başarılı mı: " + parrySuccessful);
    }
    
    /// <summary>
    /// Önceki duruma dönüş
    /// </summary>
    protected void ReturnToPreviousState()
    {
        // Eğer başarılı parry yapıldıysa ve counter attack isteniyorsa saldırı durumuna geç
        if (parrySuccessful && playerController.IsAttackPressed)
        {
            playerController.ChangeState<PlayerAttackState>();
            return;
        }
        
        // Değilse, önceki duruma dön
        if (playerController.IsGrounded)
        {
            // Hareket tuşuna basılıysa
            if (playerController.IsMovementPressed)
            {
                // Koşu tuşuna basılıysa koşma durumuna
                if (playerController.IsRunPressed)
                {
                    playerController.ChangeState<PlayerRunState>();
                }
                // Basılı değilse yürüme durumuna geç
                else
                {
                    playerController.ChangeState<PlayerWalkState>();
                }
            }
            // Hareket yoksa boşta durumuna geç
            else
            {
                playerController.ChangeState<PlayerIdleState>();
            }
        }
        // Yerde değilse düşme durumuna geç
        else
        {
            playerController.ChangeState<PlayerFallState>();
        }
    }
    
    /// <summary>
    /// Durum değişikliği kontrolü
    /// </summary>
    protected override void CheckForStateChange()
    {
        // Parry aktifken başka duruma geçmeyi engelle
        // Toparlanma süresi bitince ReturnToPreviousState metodu ile durumu değiştir
    }
} 