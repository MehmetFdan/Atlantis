using UnityEngine;

/// <summary>
/// Düşman boşta durma durumu
/// </summary>
public class EnemyIdleState : EnemyBaseState
{
    private float idleTimer;
    private float idleDuration;
    
    public EnemyIdleState(EnemyController owner, EnemyStateFactory stateFactory) : base(owner, stateFactory)
    {
    }
    
    public override void Enter()
    {
        // NavMeshAgent'ı durdur
        owner.NavMeshAgent.isStopped = true;
        
        // Rastgele bir süre belirle
        idleDuration = Random.Range(2f, 5f);
        idleTimer = 0f;
        
        // Animasyonu ayarla
        owner.Animator?.SetBool("IsIdle", true);
    }
    
    public override void Update()
    {
        // Hedef kontrol et
        if (owner.CheckForTarget())
        {
            ChangeState<EnemyChaseState>();
            return;
        }
        
        // Boşta durma süresini artır
        idleTimer += Time.deltaTime;
        
        // Süre dolduysa devriye durumuna geç
        if (idleTimer >= idleDuration)
        {
            ChangeState<EnemyPatrolState>();
        }
    }
    
    public override void Exit()
    {
        // Animasyonu sıfırla
        owner.Animator?.SetBool("IsIdle", false);
    }
} 