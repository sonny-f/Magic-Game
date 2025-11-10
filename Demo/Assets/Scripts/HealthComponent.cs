using UnityEngine;

public class HealthComponent : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damageToApply)
    {
        currentHealth -= damageToApply;

        Debug.Log(currentHealth);

        if (currentHealth <= 0)
        {
            Destroy(this.gameObject);
        }
    }

}
