using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class PlayerMove : MonoBehaviour
{
    public Tilemap tilemap; // Reference to the Tilemap
    private Vector3Int currentTilePosition; // Current tile position in grid coordinates
    public float moveSpeed = 5f; // Speed of movement
    public int maxMoves = 2; // Maximum moves allowed in a turn
    public int remainingMoves; // Count of remaining moves in the current turn
    private bool canMove = false; // Flag to control movement

    void Start()
    {
        // Initialize the current tile position based on the player's starting position
        currentTilePosition = tilemap.WorldToCell(transform.position);
        UpdatePlayerPosition();
        remainingMoves = maxMoves; // Initialize remaining moves
    }

    void Update()
    {
        // Check for mouse input only if canMove is true
        if (canMove && Input.GetMouseButtonDown(0)) // Left mouse button
        {
            // Get the mouse position and convert it to a world position
            Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int clickedTilePosition = tilemap.WorldToCell(mouseWorldPosition);

            // Calculate the distance in terms of grid coordinates
            int deltaX = Mathf.Abs(clickedTilePosition.x - currentTilePosition.x);
            int deltaY = Mathf.Abs(clickedTilePosition.y - currentTilePosition.y);

            // Check if the clicked tile is within the allowed move range (2 tiles in one direction)
            if ((deltaX + deltaY <= 2) && (deltaX == 0 || deltaY == 0) && remainingMoves > 0)
            {
                Debug.Log($"Moving to tile: {clickedTilePosition}");
                StartCoroutine(MoveToTile(clickedTilePosition));
                remainingMoves--; // Decrease remaining moves
            }
            else
            {
                Debug.Log("Clicked tile is out of range or no moves remaining.");
            }
        }
    }

    public void OnMoveButtonPressed()
    {
        // Allow movement when the Move button is pressed
        canMove = true;
        remainingMoves = maxMoves; // Reset remaining moves
        Debug.Log("Move button pressed. You have " + remainingMoves + " moves available.");
    }

    private IEnumerator MoveToTile(Vector3Int targetTilePosition)
    {
        // Calculate target position
        Vector3 targetPosition = tilemap.GetCellCenterWorld(targetTilePosition);
        float elapsedTime = 0f;

        // Get current position
        Vector3 startPosition = transform.position;

        // Move towards the target position
        while (elapsedTime < 1f) // Move for 1 second
        {
            transform.position = Vector3.Lerp(startPosition, targetPosition, (elapsedTime / 1f)); // Lerp for smooth movement
            elapsedTime += Time.deltaTime * moveSpeed; // Increment elapsed time
            yield return null; // Wait for the next frame
        }

        // Ensure the player ends up exactly at the target position
        transform.position = targetPosition;

        // Update current tile position after the move
        currentTilePosition = targetTilePosition;

        // Check if no remaining moves are left
        if (remainingMoves <= 0)
        {
            canMove = false; // Disable further movement until reset
            Debug.Log("Movement complete. Waiting for refresh.");
        }
    }

    // This function can be called from another script to refresh movement count
    public void RefreshSpaceCount()
    {
        remainingMoves = maxMoves; // Reset remaining moves to max
        canMove = true; // Allow movement again
        Debug.Log("Movement reset for the next turn.");
    }

    void UpdatePlayerPosition()
    {
        // Center the player on the current tile position
        transform.position = tilemap.GetCellCenterWorld(currentTilePosition);
    }
}