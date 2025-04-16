using UnityEngine;

public class PlayerStateFactory
{
    private PlayerController playerController;
    
    public PlayerStateFactory(PlayerController controller)
    {
        this.playerController = controller;
    }
    
    public PlayerIdleState Idle()
    {
        return new PlayerIdleState(playerController);
    }
    
    public PlayerWalkState Walk()
    {
        return new PlayerWalkState(playerController);
    }
    
    public PlayerRunState Run()
    {
        return new PlayerRunState(playerController);
    }
    
    public PlayerJumpState Jump()
    {
        return new PlayerJumpState(playerController);
    }
    
    public PlayerFallState Fall()
    {
        return new PlayerFallState(playerController);
    }
    
    public PlayerCrouchState Crouch()
    {
        return new PlayerCrouchState(playerController);
    }
    
    public PlayerAttackState Attack()
    {
        return new PlayerAttackState(playerController);
    }
    
    public PlayerDashState Dash()
    {
        return new PlayerDashState(playerController);
    }
    
    public PlayerParryState Parry()
    {
        return new PlayerParryState(playerController);
    }
} 