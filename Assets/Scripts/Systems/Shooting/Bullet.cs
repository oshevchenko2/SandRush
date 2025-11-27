using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Bullet : MonoBehaviour
{
    [Header("Bullet Stats")]
    [SerializeField] private float damage = 25f;
    [SerializeField] private GameObject _hitEffectPrefab;

    private Rigidbody _rb;
    private ObjectPool<Bullet> _pool;
    private readonly float _lifetime = 3f;
    private float _timer;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    public void Init(ObjectPool<Bullet> pool)
    {
        _pool = pool;
    }

    public void Fire(Vector3 dir, float speed)
    {
        _timer = _lifetime;
        if(_rb != null) 
        {
            _rb.linearVelocity = dir * speed;
        }
    }

    void Update()
    {
        _timer -= Time.deltaTime;
        if (_timer <= 0)
        {
            Deactivate();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // --- OSTATECZNA POPRAWKA: Użycie SendMessage ---
        // Wyślij "wiadomość" do trafionego obiektu, aby sam sobie zadał obrażenia.
        // Pocisk nie musi wiedzieć, co trafił.
        collision.collider.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
        
        // Stwórz efekt trafienia
        if (_hitEffectPrefab != null)
        {
            ContactPoint contact = collision.contacts[0];
            Quaternion rot = Quaternion.LookRotation(contact.normal);
            GameObject hitFx = Instantiate(_hitEffectPrefab, contact.point, rot);
            Destroy(hitFx, 1f);
        }

        Deactivate();
    }

    private void Deactivate()
    {
        if (_pool != null)
        {
            _pool.ReturnObject(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
