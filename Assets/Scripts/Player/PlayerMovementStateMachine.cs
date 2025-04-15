using UnityEngine;

public class PlayerMovementStateMachine
{
    private PlayerController playerController;
    private StateMachine<PlayerController> stateMachine;

    public PlayerMovementStateMachine(PlayerController controller, PlayerStateFactory stateFactory)
    {
        this.playerController = controller;
        stateMachine = new StateMachine<PlayerController>(controller);
        
        // Create states and add them to the state machine
        stateMachine.AddState(stateFactory.Idle());
        stateMachine.AddState(stateFactory.Walk());
        stateMachine.AddState(stateFactory.Run());
        stateMachine.AddState(stateFactory.Jump());
        stateMachine.AddState(stateFactory.Fall());
        stateMachine.AddState(stateFactory.Crouch());
        stateMachine.AddState(stateFactory.Attack());
    }
    
    public void Initialize()
    {
        // Set the initial state to Idle
        stateMachine.Initialize<PlayerIdleState>();
    }
    
    public void ChangeState<T>() where T : IState
    {
        stateMachine.ChangeState<T>();
    }
    
    public void Update()
    {
        stateMachine.Update();
    }
    
    public void FixedUpdate()
    {
        stateMachine.FixedUpdate();
    }
} 