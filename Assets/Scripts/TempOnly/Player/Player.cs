using UnityEngine;
using Unity.Cinemachine;
using TMPro; // Ważne: dodajemy przestrzeń nazw dla TextMeshPro

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(CinemachineImpulseSource))]
public class Player : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private GameObject _bulletPrefab;
    [SerializeField] private Transform _firePoint;
    [Tooltip("Drag the TextMeshPro UI element for the dash cooldown here.")]
    [SerializeField] private TextMeshProUGUI _dashCooldownText;

    [Header("Movement Settings")]
    [Tooltip("Target movement speed.")]
    [SerializeField] private float moveSpeed = 7f;
    [Tooltip("Time it takes to reach full speed. Lower is faster.")]
    [SerializeField] private float accelerationTime = 0.1f;

    private PlayerMovement _movement;
    private PlayerAiming _aiming;
    private PlayerShooting _shooting;
    private CinemachineImpulseSource _impulseSource;

    void Awake()
    {
        var controller = GetComponent<CharacterController>();
        var mainCamera = Camera.main;
        _impulseSource = GetComponent<CinemachineImpulseSource>();

        if (mainCamera == null)
        {
            Debug.LogError("Main Camera is not found in the scene! Please tag your camera as 'MainCamera'.");
            return;
        }

        _movement = new PlayerMovement(controller, mainCamera.transform, moveSpeed, accelerationTime);
        _aiming = new PlayerAiming(mainCamera, transform);
        _shooting = new PlayerShooting(transform, _firePoint, _bulletPrefab);

        // Hide text at the start
        if (_dashCooldownText != null)
        {
            _dashCooldownText.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (_movement == null) return;

        // Tick all player systems
        _movement.Tick();
        _aiming.Tick();
        _shooting.Tick(_aiming.AimDirection);

        // Handle Cinemachine Impulse for dash shake
        if (_movement.JustDashed)
        {
            _impulseSource.GenerateImpulse();
        }

        // Update the cooldown UI
        UpdateDashCooldownUI();
    }

    private void UpdateDashCooldownUI()
    {
        if (_dashCooldownText == null) return;

        float cooldown = _movement.DashCooldownTimer;
        if (cooldown > 0)
        {
            _dashCooldownText.gameObject.SetActive(true);
            _dashCooldownText.text = cooldown.ToString("F1"); // Format to one decimal place
        }
        else
        {
            _dashCooldownText.gameObject.SetActive(false);
        }
    }
}
