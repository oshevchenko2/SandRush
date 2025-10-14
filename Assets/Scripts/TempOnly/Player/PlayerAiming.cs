using UnityEngine;

public class PlayerAiming
{
    private readonly Camera _cam;
    private readonly Transform _playerTransform;
    private readonly float _rotationSpeed = 10f;

    public Vector3 AimDirection { get; private set; }

    public PlayerAiming(Camera cam, Transform playerTransform)
    {
        _cam = cam;
        _playerTransform = playerTransform;
    }

    public void Tick()
    {
        Ray ray = _cam.ScreenPointToRay(Input.mousePosition);
        Plane ground = new(Vector3.up, Vector3.zero);

        if (ground.Raycast(ray, out float distance))
        {
            Vector3 target = ray.GetPoint(distance);
            Vector3 dir = target - _playerTransform.position;
            dir.y = 0f;
            AimDirection = dir.normalized;

            if (dir.sqrMagnitude > 0.001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(dir);
                _playerTransform.rotation = Quaternion.Slerp(_playerTransform.rotation, targetRot, _rotationSpeed * Time.deltaTime);
            }
        }
    }
}
