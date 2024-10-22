using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 5;   // Maximum health the player can have
    public int currentHealth;   // Current health of the player
    public bool IsDead { get; private set; }

    [Header("Health Bar Images")]
    public Image[] healthImages; // Array to hold references to health images (0/5 to 5/5)

    void Start()
    {
        currentHealth = maxHealth;
        Debug.Log("Player Health initialized. Current Health: " + currentHealth);
        UpdateHealthBar(); // Update the health bar UI
    }

    // Method to reduce the player's health
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth); // Clamp to ensure it doesn't go below 0
        Debug.Log("Player took " + damage + " damage. Current Health: " + currentHealth);

        UpdateHealthBar(); // Update the health bar UI after taking damage

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // Method to handle player death
    void Die()
    {
        IsDead = true;
        GetComponent<Collider2D>().enabled = false; // Disable collider
        Debug.Log("Player has died. Ending the game.");
        EndGame();
    }

    // Method to end the game
    private void EndGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); // Restart current scene for now
    }

    // Update the health bar UI based on the current health
    private void UpdateHealthBar()
    {
        // Loop through the health images and enable the correct one
        for (int i = 0; i < healthImages.Length; i++)
        {
            healthImages[i].enabled = (i == currentHealth);
        }
    }
}