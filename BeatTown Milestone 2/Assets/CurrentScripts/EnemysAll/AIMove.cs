using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;
using System.Collections.Generic;
using static StateMachine;

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

    public EnemyHealth enemyHealth;
    private StateMachine stateMachine;
    private SpriteRenderer spriteRenderer;

    // Number of turns the AI will follow the player after being punched
    private int followPlayerTurns = 0; 

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        enemyHealth = GetComponent<EnemyHealth>();

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
        stateMachine = GetComponent<StateMachine>();

        if (tilemap == null || playerMove == null)
        {
            Debug.LogError($"AIMove: Unable to initialize {gameObject.name} due to missing references.");
            return;
        }

        CurrentTilePosition = tilemap.WorldToCell(transform.position);

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

        // If followPlayerTurns > 0, follow the player. Otherwise, move randomly.
        bool shouldFollowPlayer = (followPlayerTurns > 0);

        if (shouldFollowPlayer)
        {
            Vector3Int targetTilePosition = playerMove.CurrentTilePosition;
            // Calculate a strictly orthogonal path towards the player
            List<Vector3Int> calculatedPath = CalculatePath(CurrentTilePosition, targetTilePosition);
            path.AddRange(calculatedPath);
        }
        else
        {
            // Generate a random orthogonal path
            path = GenerateRandomPath();
        }

        if (path.Count > 0)
        {
            yield return StartCoroutine(MoveAlongPath(path));
        }
        else
        {
            Debug.Log($"{gameObject.name} has no valid moves.");
        }

        yield return null;
    }

    /// <summary>
    /// Sets the number of turns the AI will follow the player.
    /// </summary>
    /// <param name="turns">Number of turns to follow the player.</param>
    public void SetFollowPlayerForTurns(int turns)
    {
        followPlayerTurns = turns;
    }

    /// <summary>
    /// Decrements the follow turns by 1. If it hits zero, AI stops following the player.
    /// </summary>
    public void DecrementFollowTurns()
    {
        if (followPlayerTurns > 0)
        {
            followPlayerTurns--;
        }
    }

    public bool IsFollowingPlayer()
    {
        return followPlayerTurns > 0;
    }

    /// <summary>
    /// Generates a random orthogonal path (no diagonals).
    /// </summary>
    private List<Vector3Int> GenerateRandomPath()
    {
        List<Vector3Int> path = new List<Vector3Int>();
        Vector3Int currentPosition = CurrentTilePosition;

        // Only orthogonal directions
        Vector3Int[] directions = new Vector3Int[]
        {
            Vector3Int.up,
            Vector3Int.down,
            Vector3Int.left,
            Vector3Int.right
        };

        for (int step = 0; step < moveDistance; step++)
        {
            List<Vector3Int> possibleMoves = new List<Vector3Int>();

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
                int randomIndex = Random.Range(0, possibleMoves.Count);
                Vector3Int targetPosition = possibleMoves[randomIndex];
                path.Add(targetPosition);
                currentPosition = targetPosition;
            }
            else
            {
                break;
            }
        }

        return path;
    }

    /// <summary>
    /// Calculates a strictly orthogonal path towards the target position by first moving along x, then along y.
    /// </summary>
    protected virtual List<Vector3Int> CalculatePath(Vector3Int start, Vector3Int end)
    {
        List<Vector3Int> path = new List<Vector3Int>();

        int dx = end.x - start.x;
        int dy = end.y - start.y;

        int stepX = dx > 0 ? 1 : -1;
        int stepY = dy > 0 ? 1 : -1;

        int x = start.x;
        int y = start.y;

        // Move horizontally first
        for (int i = 0; i < Mathf.Abs(dx); i++)
        {
            x += stepX;
            Vector3Int nextPosition = new Vector3Int(x, y, start.z);
            // Each move changes either x or y by 1, never both - no diagonals
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

        // Then move vertically
        for (int i = 0; i < Mathf.Abs(dy); i++)
        {
            y += stepY;
            Vector3Int nextPosition = new Vector3Int(x, y, start.z);
            // Still only changing one coordinate at a time
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

    public IEnumerator MoveAlongPath(List<Vector3Int> path)
    {
        foreach (Vector3Int targetPosition in path)
        {
            OccupiedTilesManager.Instance.RemoveOccupiedPosition(CurrentTilePosition);
            yield return StartCoroutine(MoveToTile(targetPosition));
            CurrentTilePosition = targetPosition;
            OccupiedTilesManager.Instance.AddOccupiedPosition(CurrentTilePosition);

            if (Hook.Instance != null && Hook.Instance.GetHookPosition() == CurrentTilePosition)
            {
                Hook.Instance.HandleEnemyHit(gameObject);
                yield break;
            }
        }
    }

    private IEnumerator MoveToTile(Vector3Int targetTilePosition)
    {
        Vector3 targetWorldPosition = tilemap.GetCellCenterWorld(targetTilePosition);
        float elapsedTime = 0f;
        float travelTime = 1f / moveSpeed;

        Vector3 startPosition = transform.position;

        // Flip sprite if moving horizontally
        if (targetTilePosition.x < CurrentTilePosition.x)
        {
            spriteRenderer.flipX = true;
        }
        else if (targetTilePosition.x > CurrentTilePosition.x)
        {
            spriteRenderer.flipX = false;
        }

        stateMachine.ChangeState(WrestlerState.Move);

        // The movement is always from one tile to an orthogonally adjacent tile,
        // so no diagonal lines will be drawn by Lerp.
        while (elapsedTime < travelTime)
        {
            transform.position = Vector3.Lerp(startPosition, targetWorldPosition, elapsedTime / travelTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        transform.position = targetWorldPosition;
    }

    public bool IsMoveValid(Vector3Int targetTilePosition)
    {
        if (!tilemap.HasTile(targetTilePosition)
            || OccupiedTilesManager.Instance.IsTileOccupied(targetTilePosition)
            || IsPlayerOrEnemyAtPosition(targetTilePosition))
        {
            return false;
        }
        return true;
    }

    private bool IsPlayerOrEnemyAtPosition(Vector3Int position)
    {
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

    public void ResetAI()
    {
        StopAllCoroutines();
    }
}
