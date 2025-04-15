using UnityEngine;

/// <summary>
/// Düşman takip durumu
/// </summary>
public class EnemyChaseState : EnemyBaseState
{
    private float targetLostTimer;
    
    public EnemyChaseState(EnemyController owner, EnemyStateFactory stateFactory) : base(owner, stateFactory)
    {
    }
    
    public override void Enter()
    {
        // NavMeshAgent'ı aktifleştir
        owner.NavMeshAgent.isStopped = false;
        
        // Hızı ayarla
        owner.NavMeshAgent.speed = owner.EnemySettings.ChaseSpeed;
        
        // Takip kaybı sayacını sıfırla
        targetLostTimer = 0f;
        
        // Animasyonu ayarla
        owner.Animator?.SetBool("IsChasing", true);
        
        // Hedefi belirle ve takibe başla
        SetDestinationToTarget();
    }
    
    public override void Update()
    {
        // Eğer hedef yoksa veya ölmüşse
        if (owner.CurrentTarget == null)
        {
            // Takip kaybı sayacını artır
            targetLostTimer += Time.deltaTime;
            
            // Belirli bir süre geçtiyse boşta durumuna geç
            if (targetLostTimer >= owner.EnemySettings.TargetLostTime)
            {
                ChangeState<EnemyIdleState>();
                return;
            }
        }
        else
        {
            // Takip kaybı sayacını sıfırla
            targetLostTimer = 0f;
            
            // Hedefin pozisyonunu güncelle
            SetDestinationToTarget();
            
            // Hedefe olan mesafeyi kontrol et
            float distanceToTarget = Vector3.Distance(owner.transform.position, owner.CurrentTarget.position);
            
            // Saldırı menzilinde mi kontrol et
            if (distanceToTarget <= owner.EnemySettings.AttackRange)
            {
                ChangeState<EnemyAttackState>();
                return;
            }
        }
        
        // Tekrar hedef kontrolü yap
        owner.CheckForTarget();
    }
    
    private void SetDestinationToTarget()
    {
        if (owner.CurrentTarget != null && owner.NavMeshAgent.isActiveAndEnabled)
        {
            owner.NavMeshAgent.SetDestination(owner.CurrentTarget.position);
        }
    }
    
    public override void Exit()
    {
        // Animasyonu sıfırla
        owner.Animator?.SetBool("IsChasing", false);
    }
} 