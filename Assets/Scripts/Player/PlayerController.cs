using UnityEngine;
using UnityEngine.InputSystem;
using Events;

/// <summary>
/// Oyuncu kontrolcüsü. Oyuncunun hareketlerini, durumlarını ve girdilerini yönetir.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Dependencies")]
    /// <summary>
    /// Oyun içi olayları yönetmek için EventBus referansı
    /// </summary>
    [Tooltip("Oyun içi olayları yönetmek için EventBus")]
    [SerializeField] private EventBus eventBus;
    
    /// <summary>
    /// Hareket ayarları ScriptableObject referansı
    /// </summary>
    [Tooltip("Oyuncu hareket ayarlarını içeren ScriptableObject")]
    [SerializeField] private MovementSettings movementSettings;
    
    /// <summary>
    /// Saldırı ayarları ScriptableObject referansı
    /// </summary>
    [Tooltip("Oyuncu saldırı ayarlarını içeren ScriptableObject")]
    [SerializeField] private CombatSettings combatSettings;
    
    /// <summary>
    /// Aktif silah veri referansı
    /// </summary>
    [Tooltip("Aktif silah verileri")]
    [SerializeField] private WeaponData currentWeapon;
    
    /// <summary>
    /// Kullanılabilir silahlar listesi
    /// </summary>
    [Tooltip("Kullanılabilir silahlar listesi")]
    [SerializeField] private WeaponData[] availableWeapons;
    
    /// <summary>
    /// Silah dönüşüm noktası (sağ el)
    /// </summary>
    [Tooltip("Sağ el silah noktası")]
    [SerializeField] private Transform rightHandWeaponSocket;
    
    /// <summary>
    /// Silah dönüşüm noktası (sol el)
    /// </summary>
    [Tooltip("Sol el silah noktası")]
    [SerializeField] private Transform leftHandWeaponSocket;
    
    // References
    [Tooltip("CharacterController referansı")]
    private CharacterController characterController;
    
    [Tooltip("Oyuncu hareket durum makinesi")]
    public PlayerMovementStateMachine MovementStateMachine { get; private set; }
    
    [Tooltip("Oyuncu durum fabrikası")]
    private PlayerStateFactory stateFactory;
    
    // Movement data
    [Tooltip("Anlık hareket input vektörü")]
    private Vector2 currentMovementInput;
    
    [Tooltip("Anlık bakış input vektörü")]
    private Vector2 currentLookInput;
    
    [Tooltip("Anlık hareket vektörü")]
    private Vector3 currentMovement;
    
    [Tooltip("Anlık koşu hareket vektörü")]
    private Vector3 currentRunMovement;
    
    [Tooltip("Hareket tuşuna basılıp basılmadığı")]
    private bool isMovementPressed;
    
    [Tooltip("Koşma tuşuna basılıp basılmadığı")]
    private bool isRunPressed;
    
    [Tooltip("Zıplama tuşuna basılıp basılmadığı")]
    private bool isJumpPressed;
    
    [Tooltip("Çömelme tuşuna basılıp basılmadığı")]
    private bool isCrouchPressed;
    
    [Tooltip("Nişan alma tuşuna basılıp basılmadığı")]
    private bool isAimingPressed;
    
    [Tooltip("Saldırı tuşuna basılıp basılmadığı")]
    private bool isAttackPressed;
    
    // Ground check
    [Tooltip("Karakterin yerde olup olmadığı")]
    private bool isGrounded;
    
    [Tooltip("Dikey hız değeri")]
    private float verticalVelocity;
    
    [Tooltip("Yerçekimi değeri")]
    private readonly float gravity = Physics.gravity.y;
    
    // Events
    [Tooltip("Input system eylemleri")]
    private InputSystem_Actions inputActions;

    /// <summary>
    /// CharacterController bileşeninin referansı
    /// </summary>
    public CharacterController CharacterController => characterController;
    
    /// <summary>
    /// Yürüme hızı
    /// </summary>
    public float WalkSpeed => movementSettings != null ? movementSettings.WalkSpeed : 5f;
    
    /// <summary>
    /// Koşma hızı
    /// </summary>
    public float RunSpeed => movementSettings != null ? movementSettings.RunSpeed : 8f;
    
    /// <summary>
    /// Zıplama yüksekliği
    /// </summary>
    public float JumpHeight => movementSettings != null ? movementSettings.JumpHeight : 1.5f;
    
    /// <summary>
    /// Yerçekimi çarpanı
    /// </summary>
    public float GravityMultiplier => movementSettings != null ? movementSettings.GravityMultiplier : 2.5f;
    
    /// <summary>
    /// Anlık hareket girdisi
    /// </summary>
    public Vector2 CurrentMovementInput => currentMovementInput;
    
    /// <summary>
    /// Hareket tuşuna basılıp basılmadığı
    /// </summary>
    public bool IsMovementPressed => isMovementPressed;
    
    /// <summary>
    /// Koşma tuşuna basılıp basılmadığı
    /// </summary>
    public bool IsRunPressed => isRunPressed;
    
    /// <summary>
    /// Zıplama tuşuna basılıp basılmadığı
    /// </summary>
    public bool IsJumpPressed => isJumpPressed;
    
    /// <summary>
    /// Çömelme tuşuna basılıp basılmadığı
    /// </summary>
    public bool IsCrouchPressed => isCrouchPressed;
    
    /// <summary>
    /// Nişan alma tuşuna basılıp basılmadığı
    /// </summary>
    public bool IsAimingPressed => isAimingPressed;
    
    /// <summary>
    /// Saldırı tuşuna basılıp basılmadığı
    /// </summary>
    public bool IsAttackPressed => isAttackPressed;
    
    /// <summary>
    /// Karakterin yerde olup olmadığı
    /// </summary>
    public bool IsGrounded => isGrounded;
    
    /// <summary>
    /// Dikey hız değeri
    /// </summary>
    public float VerticalVelocity { get => verticalVelocity; set => verticalVelocity = value; }
    
    /// <summary>
    /// Yerçekimi değeri
    /// </summary>
    public float Gravity => gravity;
    
    /// <summary>
    /// Hareket yönü
    /// </summary>
    public Vector3 MoveDirection { get; private set; }
    
    /// <summary>
    /// Dönüş hızı
    /// </summary>
    public float RotationSpeed => movementSettings != null ? movementSettings.RotationSpeed : 10f;
    
    /// <summary>
    /// Çömelme yükseklik oranı
    /// </summary>
    public float CrouchHeightRatio => movementSettings != null ? movementSettings.CrouchHeightRatio : 0.6f;
    
    /// <summary>
    /// Çömelme hızı
    /// </summary>
    public float CrouchSpeed => movementSettings != null ? movementSettings.CrouchSpeed : 2f;
    
    /// <summary>
    /// EventBus referansı
    /// </summary>
    public EventBus EventBus => eventBus;
    
    /// <summary>
    /// Saldırı ayarları referansı
    /// </summary>
    public CombatSettings CombatSettings => combatSettings;
    
    /// <summary>
    /// Aktif silah referansı
    /// </summary>
    public WeaponData CurrentWeapon => currentWeapon;
    
    /// <summary>
    /// Silahları saklar
    /// </summary>
    private GameObject equippedWeaponObject;
    
    /// <summary>
    /// Donatılmış silah objesi
    /// </summary>
    public GameObject EquippedWeaponObject => equippedWeaponObject;
    
    /// <summary>
    /// Durum değişikliği için yardımcı metot
    /// </summary>
    /// <typeparam name="T">Geçiş yapılacak durum türü</typeparam>
    public void ChangeState<T>() where T : IState
    {
        MovementStateMachine.ChangeState<T>();
    }
    
    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        
        // Initialize state machine
        stateFactory = new PlayerStateFactory(this);
        MovementStateMachine = new PlayerMovementStateMachine(this, stateFactory);
        
        // Initialize input
        inputActions = new InputSystem_Actions();
        
        // Subscribe to events
        if (eventBus != null)
        {
            eventBus.Subscribe<MovementInputEvent>(OnMovementInput);
            eventBus.Subscribe<JumpInputEvent>(OnJumpInput);
            eventBus.Subscribe<SprintInputEvent>(OnSprintInput);
            eventBus.Subscribe<CrouchInputEvent>(OnCrouchInput);
            eventBus.Subscribe<AttackInputEvent>(OnAttackInput);
            // Sağ mouse tuşu için ayrı bir event dinleyici ekle
            eventBus.Subscribe<RightMouseInputEvent>(OnRightMouseInput);
        }
        else
        {
            Debug.LogError("EventBus reference is missing in PlayerController!");
        }
    }
    
    private void OnEnable()
    {
        inputActions.Enable();
    }
    
    private void OnDisable()
    {
        inputActions.Disable();
    }
    
    private void Start()
    {
        MovementStateMachine.Initialize();
        
        // Başlangıç silahını donat (varsa)
        if (currentWeapon != null)
        {
            EquipWeapon(currentWeapon);
        }
    }
    
    private void Update()
    {
        HandleRotation();
        CheckGrounded();
        MovementStateMachine.Update();
        
        // Silah değiştirme kontrolleri
        HandleWeaponSwitching();
    }
    
    private void FixedUpdate()
    {
        MovementStateMachine.FixedUpdate();
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from all events to prevent memory leaks
        if (eventBus != null)
        {
            eventBus.Unsubscribe<MovementInputEvent>(OnMovementInput);
            eventBus.Unsubscribe<JumpInputEvent>(OnJumpInput);
            eventBus.Unsubscribe<SprintInputEvent>(OnSprintInput);
            eventBus.Unsubscribe<CrouchInputEvent>(OnCrouchInput);
            eventBus.Unsubscribe<AttackInputEvent>(OnAttackInput);
            // Sağ mouse tuşu için unsubscribe ekle
            eventBus.Unsubscribe<RightMouseInputEvent>(OnRightMouseInput);
        }
    }
    
    private void OnMovementInput(MovementInputEvent eventData)
    {
        currentMovementInput = eventData.MovementEvent;
        isMovementPressed = currentMovementInput.magnitude > 0;
        
        // Basit hareket yönü hesaplaması
        if (isMovementPressed)
        {
            Vector3 moveDir = new Vector3(currentMovementInput.x, 0, currentMovementInput.y);
            moveDir.Normalize();
            
            MoveDirection = moveDir;
        }
        else
        {
            MoveDirection = Vector3.zero;
        }
    }
    
    private void OnJumpInput(JumpInputEvent eventData)
    {
        isJumpPressed = eventData.JumpPressed;
    }
    
    private void OnSprintInput(SprintInputEvent eventData)
    {
        isRunPressed = eventData.SprintPressed;
    }
    
    private void OnCrouchInput(CrouchInputEvent eventData)
    {
        isCrouchPressed = eventData.CrouchPressed;
    }
    
    private void OnAttackInput(AttackInputEvent eventData)
    {
        isAttackPressed = eventData.AttackPressed;
        
        if (isAttackPressed)
        {
            // Saldırı state'ine geç
            ChangeState<PlayerAttackState>();
        }
    }
    
    
    // Sağ mouse tuşu için yeni bir metot
    private void OnRightMouseInput(RightMouseInputEvent eventData)
    {
        isAimingPressed = eventData.AimPressed;
    }
    
    private void HandleRotation()
    {
        if (MoveDirection != Vector3.zero)
        {
            // Kamera yönünü hareket yönüne uygula
            Camera mainCamera = Camera.main;
            
            if (mainCamera != null)
            {
                Vector3 cameraForward = mainCamera.transform.forward;
                Vector3 cameraRight = mainCamera.transform.right;
                
                cameraForward.y = 0f;
                cameraRight.y = 0f;
                cameraForward.Normalize();
                cameraRight.Normalize();
                
                Vector3 moveDir = cameraRight * MoveDirection.x + cameraForward * MoveDirection.z;
                moveDir.Normalize();
                
                Quaternion targetRotation = Quaternion.LookRotation(moveDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, RotationSpeed * Time.deltaTime);
                
                // Hareket yönünü kamera referansına göre güncelle
                MoveDirection = moveDir;
            }
            else
            {
                Quaternion targetRotation = Quaternion.LookRotation(MoveDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, RotationSpeed * Time.deltaTime);
            }
        }
    }
    
    private void CheckGrounded()
    {
        isGrounded = characterController.isGrounded;
    }
    
    /// <summary>
    /// Silahı değiştirir ve donatır
    /// </summary>
    /// <param name="weaponIndex">Silah indeksi</param>
    public void SwitchWeapon(int weaponIndex)
    {
        if (availableWeapons == null || availableWeapons.Length == 0)
        {
            Debug.LogWarning("No available weapons to switch!");
            return;
        }
        
        if (weaponIndex < 0 || weaponIndex >= availableWeapons.Length)
        {
            Debug.LogWarning($"Invalid weapon index: {weaponIndex}! Valid range: 0-{availableWeapons.Length - 1}");
            return;
        }
        
        EquipWeapon(availableWeapons[weaponIndex]);
    }
    
    /// <summary>
    /// Silahı donatır
    /// </summary>
    /// <param name="weaponData">Silah verisi</param>
    public void EquipWeapon(WeaponData weaponData)
    {
        // Önceki silahı kaldır
        UnequipCurrentWeapon();
        
        currentWeapon = weaponData;
        
        if (currentWeapon != null && currentWeapon.WeaponPrefab != null)
        {
            Transform weaponSocket = DetermineWeaponSocket(currentWeapon.HandSlot);
            
            if (weaponSocket != null)
            {
                equippedWeaponObject = Instantiate(currentWeapon.WeaponPrefab, weaponSocket);
                equippedWeaponObject.transform.localPosition = Vector3.zero;
                equippedWeaponObject.transform.localRotation = Quaternion.identity;
                
                Debug.Log($"Equipped weapon: {currentWeapon.WeaponName}");
            }
            else
            {
                Debug.LogWarning("Could not determine weapon socket! Weapon not equipped.");
            }
        }
    }
    
    /// <summary>
    /// Mevcut silahı kaldırır
    /// </summary>
    public void UnequipCurrentWeapon()
    {
        if (equippedWeaponObject != null)
        {
            Destroy(equippedWeaponObject);
            equippedWeaponObject = null;
        }
    }
    
    /// <summary>
    /// Silahın tutulacağı soketi belirler
    /// </summary>
    /// <param name="handSlot">El yuvası (0: Sağ, 1: Sol, 2: Çift el)</param>
    /// <returns>Silah soketi transform'u</returns>
    private Transform DetermineWeaponSocket(int handSlot)
    {
        switch (handSlot)
        {
            case 0:
                return rightHandWeaponSocket;
            case 1:
                return leftHandWeaponSocket;
            case 2:
                // Çift el silahı için genellikle dominant el kullanılır
                return rightHandWeaponSocket;
            default:
                return rightHandWeaponSocket;
        }
    }
    
    /// <summary>
    /// Klavyeden silah değiştirme işlemlerini kontrol eder
    /// </summary>
    private void HandleWeaponSwitching()
    {
        // Sayı tuşları için kontrol et (1-9)
        for (int i = 0; i < 9; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                SwitchWeapon(i);
                break;
            }
        }
        
        // Fare tekerleği ile silah değiştirme
        float scrollWheel = Input.GetAxis("Mouse ScrollWheel");
        if (scrollWheel != 0)
        {
            // Aktif silahın indeksini bul
            int currentIndex = GetCurrentWeaponIndex();
            int newIndex;
            
            if (scrollWheel > 0)
            {
                // Bir sonraki silaha geç
                newIndex = (currentIndex + 1) % availableWeapons.Length;
            }
            else
            {
                // Bir önceki silaha geç
                newIndex = (currentIndex - 1 + availableWeapons.Length) % availableWeapons.Length;
            }
            
            SwitchWeapon(newIndex);
        }
    }
    
    /// <summary>
    /// Mevcut donatılmış silahın indeksini döndürür
    /// </summary>
    private int GetCurrentWeaponIndex()
    {
        if (currentWeapon == null || availableWeapons == null || availableWeapons.Length == 0)
        {
            return -1;
        }
        
        for (int i = 0; i < availableWeapons.Length; i++)
        {
            if (availableWeapons[i] == currentWeapon)
            {
                return i;
            }
        }
        
        return -1;
    }
} 