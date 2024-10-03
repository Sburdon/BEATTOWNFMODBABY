using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField]
    public int maxHealth = 100; // Max health of the enemy
    [SerializeField]
    public int currentHealth; // Current health of the enemy
    private AIMove aiMoveScript; // Reference to the AIMove script
    private GameManager2 gameManager; // Reference to GameManager for respawning
    private Collider2D enemyCollider; // Reference to the enemy's collider
    private SpriteRenderer spriteRenderer; // Reference to the sprite renderer

    private void Start()
    {
        currentHealth = maxHealth; // Initialize current health
        aiMoveScript = GetComponent<AIMove>();
        gameManager = FindObjectOfType<GameManager2>();
        enemyCollider = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (aiMoveScript == null)
        {
            Debug.LogError("AIMove script not found on the enemy.");
        }

        if (enemyCollider == null)
        {
            Debug.LogError("Collider2D component not found on the enemy.");
        }

        if (spriteRenderer == null)
        {
            Debug.LogError("SpriteRenderer component not found on the enemy.");
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

        // Instead of disabling the entire GameObject, disable individual components
        if (aiMoveScript != null) aiMoveScript.enabled = false; // Disable AI movement
        if (enemyCollider != null) enemyCollider.enabled = false; // Disable the collider
        if (spriteRenderer != null) spriteRenderer.enabled = false; // Disable the sprite renderer

        // Respawn the AI after handling the death
        gameManager.RespawnAI(this.gameObject); // Trigger respawn
    }

    // Method for traps to instantly kill the enemy
    public void HitByTrap()
    {
        Debug.Log("Enemy hit a trap!");
        currentHealth = 0; // Kill the enemy
        Die(); // Trigger the death and respawn logic
    }

    // Method to reset health and re-enable components when the enemy respawns
    public void ResetHealth()
    {
        currentHealth = maxHealth; // Reset health to maximum

        // Re-enable components to restore the enemy's behavior
        if (aiMoveScript != null) aiMoveScript.enabled = true; // Enable AI movement
        if (enemyCollider != null) enemyCollider.enabled = true; // Enable the collider
        if (spriteRenderer != null) spriteRenderer.enabled = true; // Enable the sprite renderer

        Debug.Log("Enemy health reset to full: " + currentHealth);
    }
}
