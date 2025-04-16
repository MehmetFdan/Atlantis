using UnityEngine;
using Atlantis.Events;

/// <summary>
/// Oyuncunun dash durumunu yöneten sınıf
/// </summary>
public class PlayerDashState : PlayerBaseState
{
    /// <summary>
    /// Dash olayları için EventBus referansı
    /// </summary>
    private EventBus eventBus => playerController.EventBus;
    
    public PlayerDashState(PlayerController controller) : base(controller)
    {
    }
    
    public override void Enter()
    {
        base.Enter();
        
        // Dash başlat
        playerController.StartDash();
        
        // Debug mesajı
        Debug.Log("Dash durumuna girildi!");
    }
    
    public override void Update()
    {
        base.Update();
        
        // Dash durumu bittiyse önceki duruma dön
        if (!playerController.IsDashing)
        {
            ReturnToPreviousState();
        }
    }
    
    public override void FixedUpdate()
    {
        base.FixedUpdate();
        
        // Dash hareketini uygula
        playerController.ApplyDashMovement();
    }
    
    /// <summary>
    /// Dash durumundan çıkış
    /// </summary>
    public override void Exit()
    {
        base.Exit();
        
        // Debug mesajı
        Debug.Log("Dash durumundan çıkıldı!");
    }
    
    /// <summary>
    /// Önceki duruma dönüş
    /// </summary>
    protected void ReturnToPreviousState()
    {
        // Yerdeyse idle durumuna, değilse düşme durumuna geç
        if (playerController.IsGrounded)
        {
            // Hareket tuşuna basılıysa
            if (playerController.IsMovementPressed)
            {
                // Koşu tuşuna basılıysa koşma durumuna
                if (playerController.IsRunPressed)
                {
                    playerController.ChangeState<PlayerRunState>();
                }
                // Basılı değilse yürüme durumuna geç
                else
                {
                    playerController.ChangeState<PlayerWalkState>();
                }
            }
            // Hareket yoksa boşta durumuna geç
            else
            {
                playerController.ChangeState<PlayerIdleState>();
            }
        }
        // Yerde değilse düşme durumuna geç
        else
        {
            playerController.ChangeState<PlayerFallState>();
        }
    }
    
    /// <summary>
    /// Durum değişikliği kontrolü
    /// </summary>
    protected override void CheckForStateChange()
    {
        // Dash durumunda başka durumlara geçiş yapılmasın
        // Dash süresi bittiğinde ReturnToPreviousState metodu çağrılacak
    }
} 