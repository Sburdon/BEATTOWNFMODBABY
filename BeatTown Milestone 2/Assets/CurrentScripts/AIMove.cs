using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;
using System.Collections.Generic;

public class AIMove : MonoBehaviour
{
    public Tilemap tilemap;
    public PlayerMove playerMove;
    public int moveDistance = 2; // Number of tiles to move per turn
    public float moveSpeed = 1f;

    public Vector3Int CurrentTilePosition;

    private EnemyHealth enemyHealth;

    [Header("AI Behavior Settings")]
    public bool followPlayerByDefault = false; // Toggle to follow the player or move randomly

    private int followPlayerTurns = 0; // Number of turns to follow the player after being punched

    void Start()
    {
        enemyHealth = GetComponent<EnemyHealth>();
        CurrentTilePosition = tilemap.WorldToCell(transform.position);
        OccupiedTilesManager.Instance.RegisterAI(this);
    }

    public void TakeTurn()
    {
        if (enemyHealth != null && enemyHealth.IsDead)
        {
            return;
        }

        bool shouldFollowPlayer = followPlayerByDefault || followPlayerTurns > 0;

        if (shouldFollowPlayer)
        {
            MoveTowardsPlayer();
        }
        else
        {
            MoveRandomly();
        }

        // Decrease the followPlayerTurns counter if it's greater than zero
        if (followPlayerTurns > 0)
        {
            followPlayerTurns--;
            // If followPlayerTurns reaches zero, the enemy will revert to their default behavior
        }
    }

    public void SetFollowPlayerTurns(int turns)
    {
        followPlayerTurns = turns;
    }

    private void MoveTowardsPlayer()
    {
        // Implement movement towards the player
        Vector3Int targetTilePosition = playerMove.CurrentTilePosition;

        // Calculate the path towards the player
        List<Vector3Int> path = CalculatePath(CurrentTilePosition, targetTilePosition);

        // Move along the path up to moveDistance tiles
        StartCoroutine(MoveAlongPath(path));
    }

    private void MoveRandomly()
    {
        List<Vector3Int> path = new List<Vector3Int>();

        Vector3Int currentPosition = CurrentTilePosition;

        for (int step = 0; step < moveDistance; step++)
        {
            // Generate possible directions
            List<Vector3Int> possibleMoves = new List<Vector3Int>();

            Vector3Int[] directions = new Vector3Int[]
            {
                Vector3Int.up,
                Vector3Int.down,
                Vector3Int.left,
                Vector3Int.right
            };

            foreach (Vector3Int dir in directions)
            {
                Vector3Int nextPosition = currentPosition + dir;
                if (IsMoveValid(nextPosition) && (path.Count == 0 || nextPosition != path[path.Count - 1]))
                {
                    possibleMoves.Add(nextPosition);
                }
            }

            if (possibleMoves.Count > 0)
            {
                // Randomly select one of the possible moves
                int randomIndex = Random.Range(0, possibleMoves.Count);
                Vector3Int targetPosition = possibleMoves[randomIndex];

                path.Add(targetPosition);

                // Update currentPosition for next step
                currentPosition = targetPosition;
            }
            else
            {
                // No valid moves from current position
                break;
            }
        }

        if (path.Count > 0)
        {
            // Move along the path
            StartCoroutine(MoveAlongPath(path));
        }
        else
        {
            // No valid moves, do nothing
            Debug.Log($"{gameObject.name} has no valid moves.");
        }
    }

    private List<Vector3Int> CalculatePath(Vector3Int start, Vector3Int end)
    {
        // Simple pathfinding: move in x direction, then y direction
        List<Vector3Int> path = new List<Vector3Int>();

        int dx = end.x - start.x;
        int dy = end.y - start.y;

        int stepX = dx > 0 ? 1 : -1;
        int stepY = dy > 0 ? 1 : -1;

        int x = start.x;
        int y = start.y;

        // Move along x-axis
        for (int i = 0; i < Mathf.Abs(dx); i++)
        {
            x += stepX;
            Vector3Int nextPosition = new Vector3Int(x, y, start.z);
            if (IsMoveValid(nextPosition))
            {
                path.Add(nextPosition);
                if (path.Count >= moveDistance)
                {
                    return path;
                }
            }
            else
            {
                break; // Stop if movement is blocked
            }
        }

        // Move along y-axis
        for (int i = 0; i < Mathf.Abs(dy); i++)
        {
            y += stepY;
            Vector3Int nextPosition = new Vector3Int(x, y, start.z);
            if (IsMoveValid(nextPosition))
            {
                path.Add(nextPosition);
                if (path.Count >= moveDistance)
                {
                    return path;
                }
            }
            else
            {
                break;
            }
        }

        return path;
    }

    private IEnumerator MoveAlongPath(List<Vector3Int> path)
    {
        foreach (Vector3Int targetPosition in path)
        {
            // Remove current position from occupied positions
            OccupiedTilesManager.Instance.RemoveOccupiedPosition(CurrentTilePosition);

            // Move to the target tile
            yield return MoveToTile(targetPosition);

            // Update current tile position
            CurrentTilePosition = targetPosition;
            OccupiedTilesManager.Instance.AddOccupiedPosition(CurrentTilePosition);

            // Check for collision with hook
            if (Hook.Instance != null && Hook.Instance.GetHookPosition() == CurrentTilePosition)
            {
                // Handle collision with hook
                Hook.Instance.HandleEnemyHit(gameObject);
                yield break; // Stop further movement
            }
        }

        // After moving, attempt to attack if in range
        // AttackIfInRange(); // Uncomment if you have an attack method
    }

    private IEnumerator MoveToTile(Vector3Int targetTilePosition)
    {
        Vector3 targetWorldPosition = tilemap.GetCellCenterWorld(targetTilePosition);
        float elapsedTime = 0f;
        float travelTime = 1f / moveSpeed;

        Vector3 startPosition = transform.position;

        while (elapsedTime < travelTime)
        {
            transform.position = Vector3.Lerp(startPosition, targetWorldPosition, elapsedTime / travelTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = targetWorldPosition;
    }

    private bool IsMoveValid(Vector3Int targetTilePosition)
    {
        // Check if the tile is valid and within bounds
        if (!tilemap.HasTile(targetTilePosition))
        {
            return false;
        }

        // Allow moving into the hook's tile
        if (OccupiedTilesManager.Instance.IsTileOccupied(targetTilePosition))
        {
            // Check if the occupied tile is the hook's tile
            if (Hook.Instance != null && Hook.Instance.GetHookPosition() == targetTilePosition)
            {
                // Allow moving into the hook's tile
                return true;
            }
            else
            {
                // Tile is occupied by another unit
                return false;
            }
        }

        return true;
    }

    public void ResetAI()
    {
        StopAllCoroutines(); // Stop any active coroutines
        // Reset other state variables if necessary
    }
}
