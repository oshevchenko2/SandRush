using UnityEngine;

public class PlayerMovement
{
    // --- Settings ---
    private readonly float _moveSpeed;
    private readonly float _accelerationTime;

    // --- Dash Settings ---
    private readonly float _dashSpeed = 30f;
    private readonly float _dashDuration = 0.1f;
    private readonly float _dashCooldown = 1f;

    // --- References ---
    private readonly CharacterController _controller;
    private readonly Transform _transform;
    private readonly Transform _cameraTransform;

    // --- State ---
    private Vector3 _currentMoveVelocity;
    private Vector3 _velocityDamper; // Used for Vector3.SmoothDamp
    private float _dashCooldownTimer;
    private float _dashTimer;
    private bool _isDashing;
    private Vector3 _dashDirection;

    // --- PUBLIC STATE FOR SHAKE & UI ---
    public bool JustDashed { get; private set; }
    public float DashCooldownTimer => _dashCooldownTimer; // Expose the timer for UI

    public PlayerMovement(CharacterController controller, Transform cameraTransform, float moveSpeed, float accelerationTime)
    {
        _controller = controller;
        _transform = controller.transform;
        _cameraTransform = cameraTransform;
        _moveSpeed = moveSpeed;
        _accelerationTime = accelerationTime;
    }

    public void Tick()
    {
        // Reset the flag at the beginning of each frame
        JustDashed = false; 

        Vector3 inputDirection = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));

        HandleDashState(inputDirection);

        if (_isDashing)
        {
            _controller.Move(_dashDirection * _dashSpeed * Time.deltaTime);
            return;
        }

        HandleMovement(inputDirection);
    }

    private void HandleMovement(Vector3 inputDirection)
    {
        Vector3 targetVelocity;

        if (inputDirection.magnitude >= 0.1f)
        {
            Vector3 camForward = _cameraTransform.forward;
            Vector3 camRight = _cameraTransform.right;
            camForward.y = 0;
            camRight.y = 0;
            camForward.Normalize();
            camRight.Normalize();
            
            Vector3 targetDirection = (camForward * inputDirection.z + camRight * inputDirection.x).normalized;
            targetVelocity = targetDirection * _moveSpeed;
        }
        else
        {
            targetVelocity = Vector3.zero;
        }

        _currentMoveVelocity = Vector3.SmoothDamp(_currentMoveVelocity, targetVelocity, ref _velocityDamper, _accelerationTime);
        _controller.Move(_currentMoveVelocity * Time.deltaTime);
    }

    private void HandleDashState(Vector3 inputDirection)
    {
        if (_dashCooldownTimer > 0) _dashCooldownTimer -= Time.deltaTime;
        if (_dashTimer > 0)
        {
            _dashTimer -= Time.deltaTime;
            if (_dashTimer <= 0) _isDashing = false;
        }

        if (Input.GetKeyDown(KeyCode.Space) && _dashCooldownTimer <= 0)
        {
            StartDash(inputDirection);
        }
    }

    private void StartDash(Vector3 inputDirection)
    {
        // SET THE FLAG!
        JustDashed = true; 
        
        _isDashing = true;
        _dashCooldownTimer = _dashCooldown;
        _dashTimer = _dashDuration;

        if (inputDirection.magnitude >= 0.1f)
        {
            Vector3 camForward = _cameraTransform.forward;
            Vector3 camRight = _cameraTransform.right;
            camForward.y = 0;
            camRight.y = 0;
            camForward.Normalize();
            camRight.Normalize();
            _dashDirection = (camForward * inputDirection.z + camRight * inputDirection.x).normalized;
        }
        else
        {
            _dashDirection = _transform.forward;
        }
    }
}
