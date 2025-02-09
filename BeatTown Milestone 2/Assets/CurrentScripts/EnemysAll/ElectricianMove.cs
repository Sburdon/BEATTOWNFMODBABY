using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// An Electrician script that moves along a route (list of tile coords) orthogonally,
/// without referencing AIMove. 
/// </summary>
public class ElectricianMove : MonoBehaviour
{
    [Header("Tile & Movement")]
    public Tilemap tilemap;
    public int moveDistance = 2;
    public float moveSpeed = 1f; // tiles per second

    [Header("Route")]
    [Tooltip("The list of grid positions (x,y,z=0) the Electrician will visit in order.")]
    public List<Vector3Int> routePositions = new List<Vector3Int>();
    public int currentRouteIndex = 0;

    [HideInInspector]
    public Vector3Int CurrentTilePosition;

    private SpriteRenderer spriteRenderer;
    private bool canMove; // ask Spencer where to add bool check 
    public bool InPuddle;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (tilemap == null)
            tilemap = FindObjectOfType<Tilemap>();
    }

    private void Start()
    {
        if (tilemap != null)
            CurrentTilePosition = tilemap.WorldToCell(transform.position);

        // Optionally register Electrician in OccupiedTilesManager
        if (OccupiedTilesManager.Instance != null)
        {
            OccupiedTilesManager.Instance.AddOccupiedPosition(CurrentTilePosition);
        }
    }

    /// <summary>
    /// Called each time the Electrician takes a turn (e.g., from TempTurnBase).
    /// Moves toward the next route position up to moveDistance steps orthogonally.
    /// </summary>
    public IEnumerator MoveAction()
    {
        if (currentRouteIndex >= routePositions.Count)
        {
            yield break; // done route
        }

        Vector3Int destinationTile = routePositions[currentRouteIndex];
        // Build orth path from current tile to destination
        List<Vector3Int> path = CalculateOrthPath(CurrentTilePosition, destinationTile);

        // Limit to moveDistance
        if (path.Count > moveDistance)
            path = path.GetRange(0, moveDistance);

        if (path.Count > 0)
            yield return StartCoroutine(MoveAlongPath(path));

        // If we fully arrived at the route tile
        if (CurrentTilePosition == destinationTile)
            currentRouteIndex++;

        yield return null;
    }

    /// <summary>
    /// Purely orth moves from start to end (x first, then y),
    /// verifying each tile with IsMoveValid().
    /// </summary>
    private List<Vector3Int> CalculateOrthPath(Vector3Int start, Vector3Int end)
    {
        List<Vector3Int> path = new List<Vector3Int>();

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
            if (!IsMoveValid(nextPos)) break;
            path.Add(nextPos);
        }

        // Then move vertically
        for (int i = 0; i < Mathf.Abs(dy); i++)
        {
            y += stepY;
            Vector3Int nextPos = new Vector3Int(x, y, start.z);
            if (!IsMoveValid(nextPos)) break;
            path.Add(nextPos);
        }

        return path;
    }

    private IEnumerator MoveAlongPath(List<Vector3Int> path)
    {
        foreach (Vector3Int tile in path)
        {
            // Unoccupy old tile
            if (OccupiedTilesManager.Instance != null)
                OccupiedTilesManager.Instance.RemoveOccupiedPosition(CurrentTilePosition);

            yield return StartCoroutine(MoveToTile(tile));
            CurrentTilePosition = tile;

            // Re-occupy new tile
            if (OccupiedTilesManager.Instance != null)
                OccupiedTilesManager.Instance.AddOccupiedPosition(CurrentTilePosition);
        }
    }

    private IEnumerator MoveToTile(Vector3Int tile)
    {
        Vector3 startPos = transform.position;
        Vector3 endPos = tilemap.GetCellCenterWorld(tile);
        float elapsed = 0f;
        float travelTime = 1f / moveSpeed;

        // Sprite flip
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

    private bool IsMoveValid(Vector3Int tilePos)
    {
        if (tilemap == null) return false;
        // Must have a tile on tilemap
        if (!tilemap.HasTile(tilePos)) return false;
        // Must not be in occupied tiles
        if (OccupiedTilesManager.Instance != null && OccupiedTilesManager.Instance.IsTileOccupied(tilePos))
            return false;
        // Must not be blocked by other objects
        return !IsBlockedByObject(tilePos);
    }

    private bool IsBlockedByObject(Vector3Int tilePos)
    {
        Vector3 checkPos = tilemap.GetCellCenterWorld(tilePos);
        Collider2D[] colliders = Physics2D.OverlapPointAll(checkPos);
        foreach (var col in colliders)
        {
            // If it's a Player or Enemy (not itself), treat as blocked
            if (col.CompareTag("Player") || (col.CompareTag("Enemy") && col.gameObject != this.gameObject))
                return true;
        }
        return false;
    }
}
