using UnityEngine;

public class PlayerFallState : PlayerBaseState
{
    private float fallSpeed;
    
    public PlayerFallState(PlayerController controller) : base(controller) { }
    
    public override void Enter()
    {
        base.Enter();
        Debug.Log("Entered Fall State");
        
        // Calculate the horizontal speed based on whether running or walking
        fallSpeed = playerController.IsRunPressed ? playerController.RunSpeed : playerController.WalkSpeed;
    }
    
    public override void Update()
    {
        base.Update();
    }
    
    public override void FixedUpdate()
    {
        base.FixedUpdate();
        
        // Apply horizontal movement while falling
        MovePlayer(fallSpeed);
    }
    
    protected override void CheckForStateChange()
    {
        // Check if we've landed
        if (playerController.IsGrounded)
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
    }
} 