using UnityEngine;

/// <summary>
/// Oyuncu hareket özelliklerini yöneten ScriptableObject.
/// Oyuncunun hızı, zıplama yüksekliği gibi ayarları içerir.
/// </summary>
[CreateAssetMenu(fileName = "MovementSettings", menuName = "Game/Player/Movement Settings")]
public class MovementSettings : ScriptableObject
{
    [Header("Hareket Hızları")]
    [Tooltip("Yürüme hızı")]
    [SerializeField] private float walkSpeed = 5f;
    
    [Tooltip("Koşma hızı")]
    [SerializeField] private float runSpeed = 8f;
    
    [Tooltip("Çömelme hızı")]
    [SerializeField] private float crouchSpeed = 2f;
    
    [Header("Zıplama Ayarları")]
    [Tooltip("Zıplama yüksekliği")]
    [SerializeField] private float jumpHeight = 1.5f;
    
    [Tooltip("Yerçekimi çarpanı")]
    [SerializeField] private float gravityMultiplier = 2.5f;
    
    [Tooltip("Düşme esnasında ek yerçekimi çarpanı")]
    [SerializeField] private float fallGravityMultiplier = 4f;
    
    [Tooltip("Minimum yerde kalma süresi (saniye)")]
    [SerializeField] private float minimumGroundedTime = 0.1f;

    [Tooltip("Coyote time süresi (saniye)")]
    [SerializeField] private float coyoteTime = 0.15f;
    
    [Header("Dönüş Ayarları")]
    [Tooltip("Dönüş hızı")]
    [SerializeField] private float rotationSpeed = 10f;
    
    [Header("Çömelme Ayarları")]
    [Tooltip("Çömelme sırasında yükseklik oranı")]
    [SerializeField] private float crouchHeightRatio = 0.6f;
    
    [Tooltip("Çömelme geçiş hızı")]
    [SerializeField] private float crouchTransitionSpeed = 8f;
    
    [Header("Dash Ayarları")]
    [Tooltip("Dash hızı")]
    [SerializeField] private float dashSpeed = 15f;
    
    [Tooltip("Dash süresi (saniye)")]
    [SerializeField] private float dashDuration = 0.2f;
    
    [Tooltip("Dash cooldown süresi (saniye)")]
    [SerializeField] private float dashCooldown = 1.5f;
    
    [Tooltip("Dash ile yön değişimi yapılabilir mi?")]
    [SerializeField] private bool canChangeDirectionWhileDashing = false;
    
    [Tooltip("Dash sırasında alınan hasar azaltma çarpanı (0-1)")]
    [Range(0f, 1f)]
    [SerializeField] private float dashDamageReduction = 0.5f;
    
    [Header("Hız Sınırları")]
    [Tooltip("Maksimum düşme hızı")]
    [SerializeField] private float maximumFallSpeed = 30f;
    
    /// <summary>
    /// Yürüme hızı
    /// </summary>
    public float WalkSpeed => walkSpeed;
    
    /// <summary>
    /// Koşma hızı
    /// </summary>
    public float RunSpeed => runSpeed;
    
    /// <summary>
    /// Çömelme hızı
    /// </summary>
    public float CrouchSpeed => crouchSpeed;
    
    /// <summary>
    /// Zıplama yüksekliği
    /// </summary>
    public float JumpHeight => jumpHeight;
    
    /// <summary>
    /// Yerçekimi çarpanı
    /// </summary>
    public float GravityMultiplier => gravityMultiplier;
    
    /// <summary>
    /// Düşme esnasında ek yerçekimi çarpanı
    /// </summary>
    public float FallGravityMultiplier => fallGravityMultiplier;
    
    /// <summary>
    /// Minimum yerde kalma süresi
    /// </summary>
    public float MinimumGroundedTime => minimumGroundedTime;
    
    /// <summary>
    /// Coyote time süresi (saniye)
    /// </summary>
    public float CoyoteTime => coyoteTime;
    
    /// <summary>
    /// Dönüş hızı
    /// </summary>
    public float RotationSpeed => rotationSpeed;
    
    /// <summary>
    /// Çömelme sırasında yükseklik oranı
    /// </summary>
    public float CrouchHeightRatio => crouchHeightRatio;
    
    /// <summary>
    /// Çömelme geçiş hızı
    /// </summary>
    public float CrouchTransitionSpeed => crouchTransitionSpeed;
    
    /// <summary>
    /// Dash hızı
    /// </summary>
    public float DashSpeed => dashSpeed;
    
    /// <summary>
    /// Dash süresi
    /// </summary>
    public float DashDuration => dashDuration;
    
    /// <summary>
    /// Dash cooldown süresi
    /// </summary>
    public float DashCooldown => dashCooldown;
    
    /// <summary>
    /// Dash ile yön değişimi yapılabilir mi?
    /// </summary>
    public bool CanChangeDirectionWhileDashing => canChangeDirectionWhileDashing;
    
    /// <summary>
    /// Dash sırasında alınan hasar azaltma çarpanı
    /// </summary>
    public float DashDamageReduction => dashDamageReduction;
    
    /// <summary>
    /// Maksimum düşme hızı
    /// </summary>
    public float MaximumFallSpeed => maximumFallSpeed;
} 