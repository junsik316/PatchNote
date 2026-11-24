using UnityEngine;

public class IDamagable : MonoBehaviour
{
    [SerializeField] private  float health = 100f;

    public void TakeDamage(float damage)
    {
        health -= damage;
        Debug.Log($"{gameObject.name} took {damage} damage. Remaining health: {health}");
        if (health <= 0)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        health += amount;
        Debug.Log($"{gameObject.name} healed {amount}. Current health: {health}");
    }

    public void Die()
    {
        Debug.Log($"{gameObject.name} has died.");
        // Add death logic here (e.g., play animation, disable object, etc.)
    }   
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
