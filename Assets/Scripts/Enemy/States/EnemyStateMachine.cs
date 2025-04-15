using UnityEngine;

/// <summary>
/// Düşman durum makinesi
/// </summary>
public class EnemyStateMachine
{
    /// <summary>
    /// Mevcut aktif durum
    /// </summary>
    private IEnemyState currentState;
    
    /// <summary>
    /// EnemyController referansı
    /// </summary>
    private readonly EnemyController owner;
    
    /// <summary>
    /// Durum fabrikası
    /// </summary>
    private readonly EnemyStateFactory stateFactory;
    
    /// <summary>
    /// Mevcut aktif durum
    /// </summary>
    public IEnemyState CurrentState => currentState;
    
    public EnemyStateMachine(EnemyController owner, EnemyStateFactory stateFactory)
    {
        this.owner = owner;
        this.stateFactory = stateFactory;
    }
    
    /// <summary>
    /// Durum makinesini başlatır
    /// </summary>
    public void Initialize()
    {
        // Başlangıç durumunu ayarla (Idle veya Patrol)
        ChangeState<EnemyIdleState>();
    }
    
    /// <summary>
    /// Her karede çağrılır
    /// </summary>
    public void Update()
    {
        if (currentState != null)
        {
            currentState.Update();
        }
    }
    
    /// <summary>
    /// Sabit aralıklarla çağrılır
    /// </summary>
    public void FixedUpdate()
    {
        if (currentState != null)
        {
            currentState.FixedUpdate();
        }
    }
    
    /// <summary>
    /// Verilen durum tipine geçiş yapar
    /// </summary>
    /// <typeparam name="T">Geçiş yapılacak durum tipi</typeparam>
    public void ChangeState<T>() where T : IEnemyState
    {
        // Mevcut durumdan çık
        if (currentState != null)
        {
            currentState.Exit();
        }
        
        // Yeni durumu al
        currentState = stateFactory.GetState<T>();
        
        // Yeni duruma gir
        if (currentState != null)
        {
            currentState.Enter();
            
            Debug.Log($"Enemy state changed to {typeof(T).Name}");
        }
        else
        {
            Debug.LogError($"Failed to change enemy state to {typeof(T).Name}");
        }
    }
} 