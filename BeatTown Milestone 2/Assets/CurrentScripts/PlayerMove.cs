using System.Collections;
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
        tilemap = tilemap ?? FindObjectOfType<Tilemap>(); // Finds the Tilemap in the scene if not set in inspector
        swingScript = swingScript ?? GetComponent<Swing>(); // Ensure Swing script is assigned
        if (tilemap != null)
        {
            CurrentTilePosition = tilemap.WorldToCell(transform.position);
            UpdatePlayerPosition();
        }
        remainingMoves = maxMoves; // Initialize remaining moves
    }

    void Update()
    {
        if (tilemap == null) return;
        CurrentTilePosition = tilemap.WorldToCell(transform.position);

        // Handle movement logic when canMove is true
        if (canMove && Input.GetMouseButtonDown(0)) // Left mouse button
        {
            Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int clickedTilePosition = tilemap.WorldToCell(mouseWorldPosition);

            int deltaX = Mathf.Abs(clickedTilePosition.x - CurrentTilePosition.x);
            int deltaY = Mathf.Abs(clickedTilePosition.y - CurrentTilePosition.y);

            if (deltaX + deltaY <= remainingMoves && (deltaX == 0 || deltaY == 0) && remainingMoves > 0)
            {
                StartCoroutine(MoveToTile(clickedTilePosition));
                remainingMoves -= (deltaX + deltaY); // Decrement remaining moves
            }
            else
            {
                Debug.Log("Tile out of range or no moves remaining.");
            }
        }
    }

    public void OnMoveButtonPressed()
    {
        swingScript?.CancelSwing(); // Cancel swing if it's active
        canMove = true;
        Debug.Log("Move button pressed. Remaining moves: " + remainingMoves);
        CurrentAction = ActionType.Move; // Set current action to Move
    }

    public void CancelMove()
    {
        canMove = false;
        Debug.Log("Move action canceled.");
    }

    private IEnumerator MoveToTile(Vector3Int targetTilePosition)
    {
        Vector3 targetPosition = tilemap.GetCellCenterWorld(targetTilePosition);
        float elapsedTime = 0f;
        Vector3 startPosition = transform.position;

        while (elapsedTime < 1f)
        {
            transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / 1f);
            elapsedTime += Time.deltaTime * moveSpeed;
            yield return null;
        }

        transform.position = targetPosition; // Ensure exact position
        CurrentTilePosition = targetTilePosition;

        if (remainingMoves <= 0)
        {
            canMove = false;
            Debug.Log("Movement complete. No moves remaining.");
        }
    }

    public void RefreshSpaceCount()
    {
        remainingMoves = maxMoves;
        canMove = true;
        Debug.Log("Movement reset for next turn.");
    }

    // Method to update the player's position based on the current tile
    public void UpdatePlayerPosition()
    {
        transform.position = tilemap.GetCellCenterWorld(CurrentTilePosition);
    }

    // Method to check if a tile is occupied by an enemy
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
