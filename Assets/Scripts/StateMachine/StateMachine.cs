using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Genel durum makinesini temsil eden sınıf
/// </summary>
public class StateMachine<TOwner>
{
    private IState currentState;
    private readonly Dictionary<Type, IState> states = new Dictionary<Type, IState>();
    protected TOwner owner;

    /// <summary>
    /// Constructor - durum makinesinin sahibini belirler
    /// </summary>
    public StateMachine(TOwner owner)
    {
        this.owner = owner;
    }

    /// <summary>
    /// Mevcut çalışan durum
    /// </summary>
    public IState CurrentState => currentState;

    /// <summary>
    /// Belirtilen durumu başlangıç durumu olarak ayarlar
    /// </summary>
    /// <typeparam name="T">Başlangıç durumu türü</typeparam>
    public void Initialize<T>() where T : IState
    {
        currentState = GetState<T>();
        if (currentState == null)
        {
            Debug.LogError($"State of type {typeof(T).Name} has not been added to the state machine");
            return;
        }
        currentState.Enter();
    }

    /// <summary>
    /// Durum ekler
    /// </summary>
    /// <param name="state">Eklenecek durum</param>
    public void AddState(IState state)
    {
        if (state == null)
        {
            Debug.LogError("Cannot add null state to state machine");
            return;
        }
        
        Type stateType = state.GetType();
        if (states.ContainsKey(stateType))
        {
            Debug.LogWarning($"State of type {stateType.Name} already exists in the state machine");
            return;
        }
        
        states[stateType] = state;
    }

    /// <summary>
    /// Belirtilen türdeki durumu döndürür
    /// </summary>
    /// <typeparam name="T">İstenen durum türü</typeparam>
    /// <returns>İstenen durum</returns>
    public T GetState<T>() where T : IState
    {
        Type stateType = typeof(T);
        if (!states.ContainsKey(stateType))
        {
            Debug.LogError($"State of type {stateType.Name} does not exist in the state machine");
            return default;
        }
        
        return (T)states[stateType];
    }

    /// <summary>
    /// Belirtilen duruma geçer
    /// </summary>
    /// <typeparam name="T">Geçilecek durum türü</typeparam>
    public void ChangeState<T>() where T : IState
    {
        IState newState = GetState<T>();
        if (newState == null)
        {
            Debug.LogError($"Cannot change to state of type {typeof(T).Name} as it does not exist");
            return;
        }

        // Aynı duruma geçiş gereksizse görmezden gel
        if (currentState == newState) return;

        if (currentState != null)
        {
            try
            {
                currentState.Exit();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error during state exit: {ex.Message}");
            }
        }

        try
        {
            currentState = newState;
            currentState.Enter();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error during state enter: {ex.Message}");
        }
    }

    /// <summary>
    /// Mevcut duruma güncelleme gönderir
    /// </summary>
    public void Update()
    {
        if (currentState != null)
        {
            try
            {
                currentState.Update();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error during state update: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Mevcut duruma sabitle güncelleme gönderir
    /// </summary>
    public void FixedUpdate()
    {
        if (currentState != null)
        {
            try
            {
                currentState.FixedUpdate();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error during state fixed update: {ex.Message}");
            }
        }
    }
} 