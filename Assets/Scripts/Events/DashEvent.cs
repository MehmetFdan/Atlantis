using UnityEngine;
using Events;

/// <summary>
/// Dash olayı
/// </summary>
public class DashEvent : IEvent
{
    /// <summary>
    /// Dash yönü
    /// </summary>
    public Vector3 DashDirection { get; set; }
    
    public DashEvent()
    {
        DashDirection = Vector3.forward;
    }
} 