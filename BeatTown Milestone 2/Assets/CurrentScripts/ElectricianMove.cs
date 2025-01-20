using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;
using System.Collections.Generic;

public class ElectricianMove : MonoBehaviour
{
    [Header("Tile and Movement Settings")]
    public Tilemap tilemap;
    public PlayerMove playerMove;  
    public int moveDistance = 2;    // tiles per turn
    public float moveSpeed = 1f;    // speed of movement per tile

    [HideInInspector]
    public Vector3Int CurrentTilePosition { get; set; }

    [Header("Route Settings")]
    [Tooltip("List of tile coordinates that the Electrician will visit in sequence.")]
    public List<Vector3Int> routePositions = new List<Vector3Int>();
    private int currentRouteIndex = 0;

    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (tilemap == null)
        {
            tilemap = FindObjectOfType<Tilemap>();
            if (tilemap == null)
                Debug.LogError($"ElectricianMove: No Tilemap assigned and none found in scene.");
        }

        if (playerMove == null)
        {
            playerMove = FindObjectOfType<PlayerMove>();
            // Not strictly required unless you need player reference
        }
    }

    void Start()
    {
        // Initialize the CurrentTilePosition based on spawn position
        if (tilemap != null)
            CurrentTilePosition = tilemap.WorldToCell(transform.position);

        // Instead of RegisterAI(this), use a new method that doesn’t require AIMove
        if (OccupiedTilesManager.Instance != null)
        {
            // Mark this tile as occupied so no one else can spawn here
            OccupiedTilesManager.Instance.AddOccupiedPosition(CurrentTilePosition);
        }
    }

    /// <summary>
    /// Called when it’s this Electrician’s turn. Moves it toward the next route position.
    /// </summary>
    public IEnumerator MoveAction()
    {
        // If we've visited all route positions, do nothing.
        if (currentRouteIndex >= routePositions.Count)
            yield break;

        Vector3Int destinationTile = routePositions[currentRouteIndex];
        List<Vector3Int> path = CalculatePath(CurrentTilePosition, destinationTile);

        // Only move up to 'moveDistance' steps
        if (path.Count > moveDistance)
            path = path.GetRange(0, moveDistance);

        // Move along the path
        if (path.Count > 0)
            yield return StartCoroutine(MoveAlongPath(path));

        // If we fully arrived at the route position, move to next
        if (CurrentTilePosition == destinationTile)
            currentRouteIndex++;

        yield return null;
    }

    private List<Vector3Int> CalculatePath(Vector3Int start, Vector3Int end)
    {
        List<Vector3Int> path = new List<Vector3Int>();
        if (tilemap == null) return path;

        int dx = end.x - start.x;
        int dy = end.y - start.y;
        int stepX = (dx > 0) ? 1 : -1;
        int stepY = (dy > 0) ? 1 : -1;

        int x = start.x;
        int y = start.y;

        // Move horizontally first
        for (int i = 0; i < Mathf.Abs(dx); i++)
        {
            x += stepX;
            Vector3Int nextPos = new Vector3Int(x, y, start.z);
            if (IsMoveValid(nextPos))
            {
                path.Add(nextPos);
                if (path.Count >= moveDistance)
                    return path;
            }
            else break;
        }

        // Then move vertically
        for (int i = 0; i < Mathf.Abs(dy); i++)
        {
            y += stepY;
            Vector3Int nextPos = new Vector3Int(x, y, start.z);
            if (IsMoveValid(nextPos))
            {
                path.Add(nextPos);
                if (path.Count >= moveDistance)
                    return path;
            }
            else break;
        }

        return path;
    }

    private bool IsMoveValid(Vector3Int tilePos)
    {
        if (tilemap == null) return false;
        // 1) The tile must exist in the tilemap
        if (!tilemap.HasTile(tilePos))
            return false;
        // 2) The tile must not be already occupied
        if (OccupiedTilesManager.Instance.IsTileOccupied(tilePos))
            return false;
        // 3) Can't overlap player or another enemy
        return !IsPlayerOrEnemyAtPosition(tilePos);
    }

    private bool IsPlayerOrEnemyAtPosition(Vector3Int position)
    {
        Vector3 worldPos = tilemap.GetCellCenterWorld(position);
        Collider2D[] colliders = Physics2D.OverlapPointAll(worldPos);

        foreach (var col in colliders)
        {
            // You can exclude this Electrician itself if it’s tagged "Enemy"
            if (col.CompareTag("Player") || 
               (col.CompareTag("Enemy") && col.gameObject != this.gameObject))
            {
                return true;
            }
        }
        return false;
    }

    private IEnumerator MoveAlongPath(List<Vector3Int> path)
    {
        foreach (Vector3Int targetPos in path)
        {
            // Un-occupy the old tile
            OccupiedTilesManager.Instance.RemoveOccupiedPosition(CurrentTilePosition);

            // Move smoothly to the next tile
            yield return StartCoroutine(MoveToTile(targetPos));

            // Update new position
            CurrentTilePosition = targetPos;

            // Re-occupy the tile we just arrived at
            OccupiedTilesManager.Instance.AddOccupiedPosition(CurrentTilePosition);

            // Check if we landed on the Hook
            if (Hook.Instance != null && Hook.Instance.GetHookPosition() == CurrentTilePosition)
            {
                Hook.Instance.HandleEnemyHit(gameObject);
                yield break;
            }
        }
    }

    private IEnumerator MoveToTile(Vector3Int tile)
    {
        Vector3 startPos = transform.position;
        Vector3 endPos = tilemap.GetCellCenterWorld(tile);
        float elapsed = 0f;
        float travelTime = 1f / moveSpeed; // how many seconds per 1 tile move

        // Flip sprite if moving horizontally
        if (tile.x < CurrentTilePosition.x) 
            spriteRenderer.flipX = true;
        else if (tile.x > CurrentTilePosition.x) 
            spriteRenderer.flipX = false;

        while (elapsed < travelTime)
        {
            transform.position = Vector3.Lerp(startPos, endPos, elapsed / travelTime);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = endPos;
    }
}
