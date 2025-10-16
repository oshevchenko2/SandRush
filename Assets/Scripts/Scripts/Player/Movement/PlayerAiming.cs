using UnityEngine;

// This class calculates the aim direction from the player's root transform for stability.
public class PlayerAiming
{
    private readonly Camera _cam;
    private readonly Transform _playerTransform;

    public Vector3 AimDirection { get; private set; }
    public Vector3 AimPosition { get; private set; }

    public PlayerAiming(Camera cam, Transform playerTransform)
    {
        _cam = cam;
        _playerTransform = playerTransform;
    }

    public void Tick()
    {
        Plane groundPlane = new Plane(Vector3.up, _playerTransform.position);
        Ray ray = _cam.ScreenPointToRay(Input.mousePosition);

        if (groundPlane.Raycast(ray, out float enter))
        {
            AimPosition = ray.GetPoint(enter);
            AimDirection = (AimPosition - _playerTransform.position).normalized;
        }
    }
}
