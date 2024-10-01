using UnityEngine;

public class Punch : MonoBehaviour
{
    private Collider2D selectedEnemy; // Reference to the currently selected enemy
    private PlayerFatigue playerFatigue; // Reference to PlayerFatigue script
    private bool isPunching; // State to check if punch action is active
    public int punchDamage = 1; //Damage

    void Start()
    {
        playerFatigue = FindObjectOfType<PlayerFatigue>(); // Get PlayerFatigue component
        if (playerFatigue == null)
        {
            Debug.LogError("PlayerFatigue component not found in the scene!");
        }
    }

    // Call this method when the punch button is clicked
    public void OnPunchButtonClick()
    {
        isPunching = true; // Set the state to allow enemy selection
        Debug.Log("Punch button clicked! Now select an enemy to punch.");
    }

    void Update()
    {
        if (isPunching)
        {
            // Check for mouse input to select an enemy
            if (Input.GetMouseButtonDown(0)) // Left mouse button
            {
                RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
                if (hit.collider != null && hit.collider.CompareTag("Enemy"))
                {
                    selectedEnemy = hit.collider; // Set the selected enemy
                    Debug.Log($"Selected enemy: {selectedEnemy.gameObject.name}");
                    AttemptPunch(); // Try to punch the selected enemy
                }
                else
                {
                    Debug.Log("No enemy selected or the selected object is not tagged as 'Enemy'.");
                }
            }
        }
    }

    private void AttemptPunch()
    {
        if (playerFatigue != null && playerFatigue.currentFatigue > 0)
        {
            DealDamageToEnemy(selectedEnemy, punchDamage); // Deal damage
            playerFatigue.currentFatigue -= 1; // Reduce fatigue after punching
            Debug.Log($"Fatigue remaining: {playerFatigue.currentFatigue}");
        }
        else
        {
            Debug.LogWarning("Not enough fatigue to perform a punch action.");
        }

        // Reset punch state
        isPunching = false; // Disable enemy selection
        selectedEnemy = null; // Clear selected enemy
    }

    private void DealDamageToEnemy(Collider2D enemyCollider, int damage)
    {
        EnemyHealth enemyHealth = enemyCollider.GetComponent<EnemyHealth>(); // Get the EnemyHealth component

        if (enemyHealth != null)
        {
            enemyHealth.TakeDamage(damage); // Apply damage to the enemy
        }
        else
        {
            Debug.LogError("No EnemyHealth component found on the selected enemy!");
        }
    }
}
