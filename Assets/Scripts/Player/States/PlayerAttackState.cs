using UnityEngine;
using Events;

public class PlayerAttackState : PlayerBaseState
{
    private float attackTimer = 0f;
    private bool hasAttacked = false;
    private int currentComboIndex = 0;
    private float comboTimer = 0f;
    private bool comboWindowOpen = false;
    private bool isComboRequested = false;
    
    // Saldırı ayarları referansı için kısaltma
    private CombatSettings settings => playerController.CombatSettings;
    
    // Aktif silah referansı
    private WeaponData weapon => playerController.CurrentWeapon;
    
    public PlayerAttackState(PlayerController controller) : base(controller)
    {
    }
    
    public override void Enter()
    {
        base.Enter();
        
        // Saldırı durumuna girdiğimizde timer'ı sıfırla
        attackTimer = 0f;
        hasAttacked = false;
        
        // Yeni bir kombo başlatılıyor mu yoksa mevcut kombo devam mı ediyor kontrol et
        if (comboTimer <= 0f)
        {
            // Yeni kombo başlatılıyor
            currentComboIndex = 0;
            comboWindowOpen = false;
        }
        
        // Konsola saldırı başlangıç mesajı
        Debug.Log($"Saldırı başlatıldı! Kombo: {currentComboIndex + 1}");
        
        // Saldırı animasyonu için event gönder
        if (playerController.EventBus != null)
        {
            PlayerAttackEvent attackEvent = new PlayerAttackEvent
            {
                ComboIndex = currentComboIndex
            };
            playerController.EventBus.Publish(attackEvent);
        }
        
        // Saldırı sesi
        PlayAttackSound();
    }
    
    public override void Update()
    {
        base.Update();
        
        // Saldırı timerını arttır
        attackTimer += Time.deltaTime;
        
        // Kombo timerını yönet
        if (comboTimer > 0f)
        {
            comboTimer -= Time.deltaTime;
            
            // Kombo süresi bittiyse komboyu sıfırla
            if (comboTimer <= 0f && !hasAttacked)
            {
                currentComboIndex = 0;
                comboWindowOpen = false;
            }
        }
        
        // Saldırı butonu izlemesi
        if (playerController.IsAttackPressed && !isComboRequested && comboWindowOpen)
        {
            isComboRequested = true;
            
            // Bir sonraki kombo için hazırlanılıyor
            int maxComboCount = settings != null ? settings.MaxComboCount : 3;
            if (currentComboIndex < maxComboCount - 1)
            {
                Debug.Log($"Kombo sıraya alındı: {currentComboIndex + 2}");
            }
        }
        
        // Saldırı ayarlarını kontrol et
        if (settings != null)
        {
            // Henüz saldırmadıysa ve timer gecikme süresini geçtiyse (animasyon zamanlaması için)
            if (!hasAttacked && attackTimer >= settings.AttackDelay)
            {
                PerformAttack();
                hasAttacked = true;
                
                // Kombo penceresi aç
                float comboWindow = settings.ComboTimeWindow;
                comboWindowOpen = true;
                comboTimer = comboWindow;
            }
            
            // Saldırı süresi bittiyse kontrol et
            if (attackTimer >= settings.AttackDuration)
            {
                if (isComboRequested)
                {
                    // Bir sonraki komboya geç
                    int maxComboCount = settings.MaxComboCount;
                    if (currentComboIndex < maxComboCount - 1)
                    {
                        // Kombo indeksini arttır ve yeni saldırı yap
                        currentComboIndex++;
                        attackTimer = 0f;
                        hasAttacked = false;
                        isComboRequested = false;
                        comboWindowOpen = false;
                        
                        // Yeni kombo için Enter metodunu çağır
                        Enter();
                    }
                    else
                    {
                        // Maksimum kombo sayısına ulaşıldı, durumu sıfırla ve çık
                        EndAttackSequence();
                    }
                }
                else
                {
                    // Kombo talebi yoksa, bir önceki duruma dön
                    EndAttackSequence();
                }
            }
        }
        else
        {
            // CombatSettings yoksa varsayılan değerleri kullan
            if (!hasAttacked && attackTimer >= 0.2f)
            {
                PerformAttack();
                hasAttacked = true;
            }
            
            if (attackTimer >= 0.5f)
            {
                EndAttackSequence();
            }
        }
    }
    
    private void EndAttackSequence()
    {
        // Kombo sıfırla
        currentComboIndex = 0;
        comboWindowOpen = false;
        isComboRequested = false;
        
        // Durumu değiştir
        ReturnToPreviousState();
    }
    
