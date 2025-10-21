using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Bullet : MonoBehaviour
{
    [SerializeField] private GameObject _hitEffectPrefab;

    private Rigidbody _rb;
    private ObjectPool<Bullet> _pool;
    private readonly float _lifetime = 3f;
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

    private void OnCollisionEnter(Collision collision)
    {
        ContactPoint contact = collision.contacts[0];
        Quaternion rot = Quaternion.LookRotation(contact.normal);
        GameObject hitFx = Instantiate(_hitEffectPrefab, contact.point, rot);

        Destroy(hitFx, 1f);

        if (collision.collider.TryGetComponent<EnemyManager>(out var enemy))
        {
            enemy.TakeDamage(25f);
        }

        _pool.ReturnObject(this);
    }
}
