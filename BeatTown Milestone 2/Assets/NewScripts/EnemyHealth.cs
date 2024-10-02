using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField]
    private int maxHealth = 100; // Max health of the enemy
    [SerializeField]
    private int currentHealth; // Current health of the enemy

    private void Start()
    {
        currentHealth = maxHealth; // Initialize current health
    }

    // Method to apply damage to the enemy
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0); // Ensure health doesn't go below zero
        Debug.Log("Enemy took damage! Current health: " + currentHealth);
    }
}
