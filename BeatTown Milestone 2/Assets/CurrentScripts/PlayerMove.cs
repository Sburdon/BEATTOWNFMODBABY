using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerMove : MonoBehaviour
{
    public Tilemap tilemap; // Reference to the Tilemap
    public float moveSpeed = 5f; // Speed of movement
    public int maxMoves = 2; // Maximum moves allowed in a turn
    public int remainingMoves; // Count of remaining moves in the current turn
    private bool canMove = false; // Flag to control movement
    private ActionType currentAction; // Current action type for the player
    private Swing swingScript; // Reference to the Swing script
    public Vector3Int CurrentTilePosition { get; private set; } // Current tile position in grid coordinates
    private Coroutine currentMoveCoroutine; // Store reference to the current move coroutine

    void Start()
    {
        // Initialize the current tile position based on the player's starting position
        CurrentTilePosition = tilemap.WorldToCell(transform.position);
        UpdatePlayerPosition();
        remainingMoves = maxMoves; // Initialize remaining moves

        swingScript = GetComponent<Swing>(); // Get reference to Swing script
    }

    void Update()
    {
        // Prevent movement if swing action is active
        if (swingScript.IsSwinging()) return;

        // Update CurrentTilePosition based on the player's position
        CurrentTilePosition = tilemap.WorldToCell(transform.position);

        // Check for mouse input only if canMove is true
        if (canMove && Input.GetMouseButtonDown(0)) // Left mouse button
        {
            // Get the mouse position and convert it to a world position
            Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int clickedTilePosition = tilemap.WorldToCell(mouseWorldPosition);

            // Calculate the distance in terms of grid coordinates
            int deltaX = Mathf.Abs(clickedTilePosition.x - CurrentTilePosition.x);
            int deltaY = Mathf.Abs(clickedTilePosition.y - CurrentTilePosition.y);

            // Check if the clicked tile is within the allowed move range (2 tiles in one direction)
            if ((deltaX + deltaY <= 2) && (deltaX == 0 || deltaY == 0) && remainingMoves > 0)
            {
                if (!IsTileOccupied(clickedTilePosition))
                {
                    Debug.Log($"Moving to tile: {clickedTilePosition}");
                    currentMoveCoroutine = StartCoroutine(MoveToTile(clickedTilePosition));
                    remainingMoves--; // Decrease remaining moves
                }
                else
                {
                    Debug.Log("Cannot move to this tile. It is occupied by an enemy.");
                }
            }
            else
            {
                Debug.Log("Clicked tile is out of range or no moves remaining.");
            }
        }
    }

    public void OnMoveButtonPressed()
    {
        // Cancel swing mode if active
        if (swingScript != null && swingScript.IsSwinging())
        {
            swingScript.CancelSwing();
        }

        // Allow movement when the Move button is pressed
        canMove = true;
        Debug.Log("Move button pressed. You have " + remainingMoves + " moves available.");
        CurrentAction = ActionType.Move; // Set current action to Move
    }

    public void CancelMove()
    {
        // Stop current move coroutine if active
        if (currentMoveCoroutine != null)
        {
            StopCoroutine(currentMoveCoroutine);
            currentMoveCoroutine = null;
        }

        // Reset movement state
        canMove = false;
        Debug.Log("Move action canceled.");
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
        CurrentTilePosition = targetTilePosition;

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
        transform.position = tilemap.GetCellCenterWorld(CurrentTilePosition);
    }

    // Check if a tile is occupied by an enemy
    public bool IsTileOccupied(Vector3Int position)
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(tilemap.GetCellCenterWorld(position), 0.1f);
        foreach (Collider2D collider in colliders)
        {
            if (collider.CompareTag("Enemy"))
            {
                return true; // Tile is occupied by an enemy
            }
        }
        return false; // Tile is not occupied
    }

    public ActionType CurrentAction
    {
        get { return currentAction; }
        set
        {
            currentAction = value;
            Debug.Log("Current Action set to: " + currentAction);
        }
    }
}
