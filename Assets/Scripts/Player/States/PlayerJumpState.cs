using UnityEngine;

public class PlayerJumpState : PlayerBaseState
{
    private float jumpSpeed;
    
    public PlayerJumpState(PlayerController controller) : base(controller) { }
    
    public override void Enter()
    {
        base.Enter();
        Debug.Log("Entered Jump State");
        
        // Calculate the horizontal speed based on whether running or walking
        jumpSpeed = playerController.IsRunPressed ? playerController.RunSpeed : playerController.WalkSpeed;
    }
    
    public override void Update()
    {
        base.Update();
    }
    
    public override void FixedUpdate()
    {
        base.FixedUpdate();
        
        // Apply horizontal movement while in air
        MovePlayer(jumpSpeed);
    }
    
    protected override void CheckForStateChange()
    {
        // Check if we should transition to other states
        
        // Transition to fall state as vertical velocity becomes negative
        if (playerController.VerticalVelocity <= 0)
        {
            playerController.ChangeState<PlayerFallState>();
            return;
        }
    }
} 