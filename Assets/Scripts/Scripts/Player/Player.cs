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

    [Header("Aiming IK Settings")]
    [Range(0, 1)]
    [Tooltip("How much the body (spine) contributes to aiming.")]
    [SerializeField] private float _aimBodyWeight = 0.4f;
    [Range(0, 1)]
    [Tooltip("How much the head contributes to aiming. Set to 1 for perfect head tracking.")]
    [SerializeField] private float _aimHeadWeight = 1f;
    [Tooltip("The angle at which the whole body will start to turn.")]
    [SerializeField] private float _maxAimAngle = 80f;
    
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
        
        // Aiming system uses the stable root transform for its calculations.
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
        _shooting.Tick(_aiming.AimDirection);
        UpdateAnimator();
        UpdateDashCooldownUI();
        if (_movement.JustDashed && _dashImpulseSource != null) _dashImpulseSource.GenerateImpulse();
        if (Input.GetMouseButtonDown(0) && _gunshotImpulseSource != null) _gunshotImpulseSource.GenerateImpulse();
    }
    
    void LateUpdate()
    {
        float angle = Vector3.SignedAngle(transform.forward, _aiming.AimDirection, Vector3.up);
        if (Mathf.Abs(angle) > _maxAimAngle)
        {
            Quaternion targetRotation = Quaternion.LookRotation(_aiming.AimDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 15f);
        }
        CalculateTurnSpeed();
    }

    // This is the clean, standard Unity way to make a character look at a target.
    private void OnAnimatorIK(int layerIndex)
    {
        if (_animator == null || _aiming == null) return;

        // Set how much the body, head, and eyes will be influenced by the IK.
        _animator.SetLookAtWeight(1f, _aimBodyWeight, _aimHeadWeight, 1f, 0.5f);
        
        // --- THE DEFINITIVE FIX ---

        // 1. Find the head bone. If it doesn't exist, do nothing.
        Transform headBone = _animator.GetBoneTransform(HumanBodyBones.Head);
        if (headBone == null) return;

        // 2. Create a target point far away, but starting from the HEAD's position, not the feet.
        // This creates a sightline at the correct height, forcing a perfect horizontal aim.
        Vector3 lookAtTarget = headBone.position + (_aiming.AimDirection * 100f);

        // 3. Tell the Animator to look at this point. It will handle rotating the
        // head and spine bones automatically to achieve the correct look.
        _animator.SetLookAtPosition(lookAtTarget);
    }
    
    
    private void CalculateTurnSpeed(){ Quaternion currentRotation = transform.rotation; Quaternion deltaRotation = currentRotation * Quaternion.Inverse(_lastRotation); deltaRotation.ToAngleAxis(out float angle, out Vector3 axis); if (float.IsNaN(axis.x)) return; float turnDirection = Mathf.Sign(axis.y); float turnAnglePerSecond = (angle * turnDirection) / Time.deltaTime; _turnSpeed = Mathf.Lerp(_turnSpeed, Mathf.Clamp(turnAnglePerSecond / 360f, -1f, 1f), Time.deltaTime * 20f); _lastRotation = currentRotation; }
    private void UpdateAnimator(){ if (_animator == null) return; Vector3 horizontalVelocity = new Vector3(_controller.velocity.x, 0, _controller.velocity.z); float currentSpeed = horizontalVelocity.magnitude; Vector3 localVelocity = transform.InverseTransformDirection(horizontalVelocity); _animator.SetFloat(_speedHash, currentSpeed); _animator.SetFloat(_turnSpeedHash, _turnSpeed); _animator.SetFloat(_moveXHash, localVelocity.x / moveSpeed); _animator.SetFloat(_moveYHash, localVelocity.z / moveSpeed); }
    private void UpdateDashCooldownUI(){ if (_dashCooldownText == null) return; float cooldown = _movement.DashCooldownTimer; if (cooldown > 0) { _dashCooldownText.gameObject.SetActive(true); _dashCooldownText.text = cooldown.ToString("F1"); } else { _dashCooldownText.gameObject.SetActive(false); } }
}
