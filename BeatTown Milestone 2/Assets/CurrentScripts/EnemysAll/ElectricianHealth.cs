using UnityEngine;
using UnityEngine.SceneManagement; // Needed for SceneManager.LoadScene

/// <summary>
/// A basic Health script for the Electrician.
/// If the Electrician dies, we load a "GameOver" scene.
/// </summary>
public class ElectricianHealth : MonoBehaviour
{
    [Header("Electrician Health Settings")]
    [Tooltip("Maximum health points for the Electrician.")]
    public int maxHealth = 5;

    [Tooltip("Current health points (auto-initialized to maxHealth on Start).")]
    public int currentHealth;

    private void Start()
    {
        currentHealth = maxHealth;
    }

    /// <summary>
    /// Deal 'damage' to the Electrician. If health falls to 0 or below, handle death.
    /// </summary>
    /// <param name="damage">Amount of damage to subtract.</param>
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
        Debug.Log("Electrician has died! Game Over!");

        // Disable the Electrician game object so it can't do anything else
        gameObject.SetActive(false);

        // Now load your "GameOver" scene (or do any other end-of-game logic)
        // Replace "GameOver" with whatever your actual scene name is:
        SceneManager.LoadScene("GameOver");
    }
}
