using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Atlantis.Events;

/// <summary>
/// Düşman oluşturucu (spawner) sınıfı
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [Tooltip("Düşman prefab'ı")]
    [SerializeField] private GameObject enemyPrefab;
    
    [Tooltip("Oluşturma noktaları")]
    [SerializeField] private Transform[] spawnPoints;
    
    [Tooltip("Maksimum aktif düşman sayısı")]
    [SerializeField] private int maxEnemies = 5;
    
    [Tooltip("Oluşturma aralığı (saniye)")]
    [SerializeField] private float spawnInterval = 5f;
    
    [Tooltip("Oyun başlangıcında otomatik başlat")]
    [SerializeField] private bool autoStart = true;
    
    [Header("Dependencies")]
    [Tooltip("Olayları yönetmek için EventBus")]
    [SerializeField] private EventBus eventBus;
    
    // Private fields
    private List<GameObject> activeEnemies = new List<GameObject>();
    private bool isSpawning = false;
    private Coroutine spawnCoroutine;
    
    private void Start()
    {
        if (autoStart)
        {
            StartSpawning();
        }
        
        // Event'lere abone ol
        if (eventBus != null)
        {
            eventBus.Subscribe<EnemyDeathEvent>(OnEnemyDeath);
        }
    }
    
    private void OnDestroy()
    {
        if (eventBus != null)
        {
            eventBus.Unsubscribe<EnemyDeathEvent>(OnEnemyDeath);
        }
    }
    
    /// <summary>
    /// Düşman oluşturmayı başlatır
    /// </summary>
    public void StartSpawning()
    {
        if (!isSpawning)
        {
            isSpawning = true;
            spawnCoroutine = StartCoroutine(SpawnRoutine());
        }
    }
    
    /// <summary>
    /// Düşman oluşturmayı durdurur
    /// </summary>
    public void StopSpawning()
    {
        if (isSpawning && spawnCoroutine != null)
        {
            isSpawning = false;
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
    }
    
    /// <summary>
    /// Düşman ölüm olayını işler
    /// </summary>
    private void OnEnemyDeath(EnemyDeathEvent eventData)
    {
        // Ölen düşmanı aktif listeden çıkar
        if (eventData.Enemy != null && activeEnemies.Contains(eventData.Enemy))
        {
            activeEnemies.Remove(eventData.Enemy);
        }
    }
    
    /// <summary>
    /// Düşman oluşturma rutini
    /// </summary>
    private IEnumerator SpawnRoutine()
    {
        while (isSpawning)
        {
            // Aktif düşman sayısını kontrol et
            CleanupDestroyedEnemies();
            
            if (activeEnemies.Count < maxEnemies)
            {
                SpawnEnemy();
            }
            
            // Bir sonraki oluşturma için bekle
            yield return new WaitForSeconds(spawnInterval);
        }
    }
    
    /// <summary>
    /// Yeni bir düşman oluşturur
    /// </summary>
    private void SpawnEnemy()
    {
        if (enemyPrefab == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning("Enemy prefab or spawn points are missing!");
            return;
        }
        
        // Rastgele bir oluşturma noktası seç
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        
        // Düşmanı oluştur
        GameObject enemy = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
        
        // EventBus referansını ayarla
        EnemyController enemyController = enemy.GetComponent<EnemyController>();
        if (enemyController != null && eventBus != null)
        {
            // Eğer EnemyController'da publicy bir setter yoksa bu atama yapamazsınız
            // Bu durumda EventBus'ı prefab'da ayarlamanız gerekir
            // enemyController.EventBus = eventBus;
        }
        
        // Aktif düşman listesine ekle
        activeEnemies.Add(enemy);
        
        Debug.Log($"Enemy spawned at {spawnPoint.name}. Active enemies: {activeEnemies.Count}");
    }
    
    /// <summary>
    /// Yok edilmiş düşmanları listeden temizler
    /// </summary>
    private void CleanupDestroyedEnemies()
    {
        activeEnemies.RemoveAll(enemy => enemy == null);
    }
} 