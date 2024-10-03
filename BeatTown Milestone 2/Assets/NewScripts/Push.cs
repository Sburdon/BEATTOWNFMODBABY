using UnityEngine;

public class Push : MonoBehaviour
{
    private Collider2D selectedEnemy;
    private PlayerFatigue playerFatigue;
    private PlayerMove2 playerMove;
    private bool isSelectingEnemy = false;

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

                GridSpace playerGridSpace = playerMove.currentGridSpace; // Use player's current GridSpace script
                EnemyGridTracker enemyGridTracker = selectedEnemy.GetComponent<EnemyGridTracker>();

                if (enemyGridTracker != null)
                {
                    GridSpace enemyGridSpace = enemyGridTracker.GetCurrentGridSpace(); // Enemy's current GridSpace

                    Vector2Int direction = GetPushDirection(playerGridSpace, enemyGridSpace);
                    if (direction != Vector2Int.zero)
                    {
                        PushEnemy(enemyGridSpace, direction);
                        playerFatigue.currentFatigue -= 1;  // Deduct fatigue
                        Debug.Log("Enemy pushed.");
                    }
                    else
                    {
                        Debug.Log("Invalid push direction.");
                    }
                }
                else
                {
                    Debug.LogError("Enemy does not have an EnemyGridTracker script attached.");
                }
            }
        }
    }

    private Vector2Int GetPushDirection(GridSpace playerSpace, GridSpace enemySpace)
    {
        // Determine which side of the enemy the player is on
        if (playerSpace.X < enemySpace.X && playerSpace.Y == enemySpace.Y)
            return Vector2Int.right;  // Push right
        if (playerSpace.X > enemySpace.X && playerSpace.Y == enemySpace.Y)
            return Vector2Int.left;   // Push left
        if (playerSpace.Y < enemySpace.Y && playerSpace.X == enemySpace.X)
            return Vector2Int.up;     // Push up
        if (playerSpace.Y > enemySpace.Y && playerSpace.X == enemySpace.X)
            return Vector2Int.down;   // Push down

        return Vector2Int.zero;  // No valid push direction
    }

    private void PushEnemy(GridSpace enemySpace, Vector2Int direction)
    {
        GridSpace currentSpace = enemySpace;
        GridSpace nextSpace = null;

        while (true)
        {
            // Get the next space in the push direction
            Vector2Int nextPos = new Vector2Int(currentSpace.X + direction.x, currentSpace.Y + direction.y);
            nextSpace = FindGridSpaceAtPosition(nextPos);

            // Check if we found a valid next space
            if (nextSpace == null)
            {
                // We've reached the edge of the grid, so stop here
                break;
            }

            // Check the flags of the next space and stop if it's at an edge
            if ((direction == Vector2Int.right && nextSpace.RIGHTMOST) ||
                (direction == Vector2Int.left && nextSpace.LEFTMOST) ||
                (direction == Vector2Int.up && nextSpace.TOP) ||
                (direction == Vector2Int.down && nextSpace.BOTTOM))
            {
                // Stop moving the enemy at the edge
                currentSpace = nextSpace;
                break;
            }

            // Move to the next space
            currentSpace = nextSpace;
        }

        // Move the enemy to the final grid space
        selectedEnemy.transform.position = currentSpace.transform.position;
        Debug.Log("Enemy moved to grid position: " + currentSpace.X + "," + currentSpace.Y);
    }

    private GridSpace FindGridSpaceAtPosition(Vector2Int gridPos)
    {
        // Find the GridSpace that matches the target grid position
        foreach (GridSpace gridSpace in FindObjectsOfType<GridSpace>())
        {
            if (gridSpace.X == gridPos.x && gridSpace.Y == gridPos.y)
            {
                return gridSpace;
            }
        }
        return null;  // Return null if no matching GridSpace is found
    }
}
