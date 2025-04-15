using UnityEngine;

/// <summary>
/// Oyuncunun çömelme durumunu yönetir. 
/// Çömelme sırasında karakter yüksekliği küçülür ve hareket hızı azalır.
/// </summary>
public class PlayerCrouchState : PlayerBaseState
{
    /// <summary>
    /// Karakterin orijinal yüksekliği
    /// </summary>
    [Tooltip("Karakterin orijinal yüksekliği")]
    private float originalHeight;
    
    /// <summary>
    /// Karakterin orijinal merkez noktası
    /// </summary>
    [Tooltip("Karakterin orijinal merkez noktası")]
    private Vector3 originalCenter;
    
    /// <summary>
    /// PlayerCrouchState sınıfı için yapıcı metot
    /// </summary>
    /// <param name="controller">Oyuncu kontrolcüsü referansı</param>
    public PlayerCrouchState(PlayerController controller) : base(controller) { }
    
    /// <summary>
    /// Çömelme durumuna girildiğinde çağrılır.
    /// Karakter kontrolcüsünün boyutlarını ve merkez noktasını çömelmeye uyarlar.
    /// </summary>
    public override void Enter()
    {
        base.Enter();
        Debug.Log("Entered Crouch State");
        
        // Store original character controller properties
        CharacterController characterController = playerController.CharacterController;
        originalHeight = characterController.height;
        originalCenter = characterController.center;
        
        // Adjust character controller for crouching
        characterController.height = originalHeight * playerController.CrouchHeightRatio;
        characterController.center = new Vector3(
            originalCenter.x,
            originalCenter.y * playerController.CrouchHeightRatio,
            originalCenter.z
        );
    }
    
    /// <summary>
    /// Her kare çağrılır, durum geçişlerini kontrol eder.
    /// </summary>
    public override void Update()
    {
        base.Update();
    }
    
    /// <summary>
    /// Fizik motoru güncellemelerinde çağrılır.
    /// Çömelme sırasında oyuncunun hareketini yönetir.
    /// </summary>
    public override void FixedUpdate()
    {
        base.FixedUpdate();
        
        // Move with reduced speed when crouching
        if (playerController.IsMovementPressed)
        {
            MovePlayer(playerController.CrouchSpeed);
        }
        else
        {
            MovePlayer(0f);
        }
    }
    
    /// <summary>
    /// Çömelme durumundan çıkıldığında çağrılır.
    /// Karakter kontrolcüsünün orijinal boyutlarını ve merkez noktasını geri yükler.
    /// </summary>
    public override void Exit()
    {
        // Restore original character controller properties
        CharacterController characterController = playerController.CharacterController;
        characterController.height = originalHeight;
        characterController.center = originalCenter;
        
        base.Exit();
    }
    
    /// <summary>
    /// Durum geçişlerini kontrol eder.
    /// Oyuncunun girdilerine ve fiziksel durumuna göre farklı durumlara geçiş yapar.
    /// </summary>
    protected override void CheckForStateChange()
    {
        // Check if we should transition to other states
        if (!playerController.IsCrouchPressed)
        {
            if (playerController.IsMovementPressed)
            {
                if (playerController.IsRunPressed)
                {
                    playerController.ChangeState<PlayerRunState>();
                }
                else
                {
                    playerController.ChangeState<PlayerWalkState>();
                }
            }
            else
            {
                playerController.ChangeState<PlayerIdleState>();
            }
            return;
        }
        
        if (playerController.IsJumpPressed && playerController.IsGrounded)
        {
            // Jump state
            playerController.VerticalVelocity = Mathf.Sqrt(playerController.JumpHeight * -2f * playerController.Gravity);
            playerController.ChangeState<PlayerJumpState>();
            return;
        }
        
        if (!playerController.IsGrounded && playerController.VerticalVelocity < 0)
        {
            // Fall state
            playerController.ChangeState<PlayerFallState>();
            return;
        }
    }
} 