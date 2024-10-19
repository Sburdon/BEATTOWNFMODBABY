using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Push : MonoBehaviour
{
    public Tilemap tilemap; // Reference to the Tilemap
    public Hook hook; // Reference to the Hook script
    private Transform selectedEnemy; // Currently selected enemy
    private bool isPushing; // State to track if we are in push mode
    private PlayerMove playerMove; // Reference to PlayerMove instance
    private PlayerFatigue playerFatigue; // Reference to PlayerFatigue instance

    private void Awake() // called before Start()
    {
        playerMove = GetComponent<PlayerMove>();
        playerFatigue = GetComponent<PlayerFatigue>();
    }

    void Update()
    {
        // Check for mouse input to select a tile if pushing
        if (Input.GetMouseButtonDown(0)) // Left mouse button
        {
            if (isPushing)
            {
                if (selectedEnemy != null)
                {
                    // Try to push the enemy
                    TryPushEnemy();
                }
                else
                {
                    // Select an enemy if none is currently selected
                    SelectEnemy();
                }
            }
        }
    }

    public void OnPushButtonPressed() // NOTE: PUSH BUTTON AGAIN TO CONFIRM PUSH
    {
        // Cancel any movement when the push button is pressed
        playerMove.CancelMove();

        isPushing = true; // Activate pushing mode
        selectedEnemy = null; // Reset selected enemy
        Debug.Log("Push button pressed, current action: " + playerMove.CurrentAction);
    }

    public void CancelPush()
    {
        isPushing = false;
        selectedEnemy = null; // Reset selected enemy after push attempt
        Debug.Log("Push action canceled.");
    }

    public bool IsPushing()
    {
        return isPushing;
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
                Transform enemy = hit.collider.transform;

                // Get the player's current tile position and the enemy's current tile position
                Vector3Int playerPosition = playerMove.CurrentTilePosition;
                Vector3Int enemyPosition = tilemap.WorldToCell(enemy.position);

                // Calculate the difference between the player's position and the enemy's position
                int deltaX = Mathf.Abs(enemyPosition.x - playerPosition.x);
                int deltaY = Mathf.Abs(enemyPosition.y - playerPosition.y);

                // Check if the enemy is exactly 1 tile away in one direction (up, down, left, or right)
                if ((deltaX + deltaY == 1)) // Either deltaX or deltaY must be 1, but not both
                {
                    // Enemy is within selection range
                    selectedEnemy = enemy; // Select the enemy
                    Debug.Log($"Selected enemy: {selectedEnemy.name}");
                }
                else
                {
                    Debug.Log("Enemy is not adjacent to the player (1 tile away in cardinal directions).");
                }
            }
        }
    }

    void TryPushEnemy()
    {
        if (selectedEnemy != null)
        {
            // Get the player's current position in grid coordinates
            Vector3Int playerPosition = playerMove.CurrentTilePosition;

            // Get the enemy's position in grid coordinates
            Vector3Int enemyPosition = tilemap.WorldToCell(selectedEnemy.position);

            // Determine the push direction (up, down, left, or right)
            Vector3Int direction = Vector3Int.zero;

            if (playerPosition.x < enemyPosition.x)
                direction = Vector3Int.right;  // Push right
            else if (playerPosition.x > enemyPosition.x)
                direction = Vector3Int.left;   // Push left
            else if (playerPosition.y < enemyPosition.y)
                direction = Vector3Int.up;     // Push up
            else if (playerPosition.y > enemyPosition.y)
                direction = Vector3Int.down;   // Push down

            // Find the furthest valid tile in the determined direction
            Vector3Int furthestTile = FindFurthestTile(enemyPosition, direction);

            // Move the enemy if the tile is valid
            if (furthestTile != enemyPosition)
            {
                // Remove the enemy's old position from occupied positions
                OccupiedTilesManager.Instance.RemoveOccupiedPosition(enemyPosition);

                StartCoroutine(PushEnemyToTile(selectedEnemy, furthestTile));
                playerFatigue.UseFatigue(playerFatigue.pushFatigueCost);  // Deduct fatigue for push
                selectedEnemy = null;
                isPushing = false;
            }
            else
            {
                Debug.Log("No valid tile to push to.");
            }
        }
        else
        {
            Debug.Log("No enemy selected for push.");
        }
    }

    Vector3Int FindFurthestTile(Vector3Int startTile, Vector3Int direction)
    {
        Vector3Int currentTile = startTile;
        while (IsTileValid(currentTile + direction))
        {
            currentTile += direction;  // Move to the next tile in the direction
        }
        return currentTile;  // Return the last valid tile
    }

    // Coroutine for smooth movement of the enemy
    private IEnumerator PushEnemyToTile(Transform enemy, Vector3Int targetTilePosition)
    {
        Vector3 startPosition = enemy.position;
        Vector3 targetPosition = tilemap.GetCellCenterWorld(targetTilePosition);
        float travelTime = 0.5f; // Set the duration of the travel
        float elapsedTime = 0f;

        EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();

        bool enemyDied = false;

        // Subscribe to the OnDeath event
        void OnEnemyDeath()
        {
            enemyDied = true;
        }

        if (enemyHealth != null)
        {
            enemyHealth.OnDeath += OnEnemyDeath;
        }

        // Move towards the target position over 'travelTime' seconds
        while (elapsedTime < travelTime)
        {
            if (enemyDied)
            {
                // Enemy has "died", stop movement
                Debug.Log("Enemy died during push. Stopping movement.");
                break;
            }

            enemy.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / travelTime);
            elapsedTime += Time.deltaTime;
            yield return null;  // Wait for the next frame
        }

        // Unsubscribe from the OnDeath event
        if (enemyHealth != null)
        {
            enemyHealth.OnDeath -= OnEnemyDeath;
        }

        // Check if enemy died
        if (enemyDied)
        {
            yield break;
        }

        enemy.position = targetPosition;  // Ensure the enemy ends at the target tile

        Debug.Log($"{enemy.name} has been pushed to {targetTilePosition}");

        // Update enemy's current tile position
        AIMove enemyMove = enemy.GetComponent<AIMove>();
        if (enemyMove != null)
        {
            enemyMove.CurrentTilePosition = targetTilePosition;
        }

        // Add the enemy's new position to occupied positions
        OccupiedTilesManager.Instance.AddOccupiedPosition(targetTilePosition);

        // After moving the enemy, check if it is on the same tile as the hook
        Vector3Int enemyTilePosition = targetTilePosition;
        Vector3Int hookTilePosition = tilemap.WorldToCell(hook.transform.position);

        if (enemyTilePosition == hookTilePosition)
        {
            // Enemy has been pushed into the hook
            hook.HandleSwingOrPushIntoHook(enemy.gameObject);
        }
    }

    bool IsTileValid(Vector3Int tilePosition)
    {
        // Check if the tile is within the tilemap
        bool hasTile = tilemap.HasTile(tilePosition);

        // Allow pushing into the hook's tile
        Vector3Int hookTilePosition = tilemap.WorldToCell(hook.transform.position);
        bool isOccupied = OccupiedTilesManager.Instance.IsTileOccupied(tilePosition);

        if (tilePosition == hookTilePosition)
        {
            isOccupied = false; // Allow moving into the hook tile
        }

        return hasTile && !isOccupied;
    }
}
