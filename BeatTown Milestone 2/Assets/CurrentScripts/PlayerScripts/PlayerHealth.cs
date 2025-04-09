using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static StateMachine;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 5;   // Maximum health the player can have
    public int currentHealth;   // Current health of the player
    private StateMachine stateMachine;
    public bool IsDead { get; private set; }
    public All_SFX All_SFX;

    [Header("Health Bar Images")]
    public Image[] healthImages; // Array to hold references to health images (0/5 to 5/5)

    void Start()
    {
        stateMachine = GetComponent<StateMachine>();
        currentHealth = maxHealth;
        Debug.Log("Player Health initialized. Current Health: " + currentHealth);
        UpdateHealthBar(); // Update the health bar UI
    }

    // Method to reduce the player's health
    public void TakeDamage(int damage)
    {
        stateMachine.ChangeState(WrestlerState.React);
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth); // Clamp to ensure it doesn't go below 0
        Debug.Log("Player took " + damage + " damage. Current Health: " + currentHealth);
        if (CameraShake.Instance != null)
            CameraShake.Instance.StartShake(0.2f, 0.3f);
        All_SFX.PlayPlayerHurt();

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
        SceneManager.LoadScene(0); // Restart current scene for now
    }
    private IEnumerator FadeOutHeart(Image heartImage)
    {
        float fadeSpeed = 5f;

        // Step 1: Fade to partial transparency
        yield return StartCoroutine(FadeToAlpha(heartImage, 0.4f, fadeSpeed));

        // Step 2: Fade back to full opacity
        yield return StartCoroutine(FadeToAlpha(heartImage, 1f, fadeSpeed));

        yield return StartCoroutine(FadeToAlpha(heartImage, 0.4f, fadeSpeed));

        // Step 2: Fade back to full opacity
        yield return StartCoroutine(FadeToAlpha(heartImage, 1f, fadeSpeed));

        yield return StartCoroutine(FadeToAlpha(heartImage, 0.4f, fadeSpeed));

        // Step 2: Fade back to full opacity
        yield return StartCoroutine(FadeToAlpha(heartImage, 1f, fadeSpeed));

        // Step 3: Fade all the way out
        yield return StartCoroutine(FadeToAlpha(heartImage, 0f, fadeSpeed));

        heartImage.enabled = false; // Turn it off completely
    }
    private IEnumerator FadeToAlpha(Image image, float targetAlpha, float speed)
    {
        Color color = image.color;
        float startAlpha = color.a;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * speed;
            float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            image.color = new Color(color.r, color.g, color.b, newAlpha);
            yield return null;
        }

        // Ensure final alpha is set exactly
        image.color = new Color(color.r, color.g, color.b, targetAlpha);
    }

    // Update the health bar UI based on the current health
    private void UpdateHealthBar()
    {
        for (int i = 0; i < healthImages.Length; i++)
        {
            if (i < currentHealth)
            {
                healthImages[i].enabled = true;
                SetAlpha(healthImages[i], 1f); // Reset to full visibility
            }
            else if (healthImages[i].enabled) // Only fade out if it's currently on
            {
                StartCoroutine(FadeOutHeart(healthImages[i]));
            }
        }
    }
    private void SetAlpha(Image image, float alpha)
    {
        Color c = image.color;
        image.color = new Color(c.r, c.g, c.b, alpha);
    }

}