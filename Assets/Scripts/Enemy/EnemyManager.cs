using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    [SerializeField] private float _maxHealth = 100f;

    private float _currentHealth;
    private bool _isDead;

    void Awake()
    {
        _currentHealth = _maxHealth;
    }

    public void TakeDamage(float amount)
    {
        if (_isDead) return;

        _currentHealth -= amount;

        Debug.Log($"{gameObject.name} taking {amount} damage. HP = {_currentHealth}");

        if (_currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        _isDead = true;
        Debug.Log($"{gameObject.name} is dead!");

        gameObject.SetActive(false);
    }
}