    public override void FixedUpdate()
    {
        base.FixedUpdate();
        
        // Saldırı sırasında hareket kısıtlaması olabilir veya yavaşlatma yapılabilir
        if (playerController.IsMovementPressed)
        {
            // CombatSettings varsa hız çarpanını oradan al
            float speedMultiplier = settings != null ? settings.MovementSpeedMultiplier : 0.7f;
            
            // Saldırı sırasında yavaşlatılmış hareket
            MovePlayer(playerController.WalkSpeed * speedMultiplier);
        }
    }
    
    private void PerformAttack()
    {
        // Konsola saldırı mesajı
        Debug.Log($"Saldırı gerçekleştirildi! Kombo: {currentComboIndex + 1}");
        
        // Karakterin bakış yönünde saldırı yapar
        Vector3 attackDirection = playerController.transform.forward;
        
        // Ayarları al (önce silahtan, yoksa combat settings'ten, yoksa varsayılan değerler)
        float attackRange = GetAttackRange();
        float attackAngle = GetAttackAngle();
        LayerMask attackLayers = GetTargetLayers();
        float damage = CalculateDamage();
        
        // Karakterin ön tarafında bir küresel alan içinde düşmanları kontrol et
        Collider[] hitColliders = Physics.OverlapSphere(
            playerController.transform.position + attackDirection * attackRange / 2f, 
            attackRange, 
            attackLayers
        );
        
        bool hitAny = false;
        
        foreach (var hitCollider in hitColliders)
        {
            // Düşmanın yönünü hesapla
            Vector3 directionToTarget = (hitCollider.transform.position - playerController.transform.position).normalized;
            
            // Saldırı açısı içinde mi kontrol et
            float angle = Vector3.Angle(attackDirection, directionToTarget);
            
            if (angle < attackAngle / 2)
            {
                hitAny = true;
                
                // Hedefin IDamageable interface'ini ara
                IDamageable damageable = hitCollider.GetComponent<IDamageable>();
                
                if (damageable != null)
                {
                    // Hasar ver
                    float finalDamage = damage;
                    
                    // Elemental hasarı ekle (silahta varsa)
                    if (weapon != null)
                    {
                        float elementalDamage = weapon.GetElementalDamage();
                        if (elementalDamage > 0)
                        {
                            finalDamage += elementalDamage;
                            Debug.Log($"Elemental damage ({weapon.GetElementalTypeName()}): {elementalDamage}");
                        }
                    }
                    
                    damageable.TakeDamage(finalDamage);
                    
                    // Etki gösterimi için debug çizgisi
                    Debug.DrawLine(
                        playerController.transform.position, 
                        hitCollider.transform.position, 
                        Color.red, 
                        1.0f
                    );
                    
                    // Vuruş efekti
                    SpawnHitEffect(hitCollider, directionToTarget);
                    
                    // Vuruş sesi
                    PlayHitSound(hitCollider.transform.position);
                }
                else
                {
                    // Debug amaçlı
                    Debug.Log($"Hit {hitCollider.name} but it doesn't implement IDamageable!");
                }
            }
        }
        
        // Hiçbir şeye vurmadıysa bile saldırı efekti göster
        if (!hitAny)
        {
            // Boşa saldırı efekti/sesi
            PlaySwingSound();
        }
        
        // Silah saldırı efektini göster
        SpawnAttackEffect();
    }
    
    // Saldırı menzilini hesapla
    private float GetAttackRange()
    {
        // Önce silahtan kontrol et
        if (weapon != null)
        {
            return weapon.AttackRange;
        }
        
        // Yoksa combat settings'ten al
        if (settings != null)
        {
            return settings.AttackRange;
        }
        
        // Varsayılan değer
        return 2.0f;
    }
    
    // Saldırı açısını hesapla
    private float GetAttackAngle()
    {
        // Önce silahtan kontrol et
        if (weapon != null)
        {
            return weapon.AttackAngle;
        }
        
        // Yoksa combat settings'ten al
        if (settings != null)
        {
            return settings.AttackAngle;
        }
        
        // Varsayılan değer
        return 60f;
    }
    
    // Hedef katmanları al
    private LayerMask GetTargetLayers()
    {
        // Önce silahtan kontrol et
        if (weapon != null && weapon.TargetLayers.value != 0)
        {
            return weapon.TargetLayers;
        }
        
        // Yoksa combat settings'ten al
        if (settings != null && settings.TargetLayers.value != 0)
        {
            return settings.TargetLayers;
        }
        
        // Varsayılan değer
        return LayerMask.GetMask("Enemy", "Destructible");
    }
    
    // Hasarı hesapla
    private float CalculateDamage()
    {
        // Önce silahtan kontrol et
        if (weapon != null)
        {
            return weapon.CalculateDamage(currentComboIndex);
        }
        
        // Yoksa combat settings'ten al
        if (settings != null)
        {
            return settings.CalculateDamage(currentComboIndex);
        }
        
        // Varsayılan değer
        return 10f;
    }
    
