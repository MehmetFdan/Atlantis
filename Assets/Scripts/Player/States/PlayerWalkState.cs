using UnityEngine;

public class PlayerWalkState : PlayerBaseState
{
    public PlayerWalkState(PlayerController controller) : base(controller) { }
    
    public override void Enter()
    {
        base.Enter();
        Debug.Log("Entered Walk State");
    }
    
    public override void Update()
    {
        base.Update();
    }
    
    public override void FixedUpdate()
    {
        base.FixedUpdate();
        // Move with walk speed
        MovePlayer(playerController.WalkSpeed);
    }
    
    protected override void CheckForStateChange()
    {
        // Check if we should transition to other states
        if (!playerController.IsMovementPressed)
        {
            // Back to idle if no movement
            playerController.ChangeState<PlayerIdleState>();
            return;
        }
        
        if (playerController.IsRunPressed)
        {
            // Change to run state
            playerController.ChangeState<PlayerRunState>();
            return;
        }
        
        if (playerController.IsJumpPressed && playerController.IsGrounded)
        {
            // Jump state
            playerController.VerticalVelocity = Mathf.Sqrt(playerController.JumpHeight * -2f * playerController.Gravity);
            playerController.ChangeState<PlayerJumpState>();
            return;
        }
        
        if (playerController.IsCrouchPressed)
        {
            // Crouch state
            playerController.ChangeState<PlayerCrouchState>();
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