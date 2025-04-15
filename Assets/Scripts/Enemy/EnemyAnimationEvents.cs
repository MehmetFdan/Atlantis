using UnityEngine;
using Events;

/// <summary>
/// Düşman animasyon olayları. Animasyon içindeki belirli karelerde tetiklenecek metodlar.
/// </summary>
public class EnemyAnimationEvents : MonoBehaviour
{
    [Tooltip("Olay yöneticisi referansı")]
    [SerializeField] private EventBus eventBus;
    
    [Tooltip("Düşman kontrolcüsü referansı")]
    private EnemyController enemyController;
    
    private void Awake()
    {
        enemyController = GetComponent<EnemyController>();
    }
    
    /// <summary>
    /// Saldırı animasyonunda hasar verme anında çağrılır
    /// </summary>
    public void OnAttackHit()
    {
        if (enemyController == null || enemyController.CurrentTarget == null) return;
        
        // Event yayınla
        if (eventBus != null && enemyController.EnemySettings != null)
        {
            var attackEvent = new EnemyAttackEvent(enemyController.CurrentTarget, enemyController.EnemySettings.AttackPower);
            eventBus.Publish(attackEvent);
        }
        
        // Hedefe doğrudan hasar vermek için
        float distanceToTarget = Vector3.Distance(transform.position, enemyController.CurrentTarget.position);
        
        if (distanceToTarget <= enemyController.EnemySettings.AttackRange)
        {
            var playerHealth = enemyController.CurrentTarget.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(enemyController.EnemySettings.AttackPower);
            }
        }
    }
    
    /// <summary>
    /// Ayak sesi efekti için animasyonda çağrılır
    /// </summary>
    public void OnFootstep()
    {
        // Ayak sesi çıkar
        // AudioManager.Instance.PlaySound("EnemyFootstep", transform.position);
    }
    
    /// <summary>
    /// Düşman öldüğünde animasyonda çağrılır
    /// </summary>
    public void OnDeathComplete()
    {
        // Ölüm sonrası temizlik işlemleri
        // Örneğin: deneyim, para vs drop etme
    }
} 