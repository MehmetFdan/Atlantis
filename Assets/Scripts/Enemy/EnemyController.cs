using UnityEngine;
using UnityEngine.AI;
using Events;
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
            ChangeState<EnemyDeathState>();
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
} 