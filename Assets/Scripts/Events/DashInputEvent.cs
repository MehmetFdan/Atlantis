using Events;

/// <summary>
/// Dash tuşuna basma olayı
/// </summary>
public class DashInputEvent : IEvent
{
    /// <summary>
    /// Dash tuşuna basıldı mı
    /// </summary>
    public bool IsDashPressed { get; set; }
    
    public DashInputEvent(bool isDashPressed = false)
    {
        IsDashPressed = isDashPressed;
    }
} 