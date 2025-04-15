/// <summary>
/// Tüm düşman durumları için temel sınıf
/// </summary>
public abstract class EnemyBaseState : IEnemyState
{
    protected readonly EnemyController owner;
    protected readonly EnemyStateFactory stateFactory;
    
    protected EnemyBaseState(EnemyController owner, EnemyStateFactory stateFactory)
    {
        this.owner = owner;
        this.stateFactory = stateFactory;
    }
    
    /// <summary>
    /// Duruma giriş yapıldığında çağrılır
    /// </summary>
    public virtual void Enter()
    {
        // Alt sınıflar tarafından override edilecek
    }
    
    /// <summary>
    /// Her karede çağrılır
    /// </summary>
    public virtual void Update()
    {
        // Alt sınıflar tarafından override edilecek
    }
    
    /// <summary>
    /// Sabit aralıklarla çağrılır
    /// </summary>
    public virtual void FixedUpdate()
    {
        // Alt sınıflar tarafından override edilecek
    }
    
    /// <summary>
    /// Durumdan çıkış yapıldığında çağrılır
    /// </summary>
    public virtual void Exit()
    {
        // Alt sınıflar tarafından override edilecek
    }
    
    /// <summary>
    /// Durum değişikliği yapar
    /// </summary>
    /// <typeparam name="T">Geçiş yapılacak durum tipi</typeparam>
    protected void ChangeState<T>() where T : IEnemyState
    {
        owner.ChangeState<T>();
    }
} 