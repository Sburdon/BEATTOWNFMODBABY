using UnityEngine;

/// <summary>
/// A basic Health script for the Electrician. 
/// You can expand or modify this to fit your health system.
/// </summary>
public class ElectricianHealth : MonoBehaviour
{
    [Header("Electrician Health Settings")]
    [Tooltip("Maximum health points for the Electrician.")]
    public int maxHealth = 5;

    [Tooltip("Current health points (auto-initialized to maxHealth on Start).")]
    public int currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
    }

    /// <summary>
    /// Deal 'damage' to the Electrician. If health falls to 0, you can handle a death event.
    /// </summary>
    /// <param name="damage">The amount of damage to subtract.</param>
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log($"Electrician took {damage} damage. Current HP = {currentHealth}.");

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
    }

    private void Die()
    {
        // Put any "Electrician died" logic here
        // e.g., disable them, remove from TurnBase, show game over, etc.
        Debug.Log("Electrician has died!");
        // Just disable for now
        gameObject.SetActive(false);
    }
}
