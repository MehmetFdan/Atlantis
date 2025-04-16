using UnityEngine;
using Atlantis.Events;

/// <summary>
/// Düşman ölüm durumu
/// </summary>
public class EnemyDeathState : EnemyBaseState
{
    private float deathTimer;
    private readonly float destroyDelay = 3f;
    
    public EnemyDeathState(EnemyController owner, EnemyStateFactory stateFactory) : base(owner, stateFactory)
    {
    }
    
    public override void Enter()
    {
        // NavMeshAgent'ı deaktifleştir
        owner.NavMeshAgent.isStopped = true;
        owner.NavMeshAgent.enabled = false;
        
        // Collider'ı deaktifleştir
        Collider enemyCollider = owner.GetComponent<Collider>();
        if (enemyCollider != null)
        {
            enemyCollider.enabled = false;
        }
        
        // Ölüm animasyonunu oynat
        owner.Animator?.SetTrigger("Die");
        
        // Ölüm event'ini yayınla
        if (owner.EventBus != null)
        {
            var deathEvent = new EnemyDeathEvent(owner.gameObject);
            owner.EventBus.Publish(deathEvent);
        }
        
        // Sayacı sıfırla
        deathTimer = 0f;
    }
    
    public override void Update()
    {
        // Ölüm sonrası bekle ve yok et
        deathTimer += Time.deltaTime;
        
        if (deathTimer >= destroyDelay)
        {
            GameObject.Destroy(owner.gameObject);
        }
    }
    
    public override void Exit()
    {
        // Bu durumdan çıkış olmayacak
    }
} 