using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Düşmanın kaçma ve saklanma durumu
/// </summary>
public class EnemyFleeState : EnemyBaseState
{
    private float fleeTimer = 0f;
    private float maxFleeTime = 10f;
    private Vector3 fleeDestination;
    private bool hasSetDestination = false;
    private float updatePathTimer = 0f;
    
    public EnemyFleeState(EnemyController owner, EnemyStateFactory stateFactory) : base(owner, stateFactory)
    {
    }
    
    public override void Enter()
    {
        Debug.Log("Enemy entered FLEE state");
        fleeTimer = 0f;
        hasSetDestination = false;
        
        // Flee animasyonunu aktif et
        owner.Animator?.SetBool("IsFleeing", true);
        
        // NavMeshAgent hızını ayarla - kaçarken hızlı hareket et
        owner.NavMeshAgent.speed = owner.EnemySettings.ChaseSpeed * 1.2f;
        owner.NavMeshAgent.angularSpeed = 360f; // Hızlı dönüş
        
        // Saklanma noktası bulundu mu kontrol et
        if (owner.NearestCoverPoint == null)
        {
            owner.FindNearestCoverPoint();
        }
    }
    
    public override void Exit()
    {
        owner.Animator?.SetBool("IsFleeing", false);
        owner.NavMeshAgent.speed = owner.EnemySettings.MoveSpeed;
        owner.NavMeshAgent.angularSpeed = 120f;
    }
    
    public override void Update()
    {
        fleeTimer += Time.deltaTime;
        updatePathTimer += Time.deltaTime;
        
        // Oyuncudan uzakta isek veya maximum kaçma süresi aşıldıysa, durumu değiştir
        if (fleeTimer >= maxFleeTime || IsDistanceToTargetSafe())
        {
            ChangeState<EnemyIdleState>();
            return;
        }
        
        // Saklanma noktası kontrolü
        if (owner.NearestCoverPoint != null && !hasSetDestination)
        {
            // Saklanma noktasına git
            SetCoverDestination();
            hasSetDestination = true;
        }
        else if (!hasSetDestination || updatePathTimer >= 2f)
        {
            // Saklanma noktası yoksa, oyuncudan uzaklaşacak bir yön belirle
            SetFleeDestination();
            hasSetDestination = true;
            updatePathTimer = 0f;
        }
        
        // Hedef noktaya yakın isek ve uzaktaysak idle durumuna geç
        if (owner.NavMeshAgent.remainingDistance <= owner.NavMeshAgent.stoppingDistance && IsDistanceToTargetSafe())
        {
            // Hedef noktaya vardık ve güvendeyiz, idle durumuna geç
            ChangeState<EnemyIdleState>();
        }
    }
    
    public override void FixedUpdate()
    {
        // Oyuncudan kaçarken sürekli etrafı kontrol et
        if (owner.CurrentTarget != null)
        {
            // Oyuncunun pozisyonunun zıt yönüne bak (kaçış hissi için)
            Vector3 targetDir = owner.transform.position - owner.CurrentTarget.position;
            if (targetDir != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(targetDir);
                owner.transform.rotation = Quaternion.Slerp(owner.transform.rotation, lookRotation, Time.deltaTime * owner.EnemySettings.RotationSpeed * 0.5f);
            }
        }
    }
    
    /// <summary>
    /// Saklanma noktasına gitmek için navigasyon yolunu ayarlar
    /// </summary>
    private void SetCoverDestination()
    {
        if (owner.NearestCoverPoint != null)
        {
            fleeDestination = owner.NearestCoverPoint.position;
            owner.NavMeshAgent.SetDestination(fleeDestination);
            Debug.Log("Enemy found cover point at " + fleeDestination);
        }
    }
    
    /// <summary>
    /// Oyuncudan uzaklaşacak bir yön belirler
    /// </summary>
    private void SetFleeDestination()
    {
        Vector3 fleeDirection = Vector3.zero;
        
        if (owner.CurrentTarget != null)
        {
            // Oyuncudan uzaklaşan bir yön
            fleeDirection = (owner.transform.position - owner.CurrentTarget.position).normalized;
        }
        else if (owner.LastKnownPlayerPosition != Vector3.zero)
        {
            // Son bilinen oyuncu pozisyonundan uzaklaş
            fleeDirection = (owner.transform.position - owner.LastKnownPlayerPosition).normalized;
        }
        else
        {
            // Rastgele bir yön
            fleeDirection = Random.insideUnitSphere;
            fleeDirection.y = 0;
            fleeDirection.Normalize();
        }
        
        // Belirli bir mesafe uzağa kaç
        Vector3 targetPosition = owner.transform.position + fleeDirection * 15f;
        
        // NavMesh üzerinde geçerli bir noktaya yönlendir
        NavMeshHit hit;
        if (NavMesh.SamplePosition(targetPosition, out hit, 15f, NavMesh.AllAreas))
        {
            fleeDestination = hit.position;
            owner.NavMeshAgent.SetDestination(fleeDestination);
            Debug.Log("Enemy fleeing to " + fleeDestination);
        }
    }
    
    /// <summary>
    /// Oyuncudan güvenli bir mesafede olup olmadığını kontrol eder
    /// </summary>
    private bool IsDistanceToTargetSafe()
    {
        if (owner.CurrentTarget != null)
        {
            float distanceToTarget = Vector3.Distance(owner.transform.position, owner.CurrentTarget.position);
            return distanceToTarget > owner.EnemySettings.DetectionRange * 0.8f;
        }
        else if (owner.LastKnownPlayerPosition != Vector3.zero)
        {
            float distanceToLastKnown = Vector3.Distance(owner.transform.position, owner.LastKnownPlayerPosition);
            return distanceToLastKnown > owner.EnemySettings.DetectionRange * 0.8f;
        }
        
        return true; // Hedef yoksa güvende kabul et
    }
} 