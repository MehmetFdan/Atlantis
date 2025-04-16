using UnityEngine;
using UnityEngine.AI;
using Atlantis.Events;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Düşman kontrolcüsü. Düşmanın durumlarını, hareketlerini ve AI davranışlarını yönetir.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class EnemyController : MonoBehaviour
{
    [Header("Dependencies")]
    [Tooltip("Oyun içi olayları yönetmek için EventBus")]
    [SerializeField] private EventBus eventBus;
    
    [Tooltip("Düşman ayarlarını içeren ScriptableObject")]
    [SerializeField] private EnemySettings enemySettings;
    
    [Header("Detection")]
    [Tooltip("Hedef maskesi (genellikle Player layer)")]
    [SerializeField] private LayerMask targetMask;
    
    [Tooltip("Görüş engelleyici maskesi (duvarlar vb.)")]
    [SerializeField] private LayerMask obstacleMask;
    
    [Tooltip("Diğer düşmanlar için maske")]
    [SerializeField] private LayerMask enemyMask;
    
    [Header("Patrol")]
    [Tooltip("Devriye noktaları (yoksa rastgele hareket eder)")]
    [SerializeField] private Transform[] patrolWaypoints;
    
    [Header("Cover")]
    [Tooltip("Saklanma noktaları (yoksa rastgele bulunur)")]
    [SerializeField] private Transform[] coverPoints;
    
    [Header("Ayarlar")]
    [SerializeField] private EnemyWeapon primaryWeapon;
    [SerializeField] private EnemyWeapon secondaryWeapon;
    [SerializeField] private Transform weaponSocket;
    [SerializeField] private GameObject currentWeaponInstance;

    [Header("Hedef")]
    [SerializeField] private Transform target;
    
    // References
    private NavMeshAgent navMeshAgent;
    private Animator animator;
    private Transform currentTarget;
    private float currentHealth;
    
    // State Machine
    private EnemyStateMachine stateMachine;
    private EnemyStateFactory stateFactory;
    
    // AI Learning
    private Dictionary<string, float> playerActionMemory = new Dictionary<string, float>();
    private Vector3 lastKnownPlayerPosition;
    private Vector3 lastTakenDamageDirection;
    private float lastTakenDamageTime;
    private int consecutivePlayerAttackCount = 0;
    private bool isAdaptingToBehavior = false;
    private Transform nearestCoverPoint;
    
    // Group behavior
    private List<EnemyController> alliesInRange = new List<EnemyController>();
    private bool hasCalledForHelp = false;
    private bool isRespondingToHelpCall = false;
    private Vector3 helpCallPosition;
    
    // Sound detection
    private float lastHeardSoundTime;
    private Vector3 lastHeardSoundPosition;
    private float soundIntensity;
    
    // Durum değişkenleri
    private float lastAttackTime;
    private bool isAttacking;
    private bool isDead;
    
    // Sınıf özellikleri
    private Dictionary<EnemyClass, System.Action> classSpecialAbilities;
    private float specialAbilityCooldown = 15f;
    private float lastSpecialAbilityTime;
    
    // Referanslar
    private EnemyWeapon activeWeapon;
    
    // Properties
    /// <summary>
    /// NavMeshAgent bileşeni
    /// </summary>
    public NavMeshAgent NavMeshAgent => navMeshAgent;
    
    /// <summary>
    /// Animator bileşeni
    /// </summary>
    public Animator Animator => animator;
    
    /// <summary>
    /// Mevcut hedef
    /// </summary>
    public Transform CurrentTarget => currentTarget;
    
    /// <summary>
    /// Düşman ayarları
    /// </summary>
    public EnemySettings EnemySettings => enemySettings;
    
    /// <summary>
    /// EventBus referansı
    /// </summary>
    public EventBus EventBus => eventBus;
    
    /// <summary>
    /// Devriye noktaları
    /// </summary>
    public Transform[] PatrolWaypoints => patrolWaypoints;
    
    /// <summary>
    /// Saklanma noktaları
    /// </summary>
    public Transform[] CoverPoints => coverPoints;
    
    /// <summary>
    /// Mevcut sağlık
    /// </summary>
    public float CurrentHealth => currentHealth;
    
    /// <summary>
    /// Son bilinen oyuncu pozisyonu
    /// </summary>
    public Vector3 LastKnownPlayerPosition => lastKnownPlayerPosition;
    
    /// <summary>
    /// En yakın saklanma noktası
    /// </summary>
    public Transform NearestCoverPoint => nearestCoverPoint;
    
    /// <summary>
    /// Müttefikler menzilde mi?
    /// </summary>
    public bool HasAlliesInRange => alliesInRange.Count > 0;
    
    /// <summary>
    /// Yardım çağrısına yanıt veriyor mu?
    /// </summary>
    public bool IsRespondingToHelpCall => isRespondingToHelpCall;
    
    /// <summary>
    /// Yardım çağrısı pozisyonu
    /// </summary>
    public Vector3 HelpCallPosition => helpCallPosition;
    
    /// <summary>
    /// Son duyulan ses pozisyonu
    /// </summary>
    public Vector3 LastHeardSoundPosition => lastHeardSoundPosition;
    
    /// <summary>
    /// Son ses duyulma zamanı
    /// </summary>
    public float LastHeardSoundTime => lastHeardSoundTime;
    
    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        
        // State machine oluştur
        stateFactory = new EnemyStateFactory(this);
        stateMachine = new EnemyStateMachine(this, stateFactory);
        
        // Sınıf özel yeteneklerini tanımla
        InitializeClassSpecialAbilities();
        
        // Sağlık değerini ayarla
        if (enemySettings != null)
        {
            currentHealth = enemySettings.MaxHealth;
        }
        else
        {
            Debug.LogError("Enemy settings reference is missing in EnemyController!");
            currentHealth = 100f;
        }
        
        // Event'lere abone ol
        if (eventBus != null)
        {
            eventBus.Subscribe<EnemyDamageEvent>(OnDamage);
            eventBus.Subscribe<EnemyHelpCallEvent>(OnHelpCall);
        }
        else
        {
            Debug.LogError("EventBus reference is missing in EnemyController!");
        }
    }
    
    private void Start()
    {
        // Durum makinesini başlat
        stateMachine.Initialize();
        
        // Düşman özelliklerini uygula
        ApplyEnemySettings();
        
        // Başlangıç silahını oluştur
        EquipWeapon(primaryWeapon);
    }
    
    private void Update()
    {
        stateMachine.Update();
        
        // Ses algılama 
        if (enemySettings.HearingRange > 0)
        {
            ListenForSounds();
        }
        
        // Adaptif davranış güncelleme
        if (enemySettings.CanAdaptToPlayerAttacks && isAdaptingToBehavior)
        {
            AdaptToBehavior();
        }
        
        // Menzildeki müttefikleri kontrol et
        if (enemySettings.CanCallForHelp || enemySettings.CanCoordinateAttacks)
        {
            UpdateAlliesInRange();
        }
        
        if (isDead || target == null) return;
        
        // Hedefe doğru hareket et
        MoveToTarget();
        
        // Saldırı kontrol et
        CheckAttackCondition();
        
        // Özel yetenek kullanımını kontrol et
        CheckSpecialAbilityUsage();
    }
    
    private void FixedUpdate()
    {
        stateMachine.FixedUpdate();
    }
    
    private void OnDestroy()
    {
        // Event'lerden aboneliği kaldır
        if (eventBus != null)
        {
            eventBus.Unsubscribe<EnemyDamageEvent>(OnDamage);
            eventBus.Unsubscribe<EnemyHelpCallEvent>(OnHelpCall);
        }
    }
    
    /// <summary>
    /// Düşmanın belirli bir duruma geçmesini sağlar
    /// </summary>
    /// <typeparam name="T">Geçilecek durum tipi</typeparam>
    public void ChangeState<T>() where T : IEnemyState
    {
        stateMachine.ChangeState<T>();
    }
    
    /// <summary>
    /// Hedef tespiti yapar
    /// </summary>
    /// <returns>Hedef tespit edildiyse true</returns>
    public bool CheckForTarget()
    {
        // Eğer halihazırda bir hedef varsa, hala görüş alanında mı kontrol et
        if (currentTarget != null)
        {
            if (IsTargetVisible(currentTarget))
            {
                // Hedef hala görüş alanında, son bilinen konumu güncelle
                lastKnownPlayerPosition = currentTarget.position;
                return true;
            }
            else
            {
                // Hedef görünmüyorsa, hedefi sıfırla
                currentTarget = null;
            }
        }
        
        // Görüş alanındaki hedefleri ara
        Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, enemySettings.DetectionRange, targetMask);
        
        foreach (Collider targetCollider in targetsInViewRadius)
        {
            Transform target = targetCollider.transform;
            Vector3 directionToTarget = (target.position - transform.position).normalized;
            
            // Hedef görüş açısı içinde mi
            if (Vector3.Angle(transform.forward, directionToTarget) < enemySettings.DetectionAngle / 2)
            {
                float distanceToTarget = Vector3.Distance(transform.position, target.position);
                
                // Hedefe olan görüş çizgisi engelleniyor mu
                if (!Physics.Raycast(transform.position, directionToTarget, distanceToTarget, obstacleMask))
                {
                    currentTarget = target;
                    lastKnownPlayerPosition = target.position;
                    
                    // Hedef tespit olayını yayınla
                    if (eventBus != null)
                    {
                        var targetEvent = new EnemyTargetDetectedEvent(currentTarget, distanceToTarget);
                        eventBus.Publish(targetEvent);
                    }
                    
                    // Eğer gruplaşma aktifse, müttefikleri haberdar et
                    if (enemySettings.CanCallForHelp && !hasCalledForHelp && currentTarget != null)
                    {
                        CallForHelp();
                    }
                    
                    return true;
                }
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// Belirtilen hedefe yönelir
    /// </summary>
    /// <param name="targetPosition">Hedef pozisyon</param>
    public void LookAtTarget(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        direction.y = 0f;
        
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * enemySettings.RotationSpeed);
        }
    }
    
    /// <summary>
    /// Hasar alma olayını işler
    /// </summary>
    /// <param name="damageEvent">Hasar olayı</param>
    private void OnDamage(EnemyDamageEvent damageEvent)
    {
        // Sağlığı azalt
        currentHealth -= damageEvent.DamageAmount;
        
        // Hasar alma animasyonu
        animator?.SetTrigger("TakeDamage");
        
        // Hasar yönüne göre hafif geri tepki
        StartCoroutine(KnockbackEffect(damageEvent.DamageDirection.normalized, 0.3f));
        
        // Hasar alma zamanını ve yönünü kaydet
        lastTakenDamageDirection = damageEvent.DamageDirection;
        lastTakenDamageTime = Time.time;
        
        // Oyuncunun davranışını öğrenmeye çalış
        if (enemySettings.CanLearnPlayerPatterns && damageEvent.DamageSource != null)
        {
            LearnPlayerBehavior(damageEvent.DamageSource);
        }
        
        // Ölüm kontrolü
        if (currentHealth <= 0)
        {
            Die();
        }
        // Sağlık belli bir değerin altına düştü ve kaçabiliyorsa
        else if (enemySettings.CanFlee && 
                 currentHealth / enemySettings.MaxHealth <= enemySettings.FleeHealthPercentage)
        {
            // Kaçma durumuna geç
            if (nearestCoverPoint != null)
            {
                ChangeState<EnemyFleeState>();
            }
            else
            {
                // En yakın örtü noktasını bul
                FindNearestCoverPoint();
                
                if (nearestCoverPoint != null)
                {
                    ChangeState<EnemyFleeState>();
                }
                else
                {
                    // Örtü yoksa ve hedefe tek saldırı menzili dışındaysa, menzilden uzaklaşmayı dene
                    ChangeState<EnemyFleeState>();
                }
            }
        }
        else
        {
            // Hasar alınca saldıranı takip et
            if (damageEvent.DamageSource != null)
            {
                currentTarget = damageEvent.DamageSource.transform;
                lastKnownPlayerPosition = currentTarget.position;
                
                // Gruplaşma aktifse, yardım çağır
                if (enemySettings.CanCallForHelp && !hasCalledForHelp)
                {
                    CallForHelp();
                }
                
                ChangeState<EnemyChaseState>();
            }
        }
    }
    
    /// <summary>
    /// Geri tepme efekti
    /// </summary>
    private IEnumerator KnockbackEffect(Vector3 direction, float duration)
    {
        float timer = 0;
        float knockbackForce = 5f;
        
        // NavMeshAgent'ı geçici olarak devre dışı bırak
        bool wasNavMeshEnabled = navMeshAgent.enabled;
        navMeshAgent.enabled = false;
        
        while (timer < duration)
        {
            timer += Time.deltaTime;
            transform.position += direction * knockbackForce * Time.deltaTime;
            yield return null;
        }
        
        // NavMeshAgent'ı tekrar etkinleştir
        navMeshAgent.enabled = wasNavMeshEnabled;
        
        // Pozisyonu doğrula
        if (wasNavMeshEnabled && !navMeshAgent.isOnNavMesh)
        {
            NavMeshHit hit;
            if (NavMesh.SamplePosition(transform.position, out hit, 5f, NavMesh.AllAreas))
            {
                transform.position = hit.position;
            }
        }
    }
    
    /// <summary>
    /// Hedefin görüş alanında olup olmadığını kontrol eder
    /// </summary>
    private bool IsTargetVisible(Transform target)
    {
        if (target == null) return false;
        
        Vector3 directionToTarget = (target.position - transform.position).normalized;
        float distanceToTarget = Vector3.Distance(transform.position, target.position);
        
        // Hedef görüş menzilinde mi
        if (distanceToTarget <= enemySettings.DetectionRange)
        {
            // Hedef görüş açısı içinde mi
            if (Vector3.Angle(transform.forward, directionToTarget) < enemySettings.DetectionAngle / 2)
            {
                // Görüş çizgisi engelleniyor mu
                if (!Physics.Raycast(transform.position, directionToTarget, distanceToTarget, obstacleMask))
                {
                    return true;
                }
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// Çevredeki sesleri dinler
    /// </summary>
    private void ListenForSounds()
    {
        // Burada EventBus üzerinden ses olaylarını dinleyip, gerekli işlemleri yapabiliriz
        // Örnek olarak: Oyuncunun koşma, silah ateşleme vb. seslerini algılama
        
        // Eğer son duyulan sesin üzerinden belirli bir süre geçmediyse ve hedef yoksa
        if (Time.time - lastHeardSoundTime < 5f && currentTarget == null)
        {
            if (stateMachine.CurrentState.GetType() != typeof(EnemyInvestigateState))
            {
                // Ses kaynağını araştırmak için investigate durumuna geç
                ChangeState<EnemyInvestigateState>();
            }
        }
    }
    
    /// <summary>
    /// Bir ses algılandığında çağrılır
    /// </summary>
    public void OnSoundHeard(Vector3 position, float intensity)
    {
        lastHeardSoundPosition = position;
        lastHeardSoundTime = Time.time;
        soundIntensity = intensity;
        
        // Eğer ses yeterince yüksekse ve hedef yoksa, araştır
        if (currentTarget == null && 
            (intensity > 0.7f || 
             Vector3.Distance(transform.position, position) < enemySettings.HearingRange * 0.5f))
        {
            if (stateMachine.CurrentState.GetType() != typeof(EnemyInvestigateState))
            {
                ChangeState<EnemyInvestigateState>();
            }
        }
    }
    
    /// <summary>
    /// Oyuncu davranışlarını öğrenir
    /// </summary>
    private void LearnPlayerBehavior(GameObject player)
    {
        if (player == null) return;
        
        PlayerController playerController = player.GetComponent<PlayerController>();
        if (playerController == null) return;
        
        // Oyuncunun şu anki durumuna göre hafızayı güncelle
        string currentActionKey = "";
        
        // Oyuncu dash yapıyorsa
        if (playerController.IsDashing)
        {
            currentActionKey = "PlayerDash";
            
            // Dash sayacını artır
            if (playerActionMemory.ContainsKey(currentActionKey))
            {
                playerActionMemory[currentActionKey] += 1f * enemySettings.LearningRate;
            }
            else
            {
                playerActionMemory[currentActionKey] = 1f * enemySettings.LearningRate;
            }
            
            // Dash karşı önlem durumuna geç
            if (enemySettings.CanCounterPlayerDash && playerActionMemory[currentActionKey] > 3f)
            {
                isAdaptingToBehavior = true;
            }
        }
        
        // Oyuncu saldırıyorsa
        if (playerController.IsAttackPressed)
        {
            currentActionKey = "PlayerAttack";
            consecutivePlayerAttackCount++;
            
            if (playerActionMemory.ContainsKey(currentActionKey))
            {
                playerActionMemory[currentActionKey] += 1f * enemySettings.LearningRate;
            }
            else
            {
                playerActionMemory[currentActionKey] = 1f * enemySettings.LearningRate;
            }
            
            // Saldırı davranışına uyum sağla
            if (enemySettings.CanAdaptToPlayerAttacks && playerActionMemory[currentActionKey] > 5f)
            {
                isAdaptingToBehavior = true;
            }
        }
        else
        {
            consecutivePlayerAttackCount = 0;
        }
    }
    
    /// <summary>
    /// Oyuncu davranışlarına uyum sağlar
    /// </summary>
    private void AdaptToBehavior()
    {
        // Dash karşı önlemi
        if (enemySettings.CanCounterPlayerDash && playerActionMemory.ContainsKey("PlayerDash") && 
            playerActionMemory["PlayerDash"] > 3f)
        {
            // Oyuncu dash kullanıyorsa, düşman saldırılarını zamanla
            if (currentTarget != null && Vector3.Distance(transform.position, currentTarget.position) < enemySettings.AttackRange * 1.5f)
            {
                // Dash sırasında yan tarafa kaç
                Vector3 dodgeDirection = transform.right * (Random.value > 0.5f ? 1f : -1f);
                navMeshAgent.velocity = dodgeDirection * enemySettings.MoveSpeed * 1.5f;
            }
        }
        
        // Sürekli saldırı karşı önlemi
        if (enemySettings.CanAdaptToPlayerAttacks && playerActionMemory.ContainsKey("PlayerAttack") && 
            playerActionMemory["PlayerAttack"] > 5f)
        {
            // Oyuncu sürekli saldırıyorsa, düşmanın savunma durumunu artır
            if (currentTarget != null && Vector3.Distance(transform.position, currentTarget.position) < enemySettings.AttackRange * 1.2f)
            {
                // Defans animasyonu
                animator?.SetBool("IsDefending", true);
                
                // Savunma süresi bitince normal duruma dön
                if (Random.value < 0.1f)
                {
                    animator?.SetBool("IsDefending", false);
                }
            }
            else
            {
                animator?.SetBool("IsDefending", false);
            }
        }
    }
    
    /// <summary>
    /// Menzildeki müttefikleri günceller
    /// </summary>
    private void UpdateAlliesInRange()
    {
        alliesInRange.Clear();
        
        // Menzildeki diğer düşmanları kontrol et
        Collider[] alliesInRadius = Physics.OverlapSphere(transform.position, enemySettings.HelpCallRange, enemyMask);
        
        foreach (Collider allyCollider in alliesInRadius)
        {
            if (allyCollider.gameObject != gameObject)
            {
                EnemyController allyController = allyCollider.GetComponent<EnemyController>();
                if (allyController != null)
                {
                    alliesInRange.Add(allyController);
                }
            }
        }
    }
    
    /// <summary>
    /// Yardım çağırır
    /// </summary>
    public void CallForHelp()
    {
        if (!enemySettings.CanCallForHelp || hasCalledForHelp || currentTarget == null) return;
        
        hasCalledForHelp = true;
        
        // Yardım çağrısı olayını yayınla
        if (eventBus != null)
        {
            var helpEvent = new EnemyHelpCallEvent(gameObject, transform.position, currentTarget);
            eventBus.Publish(helpEvent);
        }
        
        // Animasyon/ses efekti
        animator?.SetTrigger("CallHelp");
    }
    
    /// <summary>
    /// Yardım çağrısına yanıt verir
    /// </summary>
    private void OnHelpCall(EnemyHelpCallEvent helpEvent)
    {
        if (helpEvent.Caller == gameObject) return;
        
        // Yardım çağrısı menzilinde mi kontrol et
        float distanceToCall = Vector3.Distance(transform.position, helpEvent.CallPosition);
        
        if (distanceToCall <= enemySettings.HelpCallRange)
        {
            // Eğer hedef yoksa ve idle/devriye durumundaysa, yardıma git
            if (currentTarget == null)
            {
                isRespondingToHelpCall = true;
                helpCallPosition = helpEvent.CallPosition;
                currentTarget = helpEvent.Target;
                lastKnownPlayerPosition = helpEvent.Target?.position ?? helpEvent.CallPosition;
                
                ChangeState<EnemyChaseState>();
            }
        }
    }
    
    /// <summary>
    /// En yakın saklanma noktasını bulur
    /// </summary>
    public void FindNearestCoverPoint()
    {
        nearestCoverPoint = null;
        float closestDistance = float.MaxValue;
        
        // Eğer önceden tanımlanmış saklanma noktaları varsa, onları kullan
        if (coverPoints != null && coverPoints.Length > 0)
        {
            foreach (Transform coverPoint in coverPoints)
            {
                if (coverPoint == null) continue;
                
                float distance = Vector3.Distance(transform.position, coverPoint.position);
                
                // Hedef yönünde olmayan bir noktayı tercih et
                if (currentTarget != null)
                {
                    Vector3 dirToCover = (coverPoint.position - transform.position).normalized;
                    Vector3 dirToTarget = (currentTarget.position - transform.position).normalized;
                    
                    // Saklanma noktası ile hedef arasındaki açı büyükse daha iyi
                    float angle = Vector3.Angle(dirToCover, dirToTarget);
                    if (angle > 90f && distance < closestDistance)
                    {
                        closestDistance = distance;
                        nearestCoverPoint = coverPoint;
                    }
                }
                else if (distance < closestDistance)
                {
                    closestDistance = distance;
                    nearestCoverPoint = coverPoint;
                }
            }
        }
        else
        {
            // Rastgele saklanma noktaları bul
            for (int i = 0; i < 8; i++)
            {
                Vector3 randomDirection = Random.insideUnitSphere * 15f;
                randomDirection.y = 0;
                Vector3 randomPoint = transform.position + randomDirection;
                
                NavMeshHit hit;
                if (NavMesh.SamplePosition(randomPoint, out hit, 15f, NavMesh.AllAreas))
                {
                    // Rastgele noktanın hedeften uzak olmasını tercih et
                    if (currentTarget != null)
                    {
                        float distToTarget = Vector3.Distance(hit.position, currentTarget.position);
                        if (distToTarget > 10f)
                        {
                            // Ray ile görünürlük kontrolü
                            if (!Physics.Raycast(hit.position, currentTarget.position - hit.position, distToTarget, obstacleMask))
                            {
                                // Görüş hattında engel var, iyi bir saklanma noktası
                                GameObject tempCoverPoint = new GameObject("TempCoverPoint");
                                tempCoverPoint.transform.position = hit.position;
                                nearestCoverPoint = tempCoverPoint.transform;
                                break;
                            }
                        }
                    }
                }
            }
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        if (enemySettings == null) return;
        
        // Görüş menzilini göster
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, enemySettings.DetectionRange);
        
        // Ses algılama menzilini göster
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, enemySettings.HearingRange);
        
        // Görüş açısını göster
        Vector3 viewAngleA = DirectionFromAngle(transform.eulerAngles.y, -enemySettings.DetectionAngle / 2);
        Vector3 viewAngleB = DirectionFromAngle(transform.eulerAngles.y, enemySettings.DetectionAngle / 2);
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + viewAngleA * enemySettings.DetectionRange);
        Gizmos.DrawLine(transform.position, transform.position + viewAngleB * enemySettings.DetectionRange);
        
        // Son bilinen hedef pozisyonu
        if (lastKnownPlayerPosition != Vector3.zero)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(lastKnownPlayerPosition, 0.5f);
        }
    }
    
    private Vector3 DirectionFromAngle(float eulerY, float angleInDegrees)
    {
        angleInDegrees += eulerY;
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }
    
    private void ApplyEnemySettings()
    {
        // NavMeshAgent ayarları
        navMeshAgent.speed = enemySettings.MoveSpeed;
        navMeshAgent.angularSpeed = enemySettings.RotationSpeed;
        navMeshAgent.stoppingDistance = enemySettings.AttackRange * 0.8f;
        
        // Sağlık değeri
        currentHealth = enemySettings.MaxHealth;
    }
    
    private void MoveToTarget()
    {
        float distanceToTarget = Vector3.Distance(transform.position, target.position);
        
        if (distanceToTarget > activeWeapon.AttackRange)
        {
            navMeshAgent.isStopped = false;
            navMeshAgent.SetDestination(target.position);
            
            // Koşma veya yürüme animasyonu
            bool isChasing = distanceToTarget > enemySettings.DetectionRange * 0.8f;
            navMeshAgent.speed = isChasing ? enemySettings.ChaseSpeed : enemySettings.MoveSpeed;
            
            if (animator != null)
            {
                animator.SetBool("IsMoving", true);
                animator.SetBool("IsRunning", isChasing);
            }
        }
        else
        {
            navMeshAgent.isStopped = true;
            
            if (animator != null)
            {
                animator.SetBool("IsMoving", false);
                animator.SetBool("IsRunning", false);
            }
            
            // Hedefe dön
            Vector3 direction = (target.position - transform.position).normalized;
            if (direction != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * enemySettings.RotationSpeed);
            }
        }
    }
    
    private void CheckAttackCondition()
    {
        if (isAttacking) return;
        
        float distanceToTarget = Vector3.Distance(transform.position, target.position);
        
        if (distanceToTarget <= activeWeapon.AttackRange && Time.time >= lastAttackTime + activeWeapon.AttackRate)
        {
            Attack();
        }
    }
    
    private void Attack()
    {
        isAttacking = true;
        lastAttackTime = Time.time;
        
        // Animasyon ve ses efekti
        if (animator != null)
        {
            string attackTrigger = activeWeapon.IsRanged ? "RangedAttack" : "MeleeAttack";
            animator.SetTrigger(attackTrigger);
        }
        
        if (activeWeapon.AttackSound != null)
        {
            AudioSource.PlayClipAtPoint(activeWeapon.AttackSound, transform.position);
        }
        
        // Saldırı tipine göre hasar ver
        if (activeWeapon.IsRanged)
        {
            StartCoroutine(FireProjectile());
        }
        else
        {
            StartCoroutine(MeleeAttack());
        }
    }
    
    private IEnumerator MeleeAttack()
    {
        // Animasyon süresi için bekleme (yaklaşık 0.5 saniye)
        yield return new WaitForSeconds(0.5f);
        
        // Hedef menzil içinde mi kontrol et
        float distanceToTarget = Vector3.Distance(transform.position, target.position);
        if (distanceToTarget <= activeWeapon.AttackRange)
        {
            ApplyDamage(target.gameObject);
            
            // Vuruş efekti
            if (activeWeapon.HitEffectPrefab != null)
            {
                Instantiate(activeWeapon.HitEffectPrefab, target.position, Quaternion.identity);
            }
            
            // Vuruş sesi
            if (activeWeapon.HitSound != null)
            {
                AudioSource.PlayClipAtPoint(activeWeapon.HitSound, target.position);
            }
        }
        
        // Saldırı tamamlandı
        isAttacking = false;
    }
    
    private IEnumerator FireProjectile()
    {
        // Animasyon süresi için bekleme (yaklaşık 0.3 saniye)
        yield return new WaitForSeconds(0.3f);
        
        if (activeWeapon.ProjectilePrefab != null)
        {
            // Mermi oluştur
            GameObject projectile = Instantiate(
                activeWeapon.ProjectilePrefab, 
                weaponSocket.position, 
                Quaternion.LookRotation(target.position - weaponSocket.position)
            );
            
            // Mermi bileşenini ayarla ve fırlat
            EnemyProjectile projectileComponent = projectile.GetComponent<EnemyProjectile>();
            if (projectileComponent == null)
            {
                projectileComponent = projectile.AddComponent<EnemyProjectile>();
            }
            
            projectileComponent.Initialize(
                activeWeapon.Damage, 
                activeWeapon.ProjectileSpeed, 
                activeWeapon.ProjectileLifetime,
                activeWeapon.AppliesStatusEffect,
                activeWeapon.StatusEffectType,
                activeWeapon.StatusEffectDamage,
                activeWeapon.StatusEffectDuration
            );
            
            // Mermi yönünü ayarla
            Rigidbody projectileRigidbody = projectile.GetComponent<Rigidbody>();
            if (projectileRigidbody != null)
            {
                Vector3 direction = (target.position - weaponSocket.position).normalized;
                projectileRigidbody.linearVelocity = direction * activeWeapon.ProjectileSpeed;
            }
        }
        
        // Saldırı tamamlandı
        isAttacking = false;
    }
    
    private void ApplyDamage(GameObject targetObj)
    {
        // Hedefin sağlık bileşenini bul (örnek)
        // IDamageable damageable = targetObj.GetComponent<IDamageable>();
        // if (damageable != null)
        // {
        //     float damage = activeWeapon.CalculateDamage();
        //     damageable.TakeDamage(damage);
        //     
        //     // Özel durum etkisi uygula
        //     if (activeWeapon.AppliesStatusEffect)
        //     {
        //         damageable.ApplyStatusEffect(
        //             activeWeapon.StatusEffectType,
        //             activeWeapon.StatusEffectDamage,
        //             activeWeapon.StatusEffectDuration
        //         );
        //     }
        // }
        
        // Şimdilik sadece log
        float damage = activeWeapon.CalculateDamage();
        Debug.Log($"{gameObject.name} dealt {damage} damage to {targetObj.name}");
    }
    
    public void EquipWeapon(EnemyWeapon weapon)
    {
        if (weapon == null) return;
        
        // Eğer bu silah tipi düşmanın kullanabileceği silahlar arasında değilse iptal et
        if (!enemySettings.CanUseWeapon(weapon.WeaponType))
        {
            Debug.LogWarning($"{gameObject.name} cannot equip {weapon.WeaponName} (incompatible weapon type)");
            return;
        }
        
        // Mevcut silahı yok et
        if (currentWeaponInstance != null)
        {
            Destroy(currentWeaponInstance);
        }
        
        // Yeni silahı aktifsleştir
        activeWeapon = weapon;
        
        // Silah modelini oluştur
        if (weapon.WeaponPrefab != null && weaponSocket != null)
        {
            currentWeaponInstance = Instantiate(weapon.WeaponPrefab, weaponSocket);
            currentWeaponInstance.transform.localPosition = Vector3.zero;
            currentWeaponInstance.transform.localRotation = Quaternion.identity;
        }
    }
    
    public void SwitchWeapon()
    {
        if (activeWeapon == primaryWeapon)
        {
            EquipWeapon(secondaryWeapon);
        }
        else
        {
            EquipWeapon(primaryWeapon);
        }
    }
    
    public void TakeDamage(float amount)
    {
        if (isDead) return;
        
        currentHealth -= amount;
        
        // Kaçma kontrolü
        if (enemySettings.CanFlee && currentHealth / enemySettings.MaxHealth <= enemySettings.FleeHealthPercentage)
        {
            Flee();
        }
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    private void Die()
    {
        isDead = true;
        navMeshAgent.isStopped = true;
        
        if (animator != null)
        {
            animator.SetTrigger("Die");
        }
        
        // Collider'ı devre dışı bırak
        Collider enemyCollider = GetComponent<Collider>();
        if (enemyCollider != null)
        {
            enemyCollider.enabled = false;
        }
        
        // Belirli bir süre sonra yok et
        Destroy(gameObject, 5f);
    }
    
    private void Flee()
    {
        // Kaçma mantığı
        if (target != null)
        {
            Vector3 directionFromTarget = (transform.position - target.position).normalized;
            Vector3 fleePosition = transform.position + directionFromTarget * 20f;
            
            NavMeshHit hit;
            if (NavMesh.SamplePosition(fleePosition, out hit, 20f, NavMesh.AllAreas))
            {
                navMeshAgent.SetDestination(hit.position);
                navMeshAgent.speed = enemySettings.ChaseSpeed * 1.5f; // Kaçış hızı
            }
        }
    }
    
    // Sınıf özel yeteneklerini başlat
    private void InitializeClassSpecialAbilities()
    {
        classSpecialAbilities = new Dictionary<EnemyClass, System.Action>
        {
            { EnemyClass.Savaşçı, WarriorSpecialAbility },
            { EnemyClass.Okçu, ArcherSpecialAbility },
            { EnemyClass.Büyücü, MageSpecialAbility },
            { EnemyClass.Haydut, RogueSpecialAbility },
            { EnemyClass.Muhafız, GuardSpecialAbility },
            { EnemyClass.Avcı, HunterSpecialAbility },
            { EnemyClass.Yağmacı, MarauderSpecialAbility },
            { EnemyClass.Canavarlar, BeastSpecialAbility }
        };
    }
    
    // Özel yetenek kullanımını kontrol et
    private void CheckSpecialAbilityUsage()
    {
        if (Time.time >= lastSpecialAbilityTime + specialAbilityCooldown)
        {
            // Hedef menzil içinde ve sağlık düşükse özel yetenek kullan
            float healthPercentage = currentHealth / enemySettings.MaxHealth;
            float distanceToTarget = Vector3.Distance(transform.position, target.position);
            
            if (distanceToTarget <= enemySettings.DetectionRange && 
                (healthPercentage < 0.5f || Random.value < 0.2f))
            {
                UseSpecialAbility();
            }
        }
    }
    
    // Özel yetenek kullan
    private void UseSpecialAbility()
    {
        if (classSpecialAbilities.ContainsKey(enemySettings.EnemyClass))
        {
            classSpecialAbilities[enemySettings.EnemyClass].Invoke();
            lastSpecialAbilityTime = Time.time;
        }
    }
    
    #region Sınıf Özel Yetenekleri
    
    // Savaşçı: Güçlü darbe
    private void WarriorSpecialAbility()
    {
        StartCoroutine(PowerStrike());
    }
    
    private IEnumerator PowerStrike()
    {
        Debug.Log($"{gameObject.name} kullanıyor: Güçlü Darbe!");
        
        if (animator != null)
        {
            animator.SetTrigger("SpecialAttack");
        }
        
        yield return new WaitForSeconds(0.7f);
        
        float distanceToTarget = Vector3.Distance(transform.position, target.position);
        if (distanceToTarget <= activeWeapon.AttackRange * 1.5f)
        {
            float powerDamage = activeWeapon.Damage * 2.5f;
            // IDamageable damageable = target.GetComponent<IDamageable>();
            // if (damageable != null)
            // {
            //    damageable.TakeDamage(powerDamage);
            // }
            
            Debug.Log($"{gameObject.name} dealt {powerDamage} special attack damage to {target.name}");
            
            // Vuruş efekti (daha büyük)
            if (activeWeapon.HitEffectPrefab != null)
            {
                GameObject effect = Instantiate(activeWeapon.HitEffectPrefab, target.position, Quaternion.identity);
                effect.transform.localScale *= 2f;
            }
        }
    }
    
    // Okçu: Hızlı atış
    private void ArcherSpecialAbility()
    {
        StartCoroutine(RapidShot());
    }
    
    private IEnumerator RapidShot()
    {
        Debug.Log($"{gameObject.name} kullanıyor: Hızlı Atış!");
        
        int shotCount = 3;
        
        for (int i = 0; i < shotCount; i++)
        {
            if (animator != null)
            {
                animator.SetTrigger("RangedAttack");
            }
            
            yield return new WaitForSeconds(0.2f);
            
            if (activeWeapon.IsRanged && activeWeapon.ProjectilePrefab != null)
            {
                GameObject projectile = Instantiate(
                    activeWeapon.ProjectilePrefab, 
                    weaponSocket.position, 
                    Quaternion.LookRotation(target.position - weaponSocket.position)
                );
                
                EnemyProjectile projectileComponent = projectile.GetComponent<EnemyProjectile>();
                if (projectileComponent == null)
                {
                    projectileComponent = projectile.AddComponent<EnemyProjectile>();
                }
                
                projectileComponent.Initialize(
                    activeWeapon.Damage * 0.7f, 
                    activeWeapon.ProjectileSpeed * 1.5f, 
                    activeWeapon.ProjectileLifetime,
                    activeWeapon.AppliesStatusEffect,
                    activeWeapon.StatusEffectType,
                    activeWeapon.StatusEffectDamage,
                    activeWeapon.StatusEffectDuration
                );
                
                Rigidbody projectileRigidbody = projectile.GetComponent<Rigidbody>();
                if (projectileRigidbody != null)
                {
                    Vector3 direction = (target.position - weaponSocket.position).normalized;
                    
                    // Hafif yayılma ekle
                    direction += new Vector3(
                        Random.Range(-0.1f, 0.1f),
                        Random.Range(-0.05f, 0.05f),
                        Random.Range(-0.1f, 0.1f)
                    );
                    
                    projectileRigidbody.linearVelocity = direction.normalized * activeWeapon.ProjectileSpeed * 1.5f;
                }
            }
            
            yield return new WaitForSeconds(0.2f);
        }
    }
    
    // Büyücü: Büyü patlaması
    private void MageSpecialAbility()
    {
        StartCoroutine(MagicBurst());
    }
    
    private IEnumerator MagicBurst()
    {
        Debug.Log($"{gameObject.name} kullanıyor: Büyü Patlaması!");
        
        if (animator != null)
        {
            animator.SetTrigger("CastSpell");
        }
        
        yield return new WaitForSeconds(0.8f);
        
        // Çevrede patlama efekti oluştur
        float burstRadius = 5f;
        
        // Patlama efekti
        if (activeWeapon.HitEffectPrefab != null)
        {
            GameObject effect = Instantiate(activeWeapon.HitEffectPrefab, transform.position, Quaternion.identity);
            effect.transform.localScale = Vector3.one * burstRadius * 0.5f;
        }
        
        // Çevredeki herkese zarar ver
        Collider[] colliders = Physics.OverlapSphere(transform.position, burstRadius);
        foreach (Collider collider in colliders)
        {
            if (collider.gameObject.CompareTag("Player"))
            {
                float damage = activeWeapon.Damage * 1.8f;
                
                // IDamageable damageable = collider.GetComponent<IDamageable>();
                // if (damageable != null)
                // {
                //     damageable.TakeDamage(damage);
                // }
                
                Debug.Log($"{gameObject.name} dealt {damage} AoE magic damage to {collider.gameObject.name}");
            }
        }
    }
    
    // Haydut: Gizlenme
    private void RogueSpecialAbility()
    {
        StartCoroutine(StealthMode());
    }
    
    private IEnumerator StealthMode()
    {
        Debug.Log($"{gameObject.name} kullanıyor: Gizlenme!");
        
        // Görünürlüğü azalt
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            Material[] materials = renderer.materials;
            for (int i = 0; i < materials.Length; i++)
            {
                Color originalColor = materials[i].color;
                materials[i].color = new Color(originalColor.r, originalColor.g, originalColor.b, 0.3f);
            }
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }
        
        // Hızı artır
        float originalSpeed = navMeshAgent.speed;
        navMeshAgent.speed *= 1.5f;
        
        // 5 saniye gizli kal
        yield return new WaitForSeconds(5f);
        
        // Tekrar görünür ol
        foreach (Renderer renderer in renderers)
        {
            Material[] materials = renderer.materials;
            for (int i = 0; i < materials.Length; i++)
            {
                Color originalColor = materials[i].color;
                materials[i].color = new Color(originalColor.r, originalColor.g, originalColor.b, 1f);
            }
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        }
        
        // Hızı normale döndür
        navMeshAgent.speed = originalSpeed;
    }
    
    // Muhafız: Defans duruşu
    private void GuardSpecialAbility()
    {
        StartCoroutine(DefensiveStance());
    }
    
    private IEnumerator DefensiveStance()
    {
        Debug.Log($"{gameObject.name} kullanıyor: Defans Duruşu!");
        
        if (animator != null)
        {
            animator.SetTrigger("DefensiveStance");
        }
        
        // Hasar direncini artır (takip edilecek bir değişken olarak eklenebilir)
        float defenseBonus = 0.5f; // %50 daha az hasar
        
        // 6 saniye süreyle defans duruşunda kal
        yield return new WaitForSeconds(6f);
        
        // Defans bonusunu kaldır
    }
    
    // Avcı: Tuzak kurma
    private void HunterSpecialAbility()
    {
        StartCoroutine(PlaceTrap());
    }
    
    private IEnumerator PlaceTrap()
    {
        Debug.Log($"{gameObject.name} kullanıyor: Tuzak Kurma!");
        
        if (animator != null)
        {
            animator.SetTrigger("PlaceTrap");
        }
        
        yield return new WaitForSeconds(0.5f);
        
        // Burada bir tuzak prefabı yerleştirme kodu olabilir
        // GameObject trap = Instantiate(trapPrefab, transform.position, Quaternion.identity);
        
        Debug.Log($"{gameObject.name} positioned a trap at {transform.position}");
    }
    
    // Yağmacı: Çılgın saldırı
    private void MarauderSpecialAbility()
    {
        StartCoroutine(BerserkAttack());
    }
    
    private IEnumerator BerserkAttack()
    {
        Debug.Log($"{gameObject.name} kullanıyor: Çılgın Saldırı!");
        
        if (animator != null)
        {
            animator.SetTrigger("Berserk");
        }
        
        // Hızı ve saldırı gücünü artır
        float originalSpeed = navMeshAgent.speed;
        navMeshAgent.speed *= 1.8f;
        
        // 5 saniye çılgın modda kal
        float berserkDuration = 5f;
        float startTime = Time.time;
        
        while (Time.time < startTime + berserkDuration)
        {
            // Daha hızlı ve daha güçlü saldırılar
            float distanceToTarget = Vector3.Distance(transform.position, target.position);
            
            if (distanceToTarget <= activeWeapon.AttackRange && !isAttacking)
            {
                Attack();
                yield return new WaitForSeconds(activeWeapon.AttackRate * 0.5f); // Daha hızlı saldırı
            }
            
            yield return null;
        }
        
        // Normal moda dön
        navMeshAgent.speed = originalSpeed;
    }
    
    // Canavar: Vahşi kükreyiş
    private void BeastSpecialAbility()
    {
        StartCoroutine(WildRoar());
    }
    
    private IEnumerator WildRoar()
    {
        Debug.Log($"{gameObject.name} kullanıyor: Vahşi Kükreyiş!");
        
        if (animator != null)
        {
            animator.SetTrigger("Roar");
        }
        
        // Kükreyiş efekti/sesi
        // AudioSource.PlayClipAtPoint(roarSound, transform.position, 1.0f);
        
        yield return new WaitForSeconds(0.5f);
        
        // Etraftaki düşmanları çağır
        if (enemySettings.CanCallForHelp)
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, enemySettings.HelpCallRange);
            
            foreach (Collider collider in colliders)
            {
                EnemyController ally = collider.GetComponent<EnemyController>();
                
                if (ally != null && ally != this)
                {
                    // Müttefiki hedef almaya yönlendir
                    ally.target = this.target;
                    
                    Debug.Log($"{gameObject.name} called {ally.gameObject.name} for help!");
                }
            }
        }
    }
    
    #endregion
} 