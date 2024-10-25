using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;
using System.Collections.Generic;

public class AIMove : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Reference to the Tilemap used for grid positioning.")]
    public Tilemap tilemap;

    [Tooltip("Reference to the PlayerMove script.")]
    public PlayerMove playerMove;

    [Tooltip("Number of tiles to move per action.")]
    public int moveDistance = 2;

    [Tooltip("Speed at which the AI moves.")]
    public float moveSpeed = 1f;

    [HideInInspector]
    public Vector3Int CurrentTilePosition { get; set; }

    private EnemyHealth enemyHealth;

    [Header("AI Behavior Settings")]
    [Tooltip("If true, the AI will follow the player by default.")]
    public bool followPlayerByDefault = false;

    private int followPlayerTurns = 0; // Number of turns to follow the player after being punched

    void Awake()
    {
        // Get EnemyHealth component
        enemyHealth = GetComponent<EnemyHealth>();

        // Ensure tilemap is assigned
        if (tilemap == null)
        {
            tilemap = FindObjectOfType<Tilemap>();
            if (tilemap == null)
            {
                Debug.LogError($"AIMove: Tilemap not assigned and no Tilemap found in the scene for {gameObject.name}.");
            }
            else
            {
                Debug.Log($"AIMove: Tilemap auto-assigned for {gameObject.name}.");
            }
        }

        // Ensure playerMove is assigned
        if (playerMove == null)
        {
            playerMove = FindObjectOfType<PlayerMove>();
            if (playerMove == null)
            {
                Debug.LogError($"AIMove: PlayerMove not assigned and no PlayerMove found in the scene for {gameObject.name}.");
            }
            else
            {
                Debug.Log($"AIMove: PlayerMove auto-assigned for {gameObject.name}.");
            }
        }
    }

    void Start()
    {
        // Ensure tilemap and playerMove are assigned before using them
        if (tilemap == null || playerMove == null)
        {
            Debug.LogError($"AIMove: Unable to initialize {gameObject.name} due to missing references.");
            return;
        }

        // Set current tile position based on the tilemap
        CurrentTilePosition = tilemap.WorldToCell(transform.position);

        // Register AI with OccupiedTilesManager
        if (OccupiedTilesManager.Instance != null)
        {
            OccupiedTilesManager.Instance.RegisterAI(this);
            Debug.Log($"{gameObject.name} registered with OccupiedTilesManager at position {CurrentTilePosition}");
        }
        else
        {
            Debug.LogError($"AIMove: OccupiedTilesManager instance not found. Ensure it is initialized before AI enemies.");
        }
    }

    /// <summary>
    /// Moves the AI by moveDistance tiles and handles fatigue deduction.
    /// This method is called by AIFatigue.
    /// </summary>
    virtual public IEnumerator MoveAction()
    {
        if (enemyHealth != null && enemyHealth.IsDead)
        {
            yield break;
        }

        List<Vector3Int> path = new List<Vector3Int>();

        bool shouldFollowPlayer = followPlayerByDefault || followPlayerTurns > 0;

        if (shouldFollowPlayer)
        {
            Vector3Int targetTilePosition = playerMove.CurrentTilePosition;

            // Calculate the path towards the player
            List<Vector3Int> calculatedPath = CalculatePath(CurrentTilePosition, targetTilePosition);
            path.AddRange(calculatedPath);
        }
        else
        {
            // Generate a random path
            path = GenerateRandomPath();
        }

        if (path.Count > 0)
        {
            // Move along the path
            yield return StartCoroutine(MoveAlongPath(path));
        }
        else
        {
            // No valid moves, do nothing
            Debug.Log($"{gameObject.name} has no valid moves.");
        }

        // Decrease the followPlayerTurns counter if it's greater than zero
        if (followPlayerTurns > 0)
        {
            followPlayerTurns--;
            // If followPlayerTurns reaches zero, the enemy will revert to their default behavior
        }

        yield return null;
    }

    /// <summary>
    /// Generates a random path based on moveDistance.
    /// </summary>
    /// <returns>List of Vector3Int positions to move to.</returns>
    private List<Vector3Int> GenerateRandomPath()
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

        return path;
    }

    /// <summary>
    /// Calculates a simple path towards the target position.
    /// </summary>
    /// <param name="start">Starting tile position.</param>
    /// <param name="end">Target tile position.</param>
    /// <returns>List of Vector3Int positions to move to.</returns>
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

    /// <summary>
    /// Moves the AI along the specified path.
    /// </summary>
    /// <param name="path">List of tile positions to move through.</param>
    private IEnumerator MoveAlongPath(List<Vector3Int> path)
    {
        foreach (Vector3Int targetPosition in path)
        {
            // Remove current position from occupied positions
            OccupiedTilesManager.Instance.RemoveOccupiedPosition(CurrentTilePosition);

            // Move to the target tile
            yield return StartCoroutine(MoveToTile(targetPosition));

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
    }

    /// <summary>
    /// Moves the AI to a specific tile position over time.
    /// </summary>
    /// <param name="targetTilePosition">Target tile position.</param>
    /// <returns>Coroutine.</returns>
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

    /// <summary>
    /// Checks if moving to the target tile is valid.
    /// </summary>
    /// <param name="targetTilePosition">Target tile position.</param>
    /// <returns>True if move is valid; otherwise, false.</returns>
    private bool IsMoveValid(Vector3Int targetTilePosition)
    {
        // Check if the tile is valid, not occupied by another unit, and within bounds
        if (!tilemap.HasTile(targetTilePosition)
            || OccupiedTilesManager.Instance.IsTileOccupied(targetTilePosition)
            || IsPlayerOrEnemyAtPosition(targetTilePosition))
        {
            return false;
        }
        return true;
    }

    /// <summary>
    /// Checks if the player or any other enemy is at the given tile position.
    /// </summary>
    /// <param name="position">Tile position to check.</param>
    /// <returns>True if player or enemy is present; otherwise, false.</returns>
    private bool IsPlayerOrEnemyAtPosition(Vector3Int position)
    {
        // Check if the player or an enemy is at the given position
        Vector3 worldPosition = tilemap.GetCellCenterWorld(position);
        Collider2D[] colliders = Physics2D.OverlapPointAll(worldPosition);

        foreach (var collider in colliders)
        {
            if (collider.CompareTag("Player") || (collider.CompareTag("Enemy") && collider.gameObject != this.gameObject))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Sets the number of turns the AI will follow the player.
    /// </summary>
    /// <param name="turns">Number of turns to follow the player.</param>
    public void SetFollowPlayerTurns(int turns)
    {
        followPlayerTurns = turns;
    }

    /// <summary>
    /// Resets the AI by stopping all coroutines.
    /// </summary>
    public void ResetAI()
    {
        StopAllCoroutines(); // Stop any active coroutines
        // Reset other state variables if necessary
    }
}
