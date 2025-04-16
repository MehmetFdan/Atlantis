using UnityEngine;

/// <summary>
/// Düşman menzilli saldırıları için projektil sınıfı
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{
    private Rigidbody rb;
    private float damage;
    private float speed = 20f;
    private float lifeTime = 5f;
    private GameObject owner;
    private Vector3 direction;
    private float timer = 0f;
    private TrailRenderer trail;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        trail = GetComponent<TrailRenderer>();
    }
    
    private void Start()
    {
        // Otomatik imha zamanlayıcısı
        Destroy(gameObject, lifeTime);
    }
    
    private void Update()
    {
        timer += Time.deltaTime;
        
        // Belirli bir süre sonra hızı düşürmeye başla
        if (timer > lifeTime * 0.6f)
        {
            speed = Mathf.Lerp(speed, 5f, Time.deltaTime * 2f);
            rb.linearVelocity = direction * speed;
        }
        
        // Fizik ve grafik efektleri burada güncellenebilir
    }
    
    /// <summary>
    /// Projektili başlatır
    /// </summary>
    /// <param name="newDirection">Hareket yönü</param>
    /// <param name="newDamage">Hasar miktarı</param>
    /// <param name="projectileOwner">Sahibi (kaynağı)</param>
    public void Initialize(Vector3 newDirection, float newDamage, GameObject projectileOwner)
    {
        direction = newDirection.normalized;
        damage = newDamage;
        owner = projectileOwner;
        
        // Fiziksel hareketi başlat
        rb.linearVelocity = direction * speed;
        
        // Dönme efekti
        rb.angularVelocity = new Vector3(Random.Range(-90f, 90f), Random.Range(-90f, 90f), Random.Range(-90f, 90f));
        
        // Yönü belirle
        transform.forward = direction;
        
        // Trail efektini ayarla
        if (trail != null)
        {
            trail.time = 0.2f;
            trail.widthMultiplier = 0.5f;
        }
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        // Kendi sahibine çarpmaması için kontrol
        if (owner != null && collision.gameObject == owner)
        {
            return;
        }
        
        // Player'a isabet kontrolü
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                // Hasarı uygula
                playerHealth.TakeDamage(damage);
                
                // Çarpma efekti
                CreateHitEffect(collision.contacts[0].point, collision.contacts[0].normal);
            }
        }
        else
        {
            // Diğer yüzeylere çarptığında çarpma efekti
            CreateHitEffect(collision.contacts[0].point, collision.contacts[0].normal);
        }
        
        // Çarptıktan sonra yok ol
        Destroy(gameObject);
    }
    
    /// <summary>
    /// Çarpma efekti oluşturur
    /// </summary>
    private void CreateHitEffect(Vector3 position, Vector3 normal)
    {
        // İz bırakma efekti
        GameObject impactEffect = GameObject.Instantiate(
            Resources.Load<GameObject>("Prefabs/ProjectileImpact"),
            position,
            Quaternion.LookRotation(normal));
            
        GameObject.Destroy(impactEffect, 2f);
    }
} 