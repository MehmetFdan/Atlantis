using UnityEngine;

public class PlayerIdleState : PlayerBaseState
{
    public PlayerIdleState(PlayerController controller) : base(controller) { }
    
    public override void Enter()
    {
        base.Enter();
        Debug.Log("Entered Idle State");
    }
    
    public override void Update()
    {
        base.Update();
    }
    
    public override void FixedUpdate()
    {
        base.FixedUpdate();
        // Keep applying gravity but don't move
        MovePlayer(0f);
    }
    
    protected override void CheckForStateChange()
    {
        // Check if we should transition to other states
        if (playerController.IsJumpPressed && playerController.IsGrounded)
        {
            // Jump state
            playerController.VerticalVelocity = Mathf.Sqrt(playerController.JumpHeight * -2f * playerController.Gravity);
            playerController.ChangeState<PlayerJumpState>();
            return;
        }
        
        if (playerController.IsMovementPressed)
        {
            if (playerController.IsRunPressed)
            {
                // Run state
                playerController.ChangeState<PlayerRunState>();
                return;
            }
            
            // Walk state
            playerController.ChangeState<PlayerWalkState>();
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