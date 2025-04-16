using UnityEngine;
using Atlantis.Events;

/// <summary>
/// Düşman menzil saldırısı durumu
/// </summary>
public class EnemyRangedAttackState : EnemyBaseState
{
    private float attackTimer = 0f;
    private bool hasAttacked = false;
    
    public EnemyRangedAttackState(EnemyController owner, EnemyStateFactory stateFactory) : base(owner, stateFactory)
    {
    }
    
    public override void Enter()
    {
        Debug.Log("Enemy entered RANGED ATTACK state");
        attackTimer = 0f;
        hasAttacked = false;
        
        // Düşmanı hedefe doğru çevir
        if (owner.CurrentTarget != null)
        {
            owner.LookAtTarget(owner.CurrentTarget.position);
            owner.NavMeshAgent.isStopped = true;
        }
        
        // Ranged saldırı animasyonunu çalıştır (eğer varsa)
        owner.Animator?.SetBool("IsRangedAttacking", true);
    }
    
    public override void Exit()
    {
        owner.Animator?.SetBool("IsRangedAttacking", false);
        owner.NavMeshAgent.isStopped = false;
        attackTimer = 0f;
        hasAttacked = false;
    }
    
    public override void Update()
    {
        // Hedef yok veya hedef ölü ise idle durumuna geri dön
        if (owner.CurrentTarget == null)
        {
            ChangeState<EnemyIdleState>();
            return;
        }
        
        // Mesafeyi kontrol et
        float distanceToTarget = Vector3.Distance(owner.transform.position, owner.CurrentTarget.position);
        
        // Eğer hedef menzilli saldırı mesafesinden uzaksa takip durumuna geç
        if (distanceToTarget > owner.EnemySettings.RangedAttackDistance * 1.1f)
        {
            ChangeState<EnemyChaseState>();
            return;
        }
        
        // Eğer hedef çok yaklaşmışsa, yakın dövüş durumuna geç
        if (distanceToTarget <= owner.EnemySettings.AttackRange)
        {
            ChangeState<EnemyAttackState>();
            return;
        }
        
        // Saldırı animasyonunu zamanla
        attackTimer += Time.deltaTime;
        
        // Saldırı animasyonunu yarıya gelindiğinde hasarı uygula (animasyon senkronizasyonu için)
        if (!hasAttacked && attackTimer >= owner.EnemySettings.RangedAttackRate / 2f)
        {
            PerformRangedAttack();
            hasAttacked = true;
        }
        
        // Saldırı animasyonu bittiğinde durumu güncelle
        if (attackTimer >= owner.EnemySettings.RangedAttackRate)
        {
            // Düşman konumu ve hedef arasındaki pozisyonu güncelle
            owner.LookAtTarget(owner.CurrentTarget.position);
            
            // Saldırı zamanını sıfırla
            attackTimer = 0f;
            hasAttacked = false;
        }
    }
    
    public override void FixedUpdate()
    {
        // Hedefe sürekli bak
        if (owner.CurrentTarget != null)
        {
            owner.LookAtTarget(owner.CurrentTarget.position);
        }
    }
    
    /// <summary>
    /// Menzilli saldırı gerçekleştirir
    /// </summary>
    private void PerformRangedAttack()
    {
        if (owner.CurrentTarget == null) return;
        
        Vector3 targetPosition = owner.CurrentTarget.position;
        Vector3 direction = (targetPosition - owner.transform.position).normalized;
        
        // EventBus üzerinden saldırı olayını yayınla
        if (owner.EventBus != null)
        {
            var attackEvent = new EnemyRangedAttackEvent(owner.CurrentTarget, owner.EnemySettings.RangedAttackPower, direction);
            owner.EventBus.Publish(attackEvent);
        }
        
        // Menzilli saldırı için bir projektil oluştur
        GameObject projectile = GameObject.Instantiate(
            Resources.Load<GameObject>("Prefabs/EnemyProjectile"), 
            owner.transform.position + direction + Vector3.up * 1.5f, 
            Quaternion.LookRotation(direction));
            
        if (projectile != null)
        {
            Projectile projectileComponent = projectile.GetComponent<Projectile>();
            if (projectileComponent != null)
            {
                projectileComponent.Initialize(direction, owner.EnemySettings.RangedAttackPower, owner.gameObject);
            }
            else
            {
                // Projektil component'i yoksa, basit bir hareket ve yok etme işlemi ekle
                Rigidbody rb = projectile.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.linearVelocity = direction * 20f;
                }
                GameObject.Destroy(projectile, 5f);
            }
        }
        
        // Saldırı efekti oluştur
        if (owner.transform.Find("MuzzlePoint") != null)
        {
            // Ateş efekti oluştur
            GameObject muzzleEffect = GameObject.Instantiate(
                Resources.Load<GameObject>("Prefabs/MuzzleFlash"),
                owner.transform.Find("MuzzlePoint").position,
                Quaternion.LookRotation(direction));
                
            GameObject.Destroy(muzzleEffect, 2f);
        }
    }
} 