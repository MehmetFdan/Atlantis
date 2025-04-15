using System;
using System.Collections.Generic;

/// <summary>
/// Düşman durum fabrikası
/// </summary>
public class EnemyStateFactory
{
    private readonly Dictionary<Type, IEnemyState> states = new Dictionary<Type, IEnemyState>();
    private readonly EnemyController owner;
    
    public EnemyStateFactory(EnemyController owner)
    {
        this.owner = owner;
        
        // Kullanılabilir durumları oluştur
        RegisterState(new EnemyIdleState(owner, this));
        RegisterState(new EnemyPatrolState(owner, this));
        RegisterState(new EnemyChaseState(owner, this));
        RegisterState(new EnemyAttackState(owner, this));
        RegisterState(new EnemyDeathState(owner, this));
    }
    
    /// <summary>
    /// Durumu kaydeder
    /// </summary>
    /// <param name="state">Kayıt edilecek durum</param>
    private void RegisterState(IEnemyState state)
    {
        states[state.GetType()] = state;
    }
    
    /// <summary>
    /// İstenen tipte durum getirir
    /// </summary>
    /// <typeparam name="T">İstenen durum tipi</typeparam>
    /// <returns>İstenen durumun örneği, bulunamazsa null</returns>
    public T GetState<T>() where T : IEnemyState
    {
        Type stateType = typeof(T);
        
        if (states.TryGetValue(stateType, out IEnemyState state))
        {
            return (T)state;
        }
        
        return default;
    }
} 