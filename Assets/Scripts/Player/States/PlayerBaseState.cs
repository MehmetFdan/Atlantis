using UnityEngine;

public abstract class PlayerBaseState : IState
{
    protected PlayerController playerController;
    
    // Hareket yumuşatması için sınıf seviyesinde statik değişken
    private static Vector3 currentVelocity = Vector3.zero;
    
    public PlayerBaseState(PlayerController controller)
    {
        this.playerController = controller;
    }
    
    public virtual void Enter()
    {
        // Base implementation
    }
    
    public virtual void Update()
    {
        // Check state transitions
        CheckForStateChange();
    }
    
    public virtual void FixedUpdate()
    {
        // Base implementation for physics
        ApplyGravity();
    }
    
    public virtual void Exit()
    {
        // Base implementation
    }
    
    protected virtual void CheckForStateChange()
    {
        // Override in derived classes to handle state transitions
        
        // Tüm durumlardan saldırı durumuna geçiş kontrolü
        if (playerController.IsAttackPressed)
        {
            playerController.ChangeState<PlayerAttackState>();
        }
    }
    
    protected void ApplyGravity()
    {
        if (!playerController.IsGrounded)
        {
            // Apply gravity when not grounded
            playerController.VerticalVelocity += playerController.Gravity * playerController.GravityMultiplier * Time.fixedDeltaTime;
        }
        else if (playerController.VerticalVelocity < 0)
        {
            // Reset vertical velocity when on ground
            playerController.VerticalVelocity = -2f; // Small negative value to keep the character grounded
        }
    }
    
    protected void MovePlayer(float speed)
    {
        Vector3 moveDirection = playerController.MoveDirection;
        
        if (moveDirection.magnitude > 1f)
        {
            moveDirection.Normalize();
        }
        
        // Hareket yumuşatması için interpolasyon ekle
        float smoothTime = 0.1f; // Yumuşatma değeri - düşük değerler daha az yumuşatma
        
        // Hareket vektörünü yumuşat
        Vector3 targetMovement = moveDirection * speed;
        Vector3 smoothedMovement = Vector3.SmoothDamp(
            playerController.CharacterController.velocity,
            targetMovement,
            ref currentVelocity,
            smoothTime
        );
        
        // Yerçekimini koruyarak yumuşatılmış hareketi uygula
        smoothedMovement.y = playerController.VerticalVelocity;
        
        // Apply the movement using CharacterController
        playerController.CharacterController.Move(smoothedMovement * Time.fixedDeltaTime);
    }
} 