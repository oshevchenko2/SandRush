using UnityEngine;

public class PlayerShooting
{
    private readonly Transform _playerTransform;
    private readonly float _fireRate = 0.25f;
    private float _nextFireTime = 0f;

    private readonly WeaponBase _weapon;

    public PlayerShooting(Transform playerTransform, Transform firePoint, GameObject bulletPrefab)
    {
        _playerTransform = playerTransform;
        _weapon = new BulletWeapon(_playerTransform, firePoint, bulletPrefab);
    }

    public void Tick(Vector3 aimDir)
    {
        if (Input.GetMouseButton(0) && Time.time >= _nextFireTime)
        {
            _nextFireTime = Time.time + _fireRate;
            _weapon.Fire(aimDir);
        }
    }
}
