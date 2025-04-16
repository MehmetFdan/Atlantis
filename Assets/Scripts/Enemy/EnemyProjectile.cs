using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    private float damage;
    private float speed;
    private float lifetime;
    
    private bool appliesStatusEffect;
    private string statusEffectType;
    private float statusEffectDamage;
    private float statusEffectDuration;
    
    private bool hasCollided;
    
    public void Initialize(float damage, float speed, float lifetime, 
                           bool appliesStatusEffect, string statusEffectType, 
                           float statusEffectDamage, float statusEffectDuration)
    {
        this.damage = damage;
        this.speed = speed;
        this.lifetime = lifetime;
        this.appliesStatusEffect = appliesStatusEffect;
        this.statusEffectType = statusEffectType;
        this.statusEffectDamage = statusEffectDamage;
        this.statusEffectDuration = statusEffectDuration;
        
        // Belirli bir süre sonra yok et
        Destroy(gameObject, lifetime);
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (hasCollided) return;
        
        // Çarpışma efekti/sesi burada oynatılabilir
        
        // Zarar ver
        if (other.CompareTag("Player"))
        {
            // IDamageable damageable = other.GetComponent<IDamageable>();
            // if (damageable != null)
            // {
            //     damageable.TakeDamage(damage);
            //     
            //     // Özel durum etkisi
            //     if (appliesStatusEffect)
            //     {
            //         damageable.ApplyStatusEffect(statusEffectType, statusEffectDamage, statusEffectDuration);
            //     }
            // }
            
            Debug.Log($"Projectile hit {other.gameObject.name} for {damage} damage");
        }
        
        hasCollided = true;
        
        // Çarpışma efekti
        // Instantiate(hitEffect, transform.position, Quaternion.identity);
        
        // Yok et
        Destroy(gameObject);
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        if (hasCollided) return;
        
        // Duvara çarptıysa yok et
        if (!collision.gameObject.CompareTag("Player"))
        {
            // Çarpışma efekti
            // Instantiate(hitEffect, transform.position, Quaternion.identity);
            
            hasCollided = true;
            Destroy(gameObject);
        }
    }
} 