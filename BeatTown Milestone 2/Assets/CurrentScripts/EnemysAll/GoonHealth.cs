using UnityEngine;

public class GoonHealth : MonoBehaviour
{
    public int maxHealth = 3;
    private int currentHealth;
    private GoonMove goonMove;

    private void Awake()
    {
        currentHealth = maxHealth;
        goonMove = GetComponent<GoonMove>();
    }

    /// <summary>
    /// Reduces the Goon's health when damaged.
    /// </summary>
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log($"{name} took {damage} damage! Current health: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // If the Goon is charging a punch, update its target to the player's position.
            if (goonMove != null)
            {
                goonMove.OnPunchedByPlayer();
            }
        }
    }

    /// <summary>
    /// Handles the Goon's death.
    /// </summary>
    private void Die()
    {
        Debug.Log($"{name} has died!");
        Destroy(gameObject); 
    }
}
