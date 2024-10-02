using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField]
    private int maxHealth = 100; // Max health of the enemy
    [SerializeField]
    private int currentHealth; // Current health of the enemy

    private AIMove aiMoveScript; // Reference to the AIMove script

    private void Start()
    {
        currentHealth = maxHealth; // Initialize current health
        aiMoveScript = GetComponent<AIMove>();

        if (aiMoveScript == null)
        {
            Debug.LogError("AIMove script not found on the enemy.");
        }
    }

    // Method to apply damage to the enemy
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0); // Ensure health doesn't go below zero
        Debug.Log("Enemy took damage! Current health: " + currentHealth);

        // Set the counter to follow the player for the next two turns
        if (aiMoveScript != null)
        {
            aiMoveScript.SetFollowPlayerTurns(2);
            Debug.Log("Enemy will follow the player for the next two turns.");
        }

        // Handle enemy death
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("Enemy has died.");
        // Implement death logic here (e.g., destroy enemy GameObject, update game state)
        Destroy(gameObject);
    }
}
