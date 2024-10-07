using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;

public class Swing : MonoBehaviour
{
    public Tilemap tilemap; // Reference to the Tilemap
    private Transform selectedEnemy; // Currently selected enemy
    private bool isSwinging; // State to track if we are in swing mode
    private PlayerMove playerMove; // Reference to PlayerMove instance

    private void Awake()
    {
        playerMove = GetComponent<PlayerMove>();
    }

    void Update()
    {
        // Check for mouse input to select a tile if swinging
        if (Input.GetMouseButtonDown(0)) // Left mouse button
        {
            if (isSwinging)
            {
                if (selectedEnemy != null)
                {
                    // Try to swing the enemy to the clicked tile
                    TrySwingEnemy();
                }
                else
                {
                    // Select an enemy if none is currently selected
                    SelectEnemy();
                }
            }
        }
    }

    public void OnSwingButtonPressed()
    {
        // Cancel any movement when the swing button is pressed
        playerMove.CancelMove();

        isSwinging = true; // Activate swinging mode
        selectedEnemy = null; // Reset selected enemy
        Debug.Log("Swing button pressed, current action: " + playerMove.CurrentAction);

        // Check if any enemies are in range to swing immediately
        CheckEnemiesInRange();
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
        // Raycast to check if an enemy is clicked
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

        if (hit.collider != null)
        {
            // Check if the clicked object is tagged as "Enemy"
            if (hit.collider.CompareTag("Enemy"))
            {
                selectedEnemy = hit.collider.transform; // Select the enemy
                Debug.Log($"Selected enemy: {selectedEnemy.name}");
            }
        }
    }

    void TrySwingEnemy()
    {
        if (selectedEnemy != null)
        {
            // Get the player's current position in grid coordinates
            Vector3Int playerPosition = playerMove.CurrentTilePosition;

            // Get the mouse position in world coordinates
            Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int clickedTilePosition = tilemap.WorldToCell(mouseWorldPosition);

            // Calculate the difference between the clicked tile and the player's position
            int deltaX = clickedTilePosition.x - playerPosition.x;
            int deltaY = clickedTilePosition.y - playerPosition.y;

            // Ensure the clicked tile is exactly 1 tile away from the player in one direction (up, down, left, right)
            if (Mathf.Abs(deltaX) + Mathf.Abs(deltaY) == 1)
            {
                // Target tile is one step away from the player
                Vector3Int targetTilePosition = new Vector3Int(playerPosition.x + deltaX, playerPosition.y + deltaY, playerPosition.z);

                // Ensure the target tile is valid (not occupied and within bounds)
                if (IsTileValid(targetTilePosition))
                {
                    Debug.Log($"Swinging enemy {selectedEnemy.name} to {targetTilePosition}");
                    selectedEnemy.position = tilemap.GetCellCenterWorld(targetTilePosition); // Move enemy to new position
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
    }

    void CheckEnemiesInRange()
    {
        // Logic to check for enemies within range and display UI or similar
    }

    bool IsTileValid(Vector3Int tilePosition)
    {
        // Check if the tile is a valid, empty tile (e.g., not occupied by another enemy or blocked)
        return tilemap.HasTile(tilePosition) && !playerMove.IsTileOccupied(tilePosition);
    }
}
