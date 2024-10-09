using UnityEngine;
using UnityEngine.Tilemaps;

public class Punch : MonoBehaviour
{
    public Tilemap tilemap; // Reference to the Tilemap
    private Transform selectedEnemy; // Currently selected enemy
    private bool isPunching; // State to track if we are in punch mode
    private PlayerMove playerMove; // Reference to PlayerMove instance
    private PlayerFatigue playerFatigue; // Reference to PlayerFatigue instance
    public int punchDamage = 1; // Damage dealt by punch

    private void Awake()
    {
        playerMove = GetComponent<PlayerMove>();
        playerFatigue = GetComponent<PlayerFatigue>();
    }

    void Update()
    {
        // Check for mouse input to select an enemy if punching
        if (Input.GetMouseButtonDown(0)) // Left mouse button
        {
            if (isPunching)
            {
                if (selectedEnemy != null)
                {
                    // Try to punch the selected enemy
                    TryPunchEnemy();
                }
                else
                {
                    // Select an enemy if none is currently selected
                    SelectEnemy();
                }
            }
        }
    }

    public void OnPunchButtonPressed()
    {
        isPunching = true; // Activate punching mode
        selectedEnemy = null; // Reset selected enemy
        playerMove.CurrentAction = ActionType.Punch; // Set the current action to Punch
        Debug.Log("Punch button pressed, current action: " + playerMove.CurrentAction);

        // Check if any enemies are in range to punch immediately
        CheckEnemiesInRange();
    }

    public void CancelPunch()
    {
        isPunching = false; // Deactivate punching mode
        selectedEnemy = null; // Reset selected enemy
        playerMove.CurrentAction = ActionType.None; // Reset current action
        Debug.Log("Punch action canceled.");
    }

    void SelectEnemy()
    {
        // Raycast to check if an enemy is clicked
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

        if (hit.collider != null)
        {
            // Check if the clicked object is tagged as "Enemy"
            if (hit.collider.CompareTag("Enemy"))
            {
                Vector3Int enemyPosition = tilemap.WorldToCell(hit.collider.transform.position);
                Vector3Int playerPosition = tilemap.WorldToCell(transform.position);

                // Ensure the enemy is within punching range (1 tile in each direction)
                if (IsWithinPunchRange(playerPosition, enemyPosition))
                {
                    selectedEnemy = hit.collider.transform; // Select the enemy
                    Debug.Log($"Selected enemy for punch: {selectedEnemy.name}");
                }
                else
                {
                    Debug.Log("Selected enemy is out of punch range.");
                }
            }
        }
    }

    void TryPunchEnemy()
    {
        if (selectedEnemy != null)
        {
            // Assume the enemy has a method to take damage
            EnemyHealth enemyScript = selectedEnemy.GetComponent<EnemyHealth>();
            if (enemyScript != null)
            {
                // Deal damage to the selected enemy
                enemyScript.TakeDamage(punchDamage); // Punch damage is set through Unity editor
                Debug.Log($"{selectedEnemy.name} has been punched and took {punchDamage} damage!");

                // Deduct fatigue only when a punch is successfully delivered
                playerFatigue.UseFatigue(playerFatigue.punchFatigueCost);
            }
            else
            {
                Debug.Log("Selected enemy does not have a valid damage method.");
            }

            // Reset punch state after attempting to punch
            isPunching = false;
            selectedEnemy = null; // Reset selected enemy after punch attempt
            playerMove.CurrentAction = ActionType.None; // Reset current action
        }
        else
        {
            Debug.Log("No enemy selected to punch.");
        }
    }

    bool IsWithinPunchRange(Vector3Int playerPosition, Vector3Int enemyPosition)
    {
        // Check if the enemy is within punching range (1 tile in each direction)
        return (Mathf.Abs(playerPosition.x - enemyPosition.x) + Mathf.Abs(playerPosition.y - enemyPosition.y) == 1);
    }

    private void CheckEnemiesInRange()
    {
        Vector3Int playerCurrentPosition = tilemap.WorldToCell(transform.position);

        // Check each enemy if it is within punching range
        foreach (GameObject enemyObj in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            Transform enemy = enemyObj.transform;
            Vector3Int enemyPosition = tilemap.WorldToCell(enemy.position);
            if (IsWithinPunchRange(playerCurrentPosition, enemyPosition))
            {
                Debug.Log($"{enemy.name} is within punch range!");
                selectedEnemy = enemy; // Automatically select the enemy in range
                break; // Exit loop after selecting the first found enemy
            }
        }
    }
}
