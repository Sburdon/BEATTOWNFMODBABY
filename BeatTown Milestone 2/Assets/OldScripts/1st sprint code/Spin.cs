//using UnityEngine;

//public class Spin : MonoBehaviour
//{
//    private PlayerMove2 playerMove; // Reference to the player's movement script
//    private PlayerFatigue playerFatigue; // Reference to PlayerFatigue script
//    private EnemyHealth selectedEnemy; // Reference to the selected enemy's health
//    private bool isSelectingEnemy = false; // Whether the player is selecting an enemy to spin
//    private bool isSelectingDirection = false; // Whether the player is selecting a direction to move the enemy

//    void Start()
//    {
//        playerMove = FindObjectOfType<PlayerMove2>();
//        playerFatigue = FindObjectOfType<PlayerFatigue>();

//        if (playerMove == null)
//        {
//            Debug.LogError("PlayerMove2 script not found!");
//        }
//        if (playerFatigue == null)
//        {
//            Debug.LogError("PlayerFatigue script not found!");
//        }
//    }

//    // Call this method when the spin button is clicked
//    public void OnSpinButtonClick()
//    {
//        if (playerFatigue.currentFatigue >= 2) // Ensure player has enough fatigue
//        {
//            isSelectingEnemy = true; // Start the process of selecting an enemy
//            Debug.Log("Spin action initiated: Select an adjacent enemy.");
//        }
//        else
//        {
//            Debug.LogWarning("Not enough fatigue to perform the spin action.");
//        }
//    }

//    void Update()
//    {
//        if (isSelectingEnemy && Input.GetMouseButtonDown(0)) // Select enemy with left mouse click
//        {
//            TrySelectEnemy();
//        }
//        else if (isSelectingDirection && selectedEnemy != null) // If an enemy is selected, allow direction input
//        {
//            TrySelectDirection();
//        }
//    }

//    // Method to try selecting an enemy that is adjacent to the player
//    private void TrySelectEnemy()
//    {
//        foreach (var enemy in FindObjectsOfType<EnemyHealth>())
//        {
//            Vector2Int enemyGridPos = enemy.GetComponent<AIMove>().currentGridPos;
//            Vector2Int playerGridPos = playerMove.currentGridPos;

//            if (IsAdjacent(playerGridPos, enemyGridPos)) // Check if the enemy is adjacent
//            {
//                selectedEnemy = enemy;
//                isSelectingEnemy = false;
//                isSelectingDirection = true; // Now wait for direction input
//                Debug.Log("Enemy selected: " + selectedEnemy.name);
//                break;
//            }
//        }
//    }

//    // Method to handle direction selection via arrow keys
//    private void TrySelectDirection()
//    {
//        Vector2Int playerGridPos = playerMove.currentGridPos;

//        if (Input.GetKeyDown(KeyCode.UpArrow)) // Up
//        {
//            MoveEnemyToGrid(selectedEnemy, new Vector2Int(playerGridPos.x, playerGridPos.y + 1));
//        }
//        else if (Input.GetKeyDown(KeyCode.DownArrow)) // Down
//        {
//            MoveEnemyToGrid(selectedEnemy, new Vector2Int(playerGridPos.x, playerGridPos.y - 1));
//        }
//        else if (Input.GetKeyDown(KeyCode.LeftArrow)) // Left
//        {
//            MoveEnemyToGrid(selectedEnemy, new Vector2Int(playerGridPos.x - 1, playerGridPos.y));
//        }
//        else if (Input.GetKeyDown(KeyCode.RightArrow)) // Right
//        {
//            MoveEnemyToGrid(selectedEnemy, new Vector2Int(playerGridPos.x + 1, playerGridPos.y));
//        }
//    }

//    // Move the enemy to the selected grid position
//    private void MoveEnemyToGrid(EnemyHealth enemy, Vector2Int targetGridPos)
//    {
//        AIMove enemyMove = enemy.GetComponent<AIMove>();

//        if (enemyMove != null)
//        {
//            enemyMove.MoveToGridPosition(targetGridPos); // Move enemy to the new grid position
//            playerFatigue.currentFatigue -= 2; // Deduct fatigue after the spin action
//            selectedEnemy = null; // Clear selected enemy
//            isSelectingDirection = false; // Reset selection process
//            Debug.Log("Enemy moved to: " + targetGridPos);
//        }
//        else
//        {
//            Debug.LogError("No AIMove component found on the selected enemy.");
//        }
//    }

//    // Check if two positions are adjacent
//    private bool IsAdjacent(Vector2Int pos1, Vector2Int pos2)
//    {
//        return (Mathf.Abs(pos1.x - pos2.x) == 1 && pos1.y == pos2.y) ||
//               (Mathf.Abs(pos1.y - pos2.y) == 1 && pos1.x == pos2.x);
//    }
//}
