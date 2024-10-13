using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class AIMove : MonoBehaviour
{
    public PlayerMove playerMove; // Reference to the player for position checks
    public Tilemap tilemap; // Reference to the tilemap for grid-based movement
    public int moveDistance = 2; // AI can move up to this many spaces
    public float moveSpeed = 1f; // Speed of AI movement (adjustable in the Inspector)
    public bool followPlayer = false; // Boolean to follow the player
    private bool hasMoved = false;

    private Vector3Int CurrentTilePosition; // AI's current position
    private Vector3Int playerTilePosition; // Player's current position

    void Start()
    {
        // Initialize the AI's position on the grid
        CurrentTilePosition = tilemap.WorldToCell(transform.position);
    }

    public void TakeTurn()
    {
        // Update positions at the start of each turn
        UpdateAIPosition();
        UpdatePlayerPosition();

        Debug.Log($"{gameObject.name} is starting its turn.");

        if (followPlayer)
        {
            MoveTowardsPlayer();
        }
        else
        {
            MoveLogically();
        }

        hasMoved = true;
    }

    private void UpdateAIPosition()
    {
        // Recalculate AI position on the grid
        CurrentTilePosition = tilemap.WorldToCell(transform.position);
        Debug.Log($"{gameObject.name} position updated to {CurrentTilePosition}.");
    }

    private void UpdatePlayerPosition()
    {
        // Recalculate player's position on the grid
        playerTilePosition = playerMove.CurrentTilePosition;
        Debug.Log($"Player position updated to {playerTilePosition}.");
    }

    private void MoveTowardsPlayer()
    {
        // First, try to move the full moveDistance towards the player
        Vector3Int direction = GetDirectionTowardsPlayer();

        // Try moving two spaces first
        if (!TryMoveInDirection(direction, moveDistance))
        {
            // If moving two spaces isn't possible, move one space instead
            if (!TryMoveInDirection(direction, 1))
            {
                Debug.Log($"{gameObject.name} cannot move closer to the player.");
            }
        }
    }

    private bool TryMoveInDirection(Vector3Int direction, int distance)
    {
        Vector3Int targetPosition = CurrentTilePosition + direction * distance;

        if (IsMoveValid(targetPosition))
        {
            StartCoroutine(MoveToTile(targetPosition));
            return true;
        }
        return false; // Could not move in the specified direction and distance
    }

    private Vector3Int GetDirectionTowardsPlayer()
    {
        int deltaX = playerTilePosition.x - CurrentTilePosition.x;
        int deltaY = playerTilePosition.y - CurrentTilePosition.y;

        Vector3Int direction = Vector3Int.zero;

        // Prioritize the direction with the largest distance to the player
        if (Mathf.Abs(deltaX) > Mathf.Abs(deltaY))
        {
            direction = deltaX > 0 ? new Vector3Int(1, 0, 0) : new Vector3Int(-1, 0, 0); // Move horizontally
        }
        else
        {
            direction = deltaY > 0 ? new Vector3Int(0, 1, 0) : new Vector3Int(0, -1, 0); // Move vertically
        }

        Debug.Log($"{gameObject.name} is moving towards the player in direction {direction}.");
        return direction;
    }

    private void MoveLogically()
    {
        List<Vector3Int> directions = GetValidDirections();

        if (directions.Count == 0)
        {
            Debug.Log($"{gameObject.name} cannot move, all directions are blocked.");
            return; // No valid moves
        }

        // Randomly choose from valid directions
        Vector3Int chosenDirection = directions[Random.Range(0, directions.Count)];
        Vector3Int targetPosition = CurrentTilePosition + chosenDirection * moveDistance;

        Debug.Log($"{gameObject.name} is moving logically to {targetPosition}.");
        StartCoroutine(MoveToTile(targetPosition));
    }

    private List<Vector3Int> GetValidDirections()
    {
        List<Vector3Int> directions = new List<Vector3Int>
        {
            new Vector3Int(1, 0, 0),  // Right
            new Vector3Int(-1, 0, 0), // Left
            new Vector3Int(0, 1, 0),  // Up
            new Vector3Int(0, -1, 0)  // Down
        };

        List<Vector3Int> validDirections = new List<Vector3Int>();

        // Check which directions are valid
        foreach (Vector3Int direction in directions)
        {
            Vector3Int targetPosition = CurrentTilePosition + direction * moveDistance;
            if (IsMoveValid(targetPosition))
            {
                validDirections.Add(direction);
            }
        }

        return validDirections; // Return only valid directions
    }

    private bool IsMoveValid(Vector3Int targetTilePosition)
    {
        // Check if the tile is valid and not occupied by the player
        if (!tilemap.HasTile(targetTilePosition) || playerMove.CurrentTilePosition == targetTilePosition)
        {
            Debug.Log($"{gameObject.name} cannot move to {targetTilePosition}. Tile is invalid or occupied by the player.");
            return false;
        }
        return true;
    }

    private bool IsNextToPlayer()
    {
        int deltaX = Mathf.Abs(CurrentTilePosition.x - playerTilePosition.x);
        int deltaY = Mathf.Abs(CurrentTilePosition.y - playerTilePosition.y);

        return (deltaX + deltaY == 1); // Return true if the AI is 1 tile away
    }

    private IEnumerator MoveToTile(Vector3Int targetTilePosition)
    {
        Vector3 targetPosition = tilemap.GetCellCenterWorld(targetTilePosition);
        float elapsedTime = 0f;

        // Smooth movement over time, based on moveSpeed
        Vector3 startPosition = transform.position;
        float travelTime = 1f / moveSpeed; // Movement duration is inversely proportional to speed

        while (elapsedTime < travelTime)
        {
            transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / travelTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPosition;
        CurrentTilePosition = targetTilePosition;

        Debug.Log($"{gameObject.name} has moved to {targetTilePosition}.");
    }
}
