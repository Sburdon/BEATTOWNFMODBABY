using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using static StateMachine;

public class PlayerMove : MonoBehaviour
{
    public Tilemap tilemap; // Reference to the Tilemap
    public float moveSpeed = 1f; // Speed of movement
    public int maxMoves = 2; // Maximum moves allowed in a turn
    public int remainingMoves; // Count of remaining moves in the current turn
    private bool canMove = false; // Flag to control movement
    private ActionType currentAction; // Current action type for the player
    private Swing swingScript; // Reference to the Swing script
    private StateMachine stateMachine;
    public Vector3Int CurrentTilePosition { get; private set; } // Current tile position in grid coordinates
    private Coroutine currentMoveCoroutine; // Store reference to the current move coroutine
    private PlayerFatigue playerFatigue; // Reference to the PlayerFatigue script
    public int moveFatigueCost = 1; // Fatigue cost for movement

    private bool hasFatigueBeenDeductedForMove = false; // Flag to ensure fatigue is only deducted once per move action

    void Start()
    {
        // Initialize the current tile position based on the player's starting position
        CurrentTilePosition = tilemap.WorldToCell(transform.position);
        UpdatePlayerPosition();
        remainingMoves = maxMoves; // Initialize remaining moves

        swingScript = GetComponent<Swing>(); // Get reference to Swing script
        playerFatigue = GetComponent<PlayerFatigue>(); // Get reference to PlayerFatigue script
        stateMachine = GetComponent<StateMachine>();
        // Register the player with the OccupiedTilesManager
        OccupiedTilesManager.Instance.RegisterPlayer(this);
    }

    void Update()
    {
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

            // Check if the clicked tile is within the allowed move range (up to remaining moves)
            if (deltaX + deltaY <= remainingMoves && (deltaX == 0 || deltaY == 0) && remainingMoves > 0)
            {
                // Check if the target tile is occupied
                if (IsTileOccupied(clickedTilePosition))
                {
                    Debug.Log("Cannot move to tile, it is occupied.");
                    return; // Prevent movement if the tile is occupied
                }

                // Only deduct fatigue once per movement session
                if (!hasFatigueBeenDeductedForMove && playerFatigue.CanPerformAction(moveFatigueCost))
                {
                    playerFatigue.UseFatigue(moveFatigueCost); // Deduct fatigue
                    hasFatigueBeenDeductedForMove = true; // Mark that fatigue has been deducted for this action
                }

                if (hasFatigueBeenDeductedForMove)
                {
                    Debug.Log($"Moving to tile: {clickedTilePosition}");

                    // Start movement coroutine
                    currentMoveCoroutine = StartCoroutine(MoveToTile(clickedTilePosition));

                    // Decrement remainingMoves by the number of tiles moved
                    remainingMoves -= (deltaX + deltaY);
                }
            }
            else
            {
                Debug.Log("Clicked tile is out of range or no moves remaining.");
            }
        }
    }

    public void ResetMoveCount()
    {
        playerFatigue.currentFatigue = playerFatigue.maxFatigue;
        Debug.Log("Player's move count has been reset to maximum.");
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

        // Reset movement state, but allow further movement without fatigue deduction
        canMove = false;
        Debug.Log("Move action canceled, but further movement is allowed if remaining moves exist.");
    }

    private IEnumerator MoveToTile(Vector3Int targetTilePosition)
    {
        // Remove current position from occupied positions
        OccupiedTilesManager.Instance.RemoveOccupiedPosition(CurrentTilePosition);

        // Calculate target position
        Vector3 targetPosition = tilemap.GetCellCenterWorld(targetTilePosition);
        float elapsedTime = 0f;

        // Get current position
        Vector3 startPosition = transform.position;

        stateMachine.ChangeState(WrestlerState.Move);

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

        // Add new position to occupied positions
        OccupiedTilesManager.Instance.AddOccupiedPosition(CurrentTilePosition);

        // Check if no remaining moves are left
        if (remainingMoves <= 0)
        {
            canMove = false; // Disable further movement until reset
            Debug.Log("Movement complete. No moves remaining.");
        }
    }

    // This function can be called from another script to refresh movement count
    public void RefreshSpaceCount()
    {
        remainingMoves = maxMoves; // Reset remaining moves to max
        canMove = true; // Allow movement again
        hasFatigueBeenDeductedForMove = false; // Reset the fatigue deduction flag
        Debug.Log("Movement reset for the next turn.");
    }

    void UpdatePlayerPosition()
    {
        // Center the player on the current tile position
        transform.position = tilemap.GetCellCenterWorld(CurrentTilePosition);

        // Add current position to occupied positions
        OccupiedTilesManager.Instance.AddOccupiedPosition(CurrentTilePosition);
    }

    // Check if a tile is occupied
    public bool IsTileOccupied(Vector3Int position)
    {
        return OccupiedTilesManager.Instance.IsTileOccupied(position);
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
