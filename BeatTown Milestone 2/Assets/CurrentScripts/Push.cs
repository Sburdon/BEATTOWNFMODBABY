using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Push : MonoBehaviour
{
    public Tilemap tilemap; // Reference to the Tilemap
    private Transform selectedEnemy; // Currently selected enemy
    private bool isPushing; // State to track if we are in push mode
    private PlayerMove playerMove; // Reference to PlayerMove instance
    private PlayerFatigue playerFatigue; // Reference to PlayerFatigue instance

    private void Awake()
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
                    // Try to push the enemy to the clicked tile
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

    public void OnPushButtonPressed()
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
        // Implement push logic here
    }

    
    
    // Coroutine for smooth movement of the enemy
    private IEnumerator PushEnemyToTile(Transform enemy, Vector3Int targetTilePosition)
    {
        Vector3 startPosition = enemy.position;
        Vector3 targetPosition = tilemap.GetCellCenterWorld(targetTilePosition);
        float travelTime = 0.5f; // Set the duration of the travel
        float elapsedTime = 0f;

        // Move towards the target position over 'travelTime' seconds
        
        // Ensure the enemy ends up exactly at the target position
        // Enemy should end up at the furthest tile possible in the direction of the push

        Debug.Log($"{enemy.name} has been pushed to {targetTilePosition}");
    }

    bool IsTileValid(Vector3Int tilePosition)
    {
        // Check if the tile is a valid, empty tile (e.g., not occupied by another enemy or blocked)
        return tilemap.HasTile(tilePosition) && !playerMove.IsTileOccupied(tilePosition);
    }

}
