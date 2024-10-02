using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Punch : MonoBehaviour
{
    private Collider2D selectedEnemy; // Reference to the currently selected enemy
    private PlayerFatigue playerFatigue; // Reference to PlayerFatigue script
    private PlayerMove2 playerMove; // Reference to PlayerMove2 script
    private PlayerRange playerRange; // Reference to PlayerRange script
    private bool isPunching; // State to check if punch action is active
    public int punchDamage = 1; // Damage

    void Start()
    {
        playerFatigue = FindObjectOfType<PlayerFatigue>(); // Get PlayerFatigue component
        playerMove = FindObjectOfType<PlayerMove2>(); // Get PlayerMove2 component
        playerRange = FindObjectOfType<PlayerRange>(); // Get PlayerRange component

        if (playerFatigue == null)
        {
            Debug.LogError("PlayerFatigue component not found in the scene!");
        }
        if (playerMove == null)
        {
            Debug.LogError("PlayerMove2 component not found in the scene!");
        }
        if (playerRange == null)
        {
            Debug.LogError("PlayerRange component not found in the scene!");
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
        // Check if the selected enemy is in range using PlayerRange
        if (selectedEnemy != null && playerRange.IsEnemyInRange(selectedEnemy))
        {
            // Logic for damaging the enemy using the EnemyHealth script
            EnemyHealth enemyHealth = selectedEnemy.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(punchDamage);
                Debug.Log($"Punched {selectedEnemy.name} for {punchDamage} damage!");
                playerFatigue.currentFatigue -= 1; // Deduct fatigue
            }
            else
            {
                Debug.LogError("Enemy does not have an EnemyHealth component!");
            }
        }
        else
        {
            if (selectedEnemy == null)
            {
                Debug.LogWarning("No enemy selected to punch!");
            }
            else
            {
                Debug.LogWarning("Selected enemy is out of range to punch!");
            }
        }

        isPunching = false; // Reset the punching state
        selectedEnemy = null; // Clear selected enemy
    }
}
