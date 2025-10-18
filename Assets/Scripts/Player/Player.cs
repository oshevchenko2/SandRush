using UnityEngine;
using Unity.Cinemachine;
using TMPro;

[RequireComponent(typeof(CharacterController))]
public class Player : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private Transform _firePoint;
    [SerializeField] private GameObject _bulletPrefab;
    [SerializeField] private Animator _animator;
    [SerializeField] private TextMeshProUGUI _dashCooldownText;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float accelerationTime = 0.1f;

    [Header("Aiming Settings")]
    [Tooltip("Adjust this value until the character's model faces the exact direction of the shots.")]
    [SerializeField] private float _rotationOffset = 0f;
    [Tooltip("How quickly the character turns to face the aim direction.")]
    [SerializeField] private float _rotationSpeed = 25f;
    
    [Header("Impulse Sources")]
    [SerializeField] private CinemachineImpulseSource _dashImpulseSource;
    [SerializeField] private CinemachineImpulseSource _gunshotImpulseSource;
    
    private PlayerMovement _movement;
    private PlayerAiming _aiming;
    private PlayerShooting _shooting;
    private CharacterController _controller;
    private Quaternion _lastRotation;
    private float _turnSpeed;

    private readonly int _moveXHash = Animator.StringToHash("MoveX");
    private readonly int _moveYHash = Animator.StringToHash("MoveY");
    private readonly int _speedHash = Animator.StringToHash("Speed");
    private readonly int _turnSpeedHash = Animator.StringToHash("TurnSpeed");

    void Awake()
    {
        _controller = GetComponent<CharacterController>();
        var mainCamera = Camera.main;
        
        _aiming = new PlayerAiming(mainCamera, transform);
        _movement = new PlayerMovement(_controller, mainCamera.transform, moveSpeed, accelerationTime);
        _shooting = new PlayerShooting(transform, _firePoint, _bulletPrefab);
        
        if (_dashCooldownText != null) _dashCooldownText.gameObject.SetActive(false);
        _lastRotation = transform.rotation;
    }

    void Update()
    {
        _aiming.Tick();
        _movement.Tick();

        // --- SHOOTING LOGIC ---
        // This calculation is precise. The bullet direction is determined from the gun's muzzle
        // to the cursor's world position, flattened to the horizontal plane.
        Vector3 shootDirection = _aiming.AimPosition - _firePoint.position;
        shootDirection.y = 0; 
        _shooting.Tick(shootDirection.normalized);

        UpdateAnimator();
        UpdateDashCooldownUI();
        if (_movement.JustDashed && _dashImpulseSource != null) _dashImpulseSource.GenerateImpulse();
        if (Input.GetMouseButtonDown(0) && _gunshotImpulseSource != null) _gunshotImpulseSource.GenerateImpulse();
    }
    
    void LateUpdate()
    {
        // --- ROTATION LOGIC ---
        // The character ALWAYS rotates to face the cursor. No more conditional rotation.
        Vector3 lookDirection = _aiming.AimPosition - transform.position;
        lookDirection.y = 0;

        if (lookDirection.sqrMagnitude > 0.01f)
        {
            // Standard rotation to look towards a point.
            Quaternion targetLookRotation = Quaternion.LookRotation(lookDirection);
            
            // Apply the manual offset here to correct the model's alignment.
            Quaternion finalRotation = targetLookRotation * Quaternion.Euler(0, _rotationOffset, 0);

            // Smoothly rotate the character to the final, corrected rotation.
            transform.rotation = Quaternion.Slerp(transform.rotation, finalRotation, Time.deltaTime * _rotationSpeed);
        }
        
        CalculateTurnSpeed();
    }
    
    private void CalculateTurnSpeed(){ Quaternion currentRotation = transform.rotation; Quaternion deltaRotation = currentRotation * Quaternion.Inverse(_lastRotation); deltaRotation.ToAngleAxis(out float angle, out Vector3 axis); if (float.IsNaN(axis.x)) return; float turnDirection = Mathf.Sign(axis.y); float turnAnglePerSecond = (angle * turnDirection) / Time.deltaTime; _turnSpeed = Mathf.Lerp(_turnSpeed, Mathf.Clamp(turnAnglePerSecond / 360f, -1f, 1f), Time.deltaTime * 20f); _lastRotation = currentRotation; }
    private void UpdateAnimator(){ if (_animator == null) return; Vector3 horizontalVelocity = new Vector3(_controller.velocity.x, 0, _controller.velocity.z); float currentSpeed = horizontalVelocity.magnitude; Vector3 localVelocity = transform.InverseTransformDirection(horizontalVelocity); _animator.SetFloat(_speedHash, currentSpeed); _animator.SetFloat(_turnSpeedHash, _turnSpeed); _animator.SetFloat(_moveXHash, localVelocity.x / moveSpeed); _animator.SetFloat(_moveYHash, localVelocity.z / moveSpeed); }
    private void UpdateDashCooldownUI(){ if (_dashCooldownText == null) return; float cooldown = _movement.DashCooldownTimer; if (cooldown > 0) { _dashCooldownText.gameObject.SetActive(true); _dashCooldownText.text = cooldown.ToString("F1"); } else { _dashCooldownText.gameObject.SetActive(false); } }
}
