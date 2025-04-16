using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Düşman durum fabrikası. Düşmanın tüm durumlarını oluşturur ve yönetir.
/// </summary>
public class EnemyStateFactory
{
    private readonly Dictionary<Type, IEnemyState> states = new Dictionary<Type, IEnemyState>();
    private readonly EnemyController owner;
    
    public EnemyStateFactory(EnemyController owner)
    {
        this.owner = owner;
        
        // Temel düşman durumlarını kaydet
        RegisterState(new EnemyIdleState(owner, this));
        RegisterState(new EnemyPatrolState(owner, this));
        RegisterState(new EnemyChaseState(owner, this));
        RegisterState(new EnemyAttackState(owner, this));
        RegisterState(new EnemyDeathState(owner, this));
        
        // Yeni eklenen ileri düzey düşman durumlarını kaydet
        RegisterState(new EnemyFleeState(owner, this));
        RegisterState(new EnemyInvestigateState(owner, this));
        RegisterState(new EnemyRangedAttackState(owner, this));
    }
    
    /// <summary>
    /// Durumu kaydeder
    /// </summary>
    /// <param name="state">Kayıt edilecek durum</param>
    private void RegisterState(IEnemyState state)
    {
        Type stateType = state.GetType();
        
        if (!states.ContainsKey(stateType))
        {
            states[stateType] = state;
        }
        else
        {
            Debug.LogWarning($"Duplicate state registration: {stateType.Name}");
        }
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
        
        Debug.LogError($"State not found: {stateType.Name}");
        return default;
    }
} 