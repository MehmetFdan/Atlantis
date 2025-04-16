using UnityEngine;

/// <summary>
/// Düşmanın şüpheli sesleri veya olayları araştırma durumu
/// </summary>
public class EnemyInvestigateState : EnemyBaseState
{
    private float investigateTimer = 0f;
    private float maxInvestigateTime = 10f;
    private Vector3 investigationPoint;
    private bool hasReachedPoint = false;
    private float waitAtPointTimer = 0f;
    private float lookAroundTime = 3f;
    
    public EnemyInvestigateState(EnemyController owner, EnemyStateFactory stateFactory) : base(owner, stateFactory)
    {
    }
    
    public override void Enter()
    {
        Debug.Log("Enemy entered INVESTIGATE state");
        investigateTimer = 0f;
        waitAtPointTimer = 0f;
        hasReachedPoint = false;
        
        // Investigate animasyonunu aktif et
        owner.Animator?.SetBool("IsInvestigating", true);
        
        // NavMeshAgent hızını ayarla - temkinli hareket
        owner.NavMeshAgent.speed = owner.EnemySettings.MoveSpeed * 0.8f;
        
        // Araştırma noktasını belirle
        DetermineInvestigationPoint();
        
        // Araştırma noktasına git
        owner.NavMeshAgent.SetDestination(investigationPoint);
    }
    
    public override void Exit()
    {
        owner.Animator?.SetBool("IsInvestigating", false);
        owner.NavMeshAgent.speed = owner.EnemySettings.MoveSpeed;
    }
    
    public override void Update()
    {
        // Hedef var mı diye kontrol et
        if (owner.CheckForTarget())
        {
            ChangeState<EnemyChaseState>();
            return;
        }
        
        investigateTimer += Time.deltaTime;
        
        // Araştırma süresi aşıldıysa, normal duruma geri dön
        if (investigateTimer >= maxInvestigateTime)
        {
            ChangeState<EnemyIdleState>();
            return;
        }
        
        // Araştırma noktasına vardık mı?
        float distanceToInvestigationPoint = Vector3.Distance(owner.transform.position, investigationPoint);
        if (!hasReachedPoint && distanceToInvestigationPoint <= owner.NavMeshAgent.stoppingDistance + 0.5f)
        {
            hasReachedPoint = true;
            LookAround();
        }
        
        // Araştırma noktasında bekle ve etrafı incele
        if (hasReachedPoint)
        {
            waitAtPointTimer += Time.deltaTime;
            
            // Etrafa bakınma hareketini yap
            if (waitAtPointTimer <= lookAroundTime)
            {
                // Sinüs fonksiyonu kullanarak sağa sola bakınma hareketi
                float angle = Mathf.Sin(waitAtPointTimer * 2f) * 90f;
                owner.transform.rotation = Quaternion.Euler(0f, owner.transform.eulerAngles.y + angle * Time.deltaTime, 0f);
                
                // "Bakınma" animasyonu
                owner.Animator?.SetFloat("LookAroundBlend", Mathf.Abs(Mathf.Sin(waitAtPointTimer * 2f)));
            }
            else
            {
                // Bekledikten sonra ya yeni bir nokta belirle ya da normal duruma dön
                if (Random.value < 0.3f || waitAtPointTimer >= lookAroundTime + 2f)
                {
                    if (investigateTimer < maxInvestigateTime * 0.7f && Random.value < 0.7f)
                    {
                        // Yeni bir araştırma noktası belirle
                        DetermineInvestigationPoint();
                        owner.NavMeshAgent.SetDestination(investigationPoint);
                        hasReachedPoint = false;
                        waitAtPointTimer = 0f;
                    }
                    else
                    {
                        // Araştırmayı sonlandır ve normal duruma dön
                        ChangeState<EnemyIdleState>();
                    }
                }
            }
        }
    }
    
    public override void FixedUpdate()
    {
        // Araştırma sırasında gerçekleşecek fizik güncellemeleri
    }
    
    /// <summary>
    /// Araştırma noktasını belirler
    /// </summary>
    private void DetermineInvestigationPoint()
    {
        // Son duyulan ses varsa, o noktaya git
        if (Time.time - owner.LastHeardSoundTime < 5f)
        {
            investigationPoint = owner.LastHeardSoundPosition;
        }
        // Son bilinen oyuncu pozisyonu varsa, o noktaya git
        else if (owner.LastKnownPlayerPosition != Vector3.zero)
        {
            investigationPoint = owner.LastKnownPlayerPosition;
        }
        // Hiçbiri yoksa, rastgele bir nokta belirle
        else
        {
            Vector3 randomDirection = Random.insideUnitSphere * 10f;
            randomDirection.y = 0;
            investigationPoint = owner.transform.position + randomDirection;
            
            // NavMesh üzerinde geçerli bir nokta bul
            UnityEngine.AI.NavMeshHit hit;
            if (UnityEngine.AI.NavMesh.SamplePosition(investigationPoint, out hit, 10f, UnityEngine.AI.NavMesh.AllAreas))
            {
                investigationPoint = hit.position;
            }
        }
    }
    
    /// <summary>
    /// Araştırma noktasında etrafa bakınmayı tetikler
    /// </summary>
    private void LookAround()
    {
        owner.NavMeshAgent.isStopped = true;
        owner.Animator?.SetTrigger("LookAround");
    }
} 