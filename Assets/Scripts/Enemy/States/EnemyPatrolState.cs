using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Düşman devriye durumu
/// </summary>
public class EnemyPatrolState : EnemyBaseState
{
    private int currentWaypointIndex;
    private float waypointWaitTimer;
    private readonly float waypointWaitTime = 1f;
    private bool isWaitingAtWaypoint;
    
    public EnemyPatrolState(EnemyController owner, EnemyStateFactory stateFactory) : base(owner, stateFactory)
    {
    }
    
    public override void Enter()
    {
        // NavMeshAgent'ı aktifleştir
        owner.NavMeshAgent.isStopped = false;
        
        // Hızı ayarla
        owner.NavMeshAgent.speed = owner.EnemySettings.PatrolSpeed;
        
        // Animasyonu ayarla
        owner.Animator?.SetBool("IsWalking", true);
        
        // İlk hedef noktasını ayarla
        if (!isWaitingAtWaypoint)
        {
            SetDestinationToNextWaypoint();
        }
    }
    
    public override void Update()
    {
        // Hedef kontrol et
        if (owner.CheckForTarget())
        {
            ChangeState<EnemyChaseState>();
            return;
        }
        
        // Bir noktada bekleme durumunu kontrol et
        if (isWaitingAtWaypoint)
        {
            waypointWaitTimer += Time.deltaTime;
            
            if (waypointWaitTimer >= waypointWaitTime)
            {
                isWaitingAtWaypoint = false;
                SetDestinationToNextWaypoint();
            }
            
            return;
        }
        
        // Hedefe ulaşıp ulaşmadığını kontrol et
        if (!owner.NavMeshAgent.pathPending && owner.NavMeshAgent.remainingDistance < 0.5f)
        {
            // Bir sonraki noktaya geçmeden önce bekle
            isWaitingAtWaypoint = true;
            waypointWaitTimer = 0f;
            
            // Rastgele olarak boşta durmaya geçebilir
            if (Random.value < 0.3f)
            {
                ChangeState<EnemyIdleState>();
                return;
            }
        }
    }
    
    private void SetDestinationToNextWaypoint()
    {
        if (owner.PatrolWaypoints == null || owner.PatrolWaypoints.Length == 0)
        {
            // Waypoint yoksa rastgele bir noktaya git
            SetRandomDestination();
            return;
        }
        
        // Bir sonraki waypoint'e geç
        currentWaypointIndex = (currentWaypointIndex + 1) % owner.PatrolWaypoints.Length;
        owner.NavMeshAgent.SetDestination(owner.PatrolWaypoints[currentWaypointIndex].position);
    }
    
    private void SetRandomDestination()
    {
        // Rastgele bir noktaya git
        Vector3 randomDirection = Random.insideUnitSphere * 10f;
        randomDirection += owner.transform.position;
        
        NavMeshHit navHit;
        if (NavMesh.SamplePosition(randomDirection, out navHit, 10f, NavMesh.AllAreas))
        {
            owner.NavMeshAgent.SetDestination(navHit.position);
        }
    }
    
    public override void Exit()
    {
        // Animasyonu sıfırla
        owner.Animator?.SetBool("IsWalking", false);
    }
} 