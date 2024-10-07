using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 5;   // Maximum health the player can have
    public int currentHealth;   // Current health of the player

    void Start()
    {
        currentHealth = maxHealth;
        Debug.Log("Player Health initialized. Current Health: " + currentHealth);
    }

    // Method to reduce the player's health
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log("Player took " + damage + " damage. Current Health: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // Method to handle player death
    void Die()
    {
        Debug.Log("Player has died.");
        // Implement death logic here (e.g., reload scene, show game over screen)
    }
}
