using UnityEngine;

public class PlayerRunState : PlayerBaseState
{
    public PlayerRunState(PlayerController controller) : base(controller) { }
    
    public override void Enter()
    {
        base.Enter();
        Debug.Log("Entered Run State");
    }
    
    public override void Update()
    {
        base.Update();
    }
    
    public override void FixedUpdate()
    {
        base.FixedUpdate();
        // Move with run speed
        MovePlayer(playerController.RunSpeed);
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
        
        if (!playerController.IsRunPressed)
        {
            // Change to walk state
            playerController.ChangeState<PlayerWalkState>();
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