using UnityEngine;
using Atlantis.Events;

/// <summary>
/// Düşman saldırı durumu
/// </summary>
public class EnemyAttackState : EnemyBaseState
{
    private float attackTimer;
    private bool hasAttacked;
    
    public EnemyAttackState(EnemyController owner, EnemyStateFactory stateFactory) : base(owner, stateFactory)
    {
    }
    
    public override void Enter()
    {
        // NavMeshAgent'ı durdur
        owner.NavMeshAgent.isStopped = true;
        
        // Saldırı sayacını sıfırla
        attackTimer = 0f;
        hasAttacked = false;
        
        // Hedefe doğru dön
        if (owner.CurrentTarget != null)
        {
            owner.LookAtTarget(owner.CurrentTarget.position);
        }
        
        // Animasyonu ayarla
        owner.Animator?.SetBool("IsAttacking", true);
    }
    
    public override void Update()
    {
        // Hedef var mı kontrol et
        if (owner.CurrentTarget == null)
        {
            ChangeState<EnemyIdleState>();
            return;
        }
        
        // Hedefe doğru dön
        owner.LookAtTarget(owner.CurrentTarget.position);
        
        // Hedefe olan mesafeyi kontrol et
        float distanceToTarget = Vector3.Distance(owner.transform.position, owner.CurrentTarget.position);
        
        // Saldırı menzilinden çıktıysa takip durumuna geç
        if (distanceToTarget > owner.EnemySettings.AttackRange * 1.1f)
        {
            ChangeState<EnemyChaseState>();
            return;
        }
        
        // Saldırı sayacını artır
        attackTimer += Time.deltaTime;
        
        // Saldırı gerçekleşmemişse ve yarı zamana ulaşıldıysa
        if (!hasAttacked && attackTimer >= owner.EnemySettings.AttackRate / 2f)
        {
            PerformAttack();
            hasAttacked = true;
        }
        
        // Saldırı hızına göre süre dolduysa sıfırla
        if (attackTimer >= owner.EnemySettings.AttackRate)
        {
            // Animasyonu tekrar tetikle
            owner.Animator?.SetBool("IsAttacking", false);
            owner.Animator?.SetBool("IsAttacking", true);
            
            // Sayacı ve durumu sıfırla
            attackTimer = 0f;
            hasAttacked = false;
        }
    }
    
    private void PerformAttack()
    {
        if (owner.CurrentTarget == null) return;
        
        // Event ile saldırı bilgisini yayınla
        if (owner.EventBus != null)
        {
            var attackEvent = new EnemyAttackEvent(owner.CurrentTarget, owner.EnemySettings.AttackPower);
            owner.EventBus.Publish(attackEvent);
        }
        
        // Burada doğrudan hasar vermek için RaycastHit veya OverlapSphere kullanılabilir
        // Bu örnekte basitçe mesafe kontrolü yapıyoruz
        float distanceToTarget = Vector3.Distance(owner.transform.position, owner.CurrentTarget.position);
        
        if (distanceToTarget <= owner.EnemySettings.AttackRange)
        {
            // Hedefe hasar vermek için Health/PlayerHealth component'ini bul
            var playerHealth = owner.CurrentTarget.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(owner.EnemySettings.AttackPower);
            }
        }
    }
    
    public override void Exit()
    {
        // Animasyonu sıfırla
        owner.Animator?.SetBool("IsAttacking", false);
    }
} 