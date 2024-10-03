using UnityEngine;

public class Punch : MonoBehaviour
{
    private PlayerFatigue playerFatigue; // Reference to PlayerFatigue script
    private PlayerMove2 playerMove; // Reference to PlayerMove2 script
    public int punchDamage = 1; // Damage
    private bool isPunching = false; // State to check if punch action is active

    void Start()
    {
        playerFatigue = FindObjectOfType<PlayerFatigue>(); // Get PlayerFatigue component
        playerMove = FindObjectOfType<PlayerMove2>(); // Get PlayerMove2 component

        if (playerFatigue == null)
        {
            Debug.LogError("PlayerFatigue component not found!");
        }
        if (playerMove == null)
        {
            Debug.LogError("PlayerMove2 component not found!");
        }
    }

    // Call this method when the punch button is clicked
    public void OnPunchButtonClick()
    {
        if (playerFatigue != null && playerFatigue.currentFatigue > 0)
        {
            isPunching = true; // Set the state to allow punch action
            Debug.Log("Punch mode activated. Select an enemy to punch.");
        }
        else
        {
            Debug.LogWarning("Not enough fatigue to punch.");
        }
    }

    void Update()
    {
        if (isPunching && Input.GetMouseButtonDown(0)) // Left-click to select an enemy
        {
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
            if (hit.collider != null && hit.collider.CompareTag("Enemy"))
            {
                EnemyHealth enemyHealth = hit.collider.GetComponent<EnemyHealth>();
                if (enemyHealth != null)
                {
                    Vector2Int playerPos = playerMove.currentGridPos; // Get player's grid position
                    Vector2Int enemyPos = hit.collider.GetComponent<AIMove>().currentGridPos; // Get enemy's grid position

                    if (IsAdjacent(playerPos, enemyPos)) // Check if the enemy is adjacent to the player
                    {
                        PunchEnemy(enemyHealth);
                    }
                    else
                    {
                        Debug.LogWarning("Enemy is not adjacent to the player.");
                    }
                }
            }
            else
            {
                Debug.LogWarning("No valid enemy selected.");
            }

            isPunching = false; // Reset punch state
        }
    }

    private void PunchEnemy(EnemyHealth enemyHealth)
    {
        // Apply damage to the enemy
        enemyHealth.TakeDamage(punchDamage);
        playerFatigue.currentFatigue -= 1; // Reduce fatigue after punching
        Debug.Log("Punched enemy for " + punchDamage + " damage.");
    }

    // Check if the enemy is adjacent to the player
    private bool IsAdjacent(Vector2Int playerPos, Vector2Int enemyPos)
    {
        return (Mathf.Abs(playerPos.x - enemyPos.x) == 1 && playerPos.y == enemyPos.y) ||
               (Mathf.Abs(playerPos.y - enemyPos.y) == 1 && playerPos.x == enemyPos.x);
    }
}
