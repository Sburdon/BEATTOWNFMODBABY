using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;

public class Swing : MonoBehaviour
{
    public Tilemap tilemap; // Reference to the Tilemap
    private Transform selectedEnemy; // Currently selected enemy
    private bool isSwinging; // State to track if we are in swing mode
    private PlayerMove playerMove; // Reference to PlayerMove instance

    void Start()
    {
        tilemap = tilemap ?? FindObjectOfType<Tilemap>(); // Finds the first Tilemap in the scene
        playerMove = playerMove ?? GetComponent<PlayerMove>(); // Ensure PlayerMove is assigned
        if (playerMove == null)
        {
            Debug.LogError("PlayerMove component missing on player prefab.");
        }
    }

    void Update()
    {
        if (tilemap == null) return;

        // Check for mouse input to select a tile if swinging
        if (Input.GetMouseButtonDown(0) && isSwinging) // Left mouse button
        {
            if (selectedEnemy != null)
            {
                TrySwingEnemy(); // Try to swing the selected enemy
            }
            else
            {
                SelectEnemy(); // Select an enemy if none is currently selected
            }
        }
    }

    public void OnSwingButtonPressed()
    {
        if (tilemap == null) return;

        // Cancel any movement when the swing button is pressed
        playerMove.CancelMove(); // Cancel movement if swing is initiated

        isSwinging = true; // Activate swinging mode
        selectedEnemy = null; // Reset selected enemy
        Debug.Log("Swing button pressed, current action: " + playerMove.CurrentAction);
    }

    public void CancelSwing()
    {
        isSwinging = false;
        selectedEnemy = null; // Reset selected enemy after swing attempt
        Debug.Log("Swing action canceled.");
    }

    public bool IsSwinging()
    {
        return isSwinging;
    }

    void SelectEnemy()
    {
        if (tilemap == null) return;

        // Raycast to check if an enemy is clicked
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

        if (hit.collider != null && hit.collider.CompareTag("Enemy"))
        {
            Transform enemy = hit.collider.transform;

            // Get the player's current tile position and the enemy's current tile position
            Vector3Int playerPosition = playerMove.CurrentTilePosition;
            Vector3Int enemyPosition = tilemap.WorldToCell(enemy.position);

            // Calculate the difference between the player's position and the enemy's position
            int deltaX = Mathf.Abs(enemyPosition.x - playerPosition.x);
            int deltaY = Mathf.Abs(enemyPosition.y - playerPosition.y);

            // Check if the enemy is exactly 1 tile away in one direction (up, down, left, or right)
            if (deltaX + deltaY == 1) // Either deltaX or deltaY must be 1, but not both
            {
                selectedEnemy = enemy; // Select the enemy
                Debug.Log($"Selected enemy for swing: {selectedEnemy.name}");
            }
            else
            {
                Debug.Log("Enemy is not adjacent to the player (1 tile away in cardinal directions).");
            }
        }
    }

    void TrySwingEnemy()
    {
        if (tilemap == null || selectedEnemy == null) return;

        // Get the player's current position in grid coordinates
        Vector3Int playerPosition = playerMove.CurrentTilePosition;

        // Get the mouse position in world coordinates
        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int clickedTilePosition = tilemap.WorldToCell(mouseWorldPosition);

        // Calculate the difference between the clicked tile and the player's position
        int deltaX = clickedTilePosition.x - playerPosition.x;
        int deltaY = clickedTilePosition.y - playerPosition.y;

        // Ensure the clicked tile is exactly 1 tile away from the player in one direction (up, down, left, or right)
        if (Mathf.Abs(deltaX) + Mathf.Abs(deltaY) == 1)
        {
            Vector3Int targetTilePosition = new Vector3Int(playerPosition.x + deltaX, playerPosition.y + deltaY, playerPosition.z);

            // Ensure the target tile is valid (not occupied and within bounds)
            if (IsTileValid(targetTilePosition))
            {
                Debug.Log($"Swinging enemy {selectedEnemy.name} to {targetTilePosition}");

                // Start the coroutine to move the enemy smoothly
                StartCoroutine(MoveEnemyToTile(selectedEnemy, targetTilePosition));

                selectedEnemy = null; // Reset enemy selection after swing
                isSwinging = false; // Exit swing mode
            }
            else
            {
                Debug.Log("Target tile is not valid for swinging.");
            }
        }
        else
        {
            Debug.Log("You must click one tile adjacent to the player to swing the enemy.");
        }
    }

    private IEnumerator MoveEnemyToTile(Transform enemy, Vector3Int targetTilePosition)
    {
        if (tilemap == null) yield break;

        Vector3 startPosition = enemy.position;
        Vector3 targetPosition = tilemap.GetCellCenterWorld(targetTilePosition);
        float travelTime = 0.5f; // Set the duration of the travel
        float elapsedTime = 0f;

        // Move towards the target position over 'travelTime' seconds
        while (elapsedTime < travelTime)
        {
            enemy.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / travelTime);
            elapsedTime += Time.deltaTime;
            yield return null; // Wait for the next frame
        }

        // Ensure the enemy ends up exactly at the target position
        enemy.position = targetPosition;

        Debug.Log($"{enemy.name} has been swung to {targetTilePosition}");
    }

    bool IsTileValid(Vector3Int tilePosition)
    {
        if (tilemap == null) return false;

        // Check if the tile is valid and not occupied
        return tilemap.HasTile(tilePosition) && !playerMove.IsTileOccupied(tilePosition);
    }
}
