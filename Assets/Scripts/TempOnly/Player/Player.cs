using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class Player : MonoBehaviour
{
    [SerializeField] private GameObject _bulletPrefab;
    [SerializeField] private Transform _firePoint;

    private PlayerMovement _movement;
    private PlayerAiming _aiming;
    private PlayerShooting _shooting;

    void Awake()
    {
        var controller = GetComponent<CharacterController>();
        var cam = Camera.main;

        _movement = new PlayerMovement(controller);
        _aiming = new PlayerAiming(cam, transform);
        _shooting = new PlayerShooting(transform, _firePoint, _bulletPrefab);
    }

    void Update()
    {
        _movement.Tick();
        _aiming.Tick();
        _shooting.Tick(_aiming.AimDirection);
    }
}