    private void SpawnHitEffect(Collider hitCollider, Vector3 directionToTarget)
    {
        GameObject hitEffectPrefab = null;
        
        // Önce silahtan kontrol et
        if (weapon != null && weapon.HitEffectPrefab != null)
        {
            hitEffectPrefab = weapon.HitEffectPrefab;
        }
        // Yoksa combat settings'ten al
        else if (settings != null && settings.HitEffectPrefab != null)
        {
            hitEffectPrefab = settings.HitEffectPrefab;
        }
        
        if (hitEffectPrefab != null)
        {
            // Vuruş noktasını hesapla
            Vector3 hitPoint = hitCollider.ClosestPoint(playerController.transform.position);
            
            // Efekti oluştur
            GameObject hitEffect = Object.Instantiate(
                hitEffectPrefab, 
                hitPoint, 
                Quaternion.LookRotation(directionToTarget)
            );
            
            // Efekti bir süre sonra yok et
            Object.Destroy(hitEffect, 2.0f);
        }
    }
    
    private void SpawnAttackEffect()
    {
        // Silahtan saldırı efekti kontrolü
        if (weapon != null && weapon.AttackEffectPrefab != null)
        {
            Vector3 spawnPosition = playerController.transform.position + playerController.transform.forward * 1.0f;
            Quaternion spawnRotation = Quaternion.LookRotation(playerController.transform.forward);
            
            GameObject attackEffect = Object.Instantiate(
                weapon.AttackEffectPrefab,
                spawnPosition,
                spawnRotation
            );
            
            // Efekti bir süre sonra yok et
            Object.Destroy(attackEffect, 2.0f);
        }
    }
    
    private void PlayHitSound(Vector3 position)
    {
        AudioClip hitSound = null;
        
        // Önce silahtan kontrol et
        if (weapon != null && weapon.HitSounds != null && weapon.HitSounds.Length > 0)
        {
            hitSound = weapon.HitSounds[Random.Range(0, weapon.HitSounds.Length)];
        }
        // Yoksa combat settings'ten al
        else if (settings != null && settings.HitSounds != null && settings.HitSounds.Length > 0)
        {
            hitSound = settings.HitSounds[Random.Range(0, settings.HitSounds.Length)];
        }
        
        // Sesi çal
        if (hitSound != null)
        {
            AudioSource.PlayClipAtPoint(hitSound, position);
        }
    }
    
    private void PlayAttackSound()
    {
        AudioClip attackSound = null;
        
        // Önce silahtan kontrol et
        if (weapon != null && weapon.AttackSounds != null && weapon.AttackSounds.Length > 0)
        {
            // Komboya göre ses seç (eğer yeterli ses varsa)
            int soundIndex = currentComboIndex;
            if (soundIndex >= weapon.AttackSounds.Length)
            {
                soundIndex = Random.Range(0, weapon.AttackSounds.Length);
            }
            
            attackSound = weapon.AttackSounds[soundIndex];
        }
        // Yoksa combat settings'ten al
        else if (settings != null && settings.AttackSounds != null && settings.AttackSounds.Length > 0)
        {
            int soundIndex = currentComboIndex;
            if (soundIndex >= settings.AttackSounds.Length)
            {
                soundIndex = Random.Range(0, settings.AttackSounds.Length);
            }
            
            attackSound = settings.AttackSounds[soundIndex];
        }
        
        // Sesi çal
        if (attackSound != null)
        {
            AudioSource.PlayClipAtPoint(attackSound, playerController.transform.position);
        }
    }
    
    private void PlaySwingSound()
    {
        // Boşa saldırı sesi burada çalınabilir
    }
    
    public override void Exit()
    {
        base.Exit();
        
        // Çıkışta kombo zamanlayıcısını takip etmeye devam et
        if (comboWindowOpen)
        {
            comboTimer = settings != null ? settings.ComboTimeWindow : 0.8f;
        }
    }
    
    private void ReturnToPreviousState()
    {
        // İlgili duruma dön
        if (playerController.IsMovementPressed)
        {
            if (playerController.IsRunPressed)
            {
                playerController.ChangeState<PlayerRunState>();
            }
            else
            {
                playerController.ChangeState<PlayerWalkState>();
            }
        }
        else
        {
            playerController.ChangeState<PlayerIdleState>();
        }
    }
    
    protected override void CheckForStateChange()
    {
        // Saldırı durumundayken, saldırı bitene kadar durum değişikliği olmaz
        // Timer ile kontrol edilir
    }
} 