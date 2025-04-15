/// <summary>
/// Düşman durum arayüzü
/// </summary>
public interface IEnemyState
{
    /// <summary>
    /// Duruma giriş yapıldığında çağrılır
    /// </summary>
    void Enter();
    
    /// <summary>
    /// Her karede çağrılır
    /// </summary>
    void Update();
    
    /// <summary>
    /// Sabit aralıklarla çağrılır
    /// </summary>
    void FixedUpdate();
    
    /// <summary>
    /// Durumdan çıkış yapıldığında çağrılır
    /// </summary>
    void Exit();
} 