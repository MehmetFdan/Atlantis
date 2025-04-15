using UnityEngine;
using UnityEngine.InputSystem;

namespace Events
{
    public class InputHandler : MonoBehaviour, InputSystem_Actions.IPlayerActions, InputSystem_Actions.IUIActions
    {
        [SerializeField] private EventBus eventBus;
        
        [Header("Debug")]
        [Tooltip("Gelen input olaylarını konsolda göster")]
        [SerializeField] private bool debugInput = false;
        
        private InputSystem_Actions inputActions;
        
        private void Awake()
        {
            if (eventBus == null)
            {
                Debug.LogError("EventBus reference is missing in InputHandler!");
                eventBus = Resources.Load<EventBus>("EventBus");
                
                if (eventBus == null)
                {
                    Debug.LogError("EventBus couldn't be loaded from Resources either. Input will not work!");
                    return;
                }
            }
            
            inputActions = new InputSystem_Actions();
            inputActions.Player.SetCallbacks(this);
            inputActions.UI.SetCallbacks(this); // UI input callbacklerini aktif et
        }
        
        private void OnEnable()
        {
            inputActions.Enable();
        }
        
        private void OnDisable()
        {
            inputActions.Disable();
        }
        
        public void OnMove(InputAction.CallbackContext context)
        {
            Vector2 movementInput = context.ReadValue<Vector2>();
            if (debugInput) Debug.Log($"Move Input: {movementInput}");
            eventBus.Publish(new MovementInputEvent(movementInput));
        }
        
        public void OnLook(InputAction.CallbackContext context)
        {
            Vector2 lookInput = context.ReadValue<Vector2>();
            eventBus.Publish(new LookInputEvent(lookInput));
        }
        
        public void OnJump(InputAction.CallbackContext context)
        {
            // Only publish on started or canceled to avoid continuous events during hold
            if (context.started)
            {
                eventBus.Publish(new JumpInputEvent(true));
            }
            else if (context.canceled)
            {
                eventBus.Publish(new JumpInputEvent(false));
            }
        }
        
        public void OnAttack(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                eventBus.Publish(new AttackInputEvent(true));
            }
            else if (context.canceled)
            {
                eventBus.Publish(new AttackInputEvent(false));
            }
        }
        
        public void OnCrouch(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                eventBus.Publish(new CrouchInputEvent(true));
            }
            else if (context.canceled)
            {
                eventBus.Publish(new CrouchInputEvent(false));
            }
        }
        
        public void OnInteract(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                eventBus.Publish(new InteractInputEvent(true));
            }
            else if (context.canceled)
            {
                eventBus.Publish(new InteractInputEvent(false));
            }
        }
        
        public void OnSprint(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                eventBus.Publish(new SprintInputEvent(true));
            }
            else if (context.canceled)
            {
                eventBus.Publish(new SprintInputEvent(false));
            }
        }
        
        // Sağ mouse tuşu için UI.RightClick olayını dinle
        public void OnRightClick(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                if (debugInput) Debug.Log("Sağ mouse tuşu basıldı");
                eventBus.Publish(new RightMouseInputEvent(true));
            }
            else if (context.canceled)
            {
                if (debugInput) Debug.Log("Sağ mouse tuşu bırakıldı");
                eventBus.Publish(new RightMouseInputEvent(false));
            }
        }
        
        // Implementing required interface methods
        public void OnNext(InputAction.CallbackContext context) { }
        public void OnPrevious(InputAction.CallbackContext context) { }
        
        // Implementing required IUIActions interface methods
        public void OnNavigate(InputAction.CallbackContext context) { }
        public void OnSubmit(InputAction.CallbackContext context) { }
        public void OnCancel(InputAction.CallbackContext context) { }
        public void OnPoint(InputAction.CallbackContext context) { }
        public void OnClick(InputAction.CallbackContext context) { }
        public void OnMiddleClick(InputAction.CallbackContext context) { }
        public void OnScrollWheel(InputAction.CallbackContext context) { }
        public void OnTrackedDevicePosition(InputAction.CallbackContext context) { }
        public void OnTrackedDeviceOrientation(InputAction.CallbackContext context) { }
    }
} 