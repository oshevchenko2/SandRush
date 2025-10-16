using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }

    private Transform _transform;
    private Vector3 _originalPosition;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }

        _transform = GetComponent<Transform>();
    }

    private void OnEnable()
    {
        // Store the original position when the script is enabled.
        _originalPosition = _transform.localPosition;
    }

    public void Shake(float duration = 0.15f, float magnitude = 0.2f)
    {
        // We use localPosition to not interfere with any camera-following logic.
        _originalPosition = _transform.localPosition;
        StartCoroutine(ShakeCoroutine(duration, magnitude));
    }

    private IEnumerator ShakeCoroutine(float duration, float magnitude)
    {
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            // Generate a random point inside a sphere of radius 1, scaled by magnitude.
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            _transform.localPosition = _originalPosition + new Vector3(x, y, 0);

            elapsed += Time.deltaTime;

            // Wait for the next frame.
            yield return null;
        }

        // Reset the position after the shake is complete.
        _transform.localPosition = _originalPosition;
    }
}
