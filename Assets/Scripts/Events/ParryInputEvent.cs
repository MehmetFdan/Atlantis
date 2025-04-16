using Events;

/// <summary>
/// Parry (savuşturma) tuşuna basma olayı
/// </summary>
public class ParryInputEvent : IEvent
{
    /// <summary>
    /// Parry tuşuna basıldı mı
    /// </summary>
    public bool IsParryPressed { get; set; }
    
    public ParryInputEvent(bool isParryPressed = false)
    {
        IsParryPressed = isParryPressed;
    }
} 