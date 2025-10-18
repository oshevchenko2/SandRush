using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Bullet : MonoBehaviour
{
    private Rigidbody _rb;
    private ObjectPool<Bullet> _pool;
    private float _lifetime = 3f;
    private float _timer;

    public void Init(ObjectPool<Bullet> pool)
    {
        _pool = pool;
        _rb = GetComponent<Rigidbody>();
    }

    public void Fire(Vector3 dir, float speed)
    {
        _timer = _lifetime;
        _rb.linearVelocity = dir * speed;
    }

    void Update()
    {
        _timer -= Time.deltaTime;
        if (_timer <= 0)
        {
            _pool.ReturnObject(this);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            Debug.Log("Hit enemy: " + other.name);
        }

        _pool.ReturnObject(this);
    }
}
