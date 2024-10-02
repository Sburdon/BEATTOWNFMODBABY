using UnityEngine;

public class Push : MonoBehaviour
{
    private Collider2D selectedEnemy;
    private PlayerFatigue playerFatigue;
    private PlayerMove2 playerMove;
    private bool isSelectingEnemy = false;
    // private GameManager2 gameManager;

    void Start()
    {
        playerFatigue = FindObjectOfType<PlayerFatigue>();
        playerMove = FindObjectOfType<PlayerMove2>();

        if (playerFatigue == null || playerMove == null)
        {
            Debug.LogError("PlayerFatigue or PlayerMove2 not found.");
        }
    }

    // Call this method when the Push button is clicked
    public void OnPushButtonClick()
    {
        if (playerFatigue != null && playerFatigue.currentFatigue > 0)
        {
            isSelectingEnemy = true;  // Start the enemy selection process
            Debug.Log("Select an enemy to push.");
        }
        else
        {
            Debug.LogWarning("Not enough fatigue to perform the push action.");
        }
    }

    void Update()
    {
        // If the player is selecting an enemy to push
        if (isSelectingEnemy && Input.GetMouseButtonDown(0))
        {
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

            if (hit.collider != null && hit.collider.CompareTag("Enemy"))
            {
                selectedEnemy = hit.collider;
                isSelectingEnemy = false;

                Vector2Int playerPos = playerMove.currentGridPos;
                Vector2Int enemyPos = GetGridPositionFromCollider(selectedEnemy);

                Vector2Int direction = GetPushDirection(playerPos, enemyPos);
                if (direction != Vector2Int.zero)
                {
                    PushEnemy(enemyPos, direction);
                    playerFatigue.currentFatigue -= 1;  // Deduct fatigue
                    Debug.Log("Enemy pushed.");
                }
                else
                {
                    Debug.Log("Invalid push direction.");
                }
            }
        }
    }

    private Vector2Int GetPushDirection(Vector2Int playerPos, Vector2Int enemyPos)
    {
        // Determine which side of the enemy the player is on
        if (playerPos.x < enemyPos.x && playerPos.y == enemyPos.y)
            return Vector2Int.right;  // Push right
        if (playerPos.x > enemyPos.x && playerPos.y == enemyPos.y)
            return Vector2Int.left;   // Push left
        if (playerPos.y < enemyPos.y && playerPos.x == enemyPos.x)
            return Vector2Int.up;     // Push up
        if (playerPos.y > enemyPos.y && playerPos.x == enemyPos.x)
            return Vector2Int.down;   // Push down

        return Vector2Int.zero;  // No valid push direction
    }

    private void PushEnemy(Vector2Int enemyPos, Vector2Int direction)
    {
        Vector2Int targetPos = enemyPos;

        // Keep pushing the enemy until they hit the edge of the grid
        while (true)
        {
            Vector2Int nextPos = targetPos + direction;

            // Check if the next position is within grid bounds
            if (nextPos.x < 1 || nextPos.x > 5 || nextPos.y < 1 || nextPos.y > 5)
                break;  // Stop when we hit the edge of the grid

            // Update target position
            targetPos = nextPos;
        }

        // Move the enemy to the final target position
        selectedEnemy.transform.position = playerMove.gameManager.gridSquares[targetPos].transform.position;
        Debug.Log("Enemy moved to grid position: " + targetPos);
    }

    private Vector2Int GetGridPositionFromCollider(Collider2D collider)
    {
        // Similar logic to Spin.GetGridPositionFromCollider
        string colliderName = collider.name;  // e.g., "Square (3, 2)"
        string[] parts = colliderName.Replace("Square (", "").Replace(")", "").Split(',');

        int x = int.Parse(parts[0].Trim());
        int y = int.Parse(parts[1].Trim());

        return new Vector2Int(x, y);
    }
}
