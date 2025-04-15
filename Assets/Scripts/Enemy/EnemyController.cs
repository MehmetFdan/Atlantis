using UnityEngine;
using UnityEngine.AI;
using Events;
using System.Collections;

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
    
    [Header("Patrol")]
    [Tooltip("Devriye noktaları (yoksa rastgele hareket eder)")]
    [SerializeField] private Transform[] patrolWaypoints;
    
    // References
    private NavMeshAgent navMeshAgent;
    private Animator animator;
    private Transform currentTarget;
    private float currentHealth;
    
    // State Machine
    private EnemyStateMachine stateMachine;
    private EnemyStateFactory stateFactory;
    
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
    /// Mevcut sağlık
    /// </summary>
    public float CurrentHealth => currentHealth;
    
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
                    
                    // Hedef tespit olayını yayınla
                    if (eventBus != null)
                    {
                        var targetEvent = new EnemyTargetDetectedEvent(currentTarget, distanceToTarget);
                        eventBus.Publish(targetEvent);
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
        
        // Ölüm kontrolü
        if (currentHealth <= 0)
        {
            ChangeState<EnemyDeathState>();
        }
        else
        {
            // Hasar alınca saldıranı takip et
            if (damageEvent.DamageSource != null)
            {
                currentTarget = damageEvent.DamageSource.transform;
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
            float strength = Mathf.Lerp(knockbackForce, 0, timer / duration);
            
            transform.position += direction * strength * Time.deltaTime;
            
            yield return null;
        }
        
        // NavMeshAgent'ı tekrar etkinleştir
        navMeshAgent.enabled = wasNavMeshEnabled;
        
        // Pozisyonu NavMesh üzerine hizala
        if (navMeshAgent.enabled)
        {
            navMeshAgent.Warp(transform.position);
        }
    }
    
    /// <summary>
    /// Hedefin görünür olup olmadığını kontrol eder
    /// </summary>
    private bool IsTargetVisible(Transform target)
    {
        if (target == null) return false;
        
        Vector3 directionToTarget = (target.position - transform.position).normalized;
        float distanceToTarget = Vector3.Distance(transform.position, target.position);
        
        // Mesafe ve açı kontrolü
        if (distanceToTarget <= enemySettings.DetectionRange &&
            Vector3.Angle(transform.forward, directionToTarget) < enemySettings.DetectionAngle / 2)
        {
            // Engel kontrolü
            if (!Physics.Raycast(transform.position, directionToTarget, distanceToTarget, obstacleMask))
            {
                return true;
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// Gizmo çizimi (editör için)
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (enemySettings == null) return;
        
        // Görüş menzili
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, enemySettings.DetectionRange);
        
        // Görüş açısı
        Vector3 viewAngleA = DirectionFromAngle(transform.eulerAngles.y, -enemySettings.DetectionAngle / 2);
        Vector3 viewAngleB = DirectionFromAngle(transform.eulerAngles.y, enemySettings.DetectionAngle / 2);
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + viewAngleA * enemySettings.DetectionRange);
        Gizmos.DrawLine(transform.position, transform.position + viewAngleB * enemySettings.DetectionRange);
        
        // Saldırı menzili
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, enemySettings.AttackRange);
    }
    
    /// <summary>
    /// Açı değerinden yön vektörü hesaplar
    /// </summary>
    private Vector3 DirectionFromAngle(float eulerY, float angleInDegrees)
    {
        angleInDegrees += eulerY;
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }
} 