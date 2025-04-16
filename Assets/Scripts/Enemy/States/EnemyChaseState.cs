using UnityEngine;

/// <summary>
/// Düşman takip durumu
/// </summary>
public class EnemyChaseState : EnemyBaseState
{
    private float targetLostTimer = 0f;
    private Vector3 lastTargetPosition;
    
    public EnemyChaseState(EnemyController owner, EnemyStateFactory stateFactory) : base(owner, stateFactory)
    {
    }
    
    public override void Enter()
    {
        Debug.Log("Enemy entered CHASE state");
        targetLostTimer = 0f;
        
        // Koşma animasyonunu aktifleştir
        owner.Animator?.SetBool("IsChasing", true);
        
        // Hızı ayarla
        owner.NavMeshAgent.speed = owner.EnemySettings.ChaseSpeed;
        
        // Son hedef pozisyonunu kaydet
        if (owner.CurrentTarget != null)
        {
            lastTargetPosition = owner.CurrentTarget.position;
        }
        else if (owner.LastKnownPlayerPosition != Vector3.zero)
        {
            lastTargetPosition = owner.LastKnownPlayerPosition;
        }
        else if (owner.IsRespondingToHelpCall)
        {
            lastTargetPosition = owner.HelpCallPosition;
        }
    }
    
    public override void Exit()
    {
        owner.Animator?.SetBool("IsChasing", false);
    }
    
    public override void Update()
    {
        // Hedefe doğru hareket et
        if (owner.CurrentTarget != null)
        {
            // Hedefi takip et
            owner.NavMeshAgent.SetDestination(owner.CurrentTarget.position);
            lastTargetPosition = owner.CurrentTarget.position;
            targetLostTimer = 0f;
        }
        else
        {
            // Hedef kayboldu, son bilinen pozisyona git
            if (targetLostTimer == 0f && lastTargetPosition != Vector3.zero)
            {
                owner.NavMeshAgent.SetDestination(lastTargetPosition);
            }
            
            targetLostTimer += Time.deltaTime;
            
            // Belirli bir süre sonra hedef bulunamazsa idle durumuna geç
            if (targetLostTimer >= owner.EnemySettings.TargetLostTime)
            {
                // Eğer oyuncu ses çıkarıyorsa veya bir ipucu varsa, araştırma durumuna geç
                if (Time.time - owner.LastHeardSoundTime < 3f)
                {
                    ChangeState<EnemyInvestigateState>();
                }
                else
                {
                    ChangeState<EnemyIdleState>();
                }
                return;
            }
        }
        
        // Hedefe olan mesafeyi kontrol et
        if (owner.CurrentTarget != null)
        {
            float distanceToTarget = Vector3.Distance(owner.transform.position, owner.CurrentTarget.position);
            
            // Hedef menzilinde ise, saldırıya başla
            if (distanceToTarget <= owner.EnemySettings.AttackRange)
            {
                // Yakın dövüş saldırısına geç
                ChangeState<EnemyAttackState>();
            }
            // Hedef menzilli saldırı mesafesinde ve düşman menzilli saldırı yapabiliyorsa
            else if (owner.EnemySettings.CanUseRangedAttack && 
                     distanceToTarget <= owner.EnemySettings.RangedAttackDistance && 
                     distanceToTarget > owner.EnemySettings.AttackRange * 1.5f)
            {
                // Menzilli saldırı mesafesindeyse, menzilli saldırıya geç
                ChangeState<EnemyRangedAttackState>();
            }
        }
        else
        {
            // Hedef pozisyonuna yaklaştık mı?
            if (owner.NavMeshAgent.remainingDistance <= owner.NavMeshAgent.stoppingDistance)
            {
                // Hedef noktaya vardık ama hedef yok, araştırmaya geç
                ChangeState<EnemyInvestigateState>();
            }
        }
        
        // Düşman zayıfladıysa ve kaçabiliyorsa, kaçmayı düşün
        if (owner.EnemySettings.CanFlee && 
            owner.CurrentHealth / owner.EnemySettings.MaxHealth <= owner.EnemySettings.FleeHealthPercentage)
        {
            // Yeterince zayıfladık, kaçma kontrolü yap
            float fleeChance = 1f - (owner.CurrentHealth / owner.EnemySettings.MaxHealth);
            if (Random.value < fleeChance * 0.5f)
            {
                ChangeState<EnemyFleeState>();
            }
        }
    }
    
    public override void FixedUpdate()
    {
        // Düşman özel davranışları
        if (owner.CurrentTarget != null)
        {
            // Eğer grup koordinasyonu aktif ve müttefikler varsa
            if (owner.EnemySettings.CanCoordinateAttacks && owner.HasAlliesInRange)
            {
                // Yakın dövüş menzilindeki düşman sayısını sınırla - etrafta dolaş
                RaycastHit[] hits = Physics.SphereCastAll(owner.CurrentTarget.position, 
                                                         owner.EnemySettings.AttackRange, 
                                                         Vector3.up, 0.1f);
                
                int nearbyEnemies = 0;
                foreach (RaycastHit hit in hits)
                {
                    if (hit.collider.CompareTag("Enemy") && hit.collider.gameObject != owner.gameObject)
                    {
                        nearbyEnemies++;
                    }
                }
                
                // Etrafta çok düşman varsa, biraz uzakta dur ve çember oluştur
                if (nearbyEnemies >= 3)
                {
                    // Hedefe yaklaşma, etrafında dön
                    Vector3 dirToTarget = owner.CurrentTarget.position - owner.transform.position;
                    Vector3 perpendicular = Vector3.Cross(dirToTarget.normalized, Vector3.up);
                    Vector3 circlePoint = owner.CurrentTarget.position + perpendicular * owner.EnemySettings.AttackRange * 1.5f;
                    
                    // Çemberdeki noktaya git
                    owner.NavMeshAgent.SetDestination(circlePoint);
                }
            }
        }
    }
} 