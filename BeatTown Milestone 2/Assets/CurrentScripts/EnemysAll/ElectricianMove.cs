using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

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

    [Header("Panel System")]
    public GameObject brokenPanelPrefab; // Assign in Inspector
    public GameObject fixedPanelPrefab;  // Assign in Inspector
    private Dictionary<Vector3Int, GameObject> panelObjects = new Dictionary<Vector3Int, GameObject>();

    [HideInInspector]
    public Vector3Int CurrentTilePosition;

    private SpriteRenderer spriteRenderer;
    public bool InPuddle;

    private int fatigue = 2; // Fatigue resets to 2 at the start of each turn

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

    if (OccupiedTilesManager.Instance != null)
    {
        OccupiedTilesManager.Instance.AddOccupiedPosition(CurrentTilePosition);
    }

    // Remove the initial spawn position after the first move
    StartCoroutine(RemoveSpawnOccupiedTile());

    // Spawn broken panels at designated positions
    SpawnBrokenPanels();
}

private IEnumerator RemoveSpawnOccupiedTile()
{
    yield return new WaitForSeconds(0.1f); // Ensure it happens after first frame
    if (OccupiedTilesManager.Instance != null)
    {
        OccupiedTilesManager.Instance.RemoveOccupiedPosition(CurrentTilePosition);
    }
}

    /// <summary>
    /// Resets the Electrician's fatigue at the start of each turn.
    /// </summary>
    public void ResetFatigue()
    {
        fatigue = 2;
        Debug.Log("Electrician: Fatigue reset to 2.");
    }

    /// <summary>
    /// Handles the Electrician's movement and fatigue usage.
    /// </summary>
    public IEnumerator MoveAction()
{
    if (currentRouteIndex >= routePositions.Count)
    {
        yield break; // No more route positions left
    }

    Vector3Int destinationTile = routePositions[currentRouteIndex];

    // Move only if enough fatigue is available
    if (fatigue >= 1)
    {
        List<Vector3Int> path = CalculateOrthPath(CurrentTilePosition, destinationTile);

        int maxSteps = (fatigue >= 1) ? 2 : 0; // Each fatigue allows 2 steps
        if (path.Count > maxSteps)
            path = path.GetRange(0, maxSteps);

        if (path.Count > 0)
        {
            fatigue--; // Deduct 1 fatigue per movement (2 tiles)
            yield return StartCoroutine(MoveAlongPath(path));
        }
        else
        {
            Debug.Log("Electrician: Not enough fatigue to move!");
        }
    }

    // **After moving, check if the panel is there and fix it**
    if (panelObjects.ContainsKey(CurrentTilePosition))
    {
        yield return StartCoroutine(FixPanel());
    }

    // If fully reached the destination, advance the route
    if (CurrentTilePosition == destinationTile)
    {
        currentRouteIndex++;
    }

    yield return null;
}


    /// <summary>
    /// Spawns broken panels at designated route positions.
    /// </summary>
    private void SpawnBrokenPanels()
    {
        if (brokenPanelPrefab == null)
        {
            Debug.LogWarning("ElectricianMove: Broken Panel Prefab is not assigned!");
            return;
        }

        foreach (Vector3Int panelTile in routePositions)
        {
            Vector3 worldPos = tilemap.GetCellCenterWorld(panelTile);
            GameObject brokenPanel = Instantiate(brokenPanelPrefab, worldPos, Quaternion.identity);
            panelObjects[panelTile] = brokenPanel;
        }
    }

    /// <summary>
    /// Fixes the panel at the current position, replacing the broken panel with a fixed one.
    /// </summary>
    public IEnumerator FixPanel()
    {
        // If there is no panel at this position, return
        if (!panelObjects.ContainsKey(CurrentTilePosition))
        {
            yield break;
        }

        // If the panel is already fixed, return
        if (panelObjects[CurrentTilePosition] == null)
        {
            yield break;
        }

        // Check if enough fatigue is available
        if (fatigue < 1)
        {
            Debug.Log("Electrician: Not enough fatigue to fix the panel!");
            yield break;
        }

        Debug.Log($"Electrician: Fixing panel at {CurrentTilePosition}...");
        yield return new WaitForSeconds(1.5f); // Simulate fixing time

        // Replace broken panel with fixed panel
        GameObject brokenPanel = panelObjects[CurrentTilePosition];
        Destroy(brokenPanel); // Remove broken panel

        if (fixedPanelPrefab != null)
        {
            Vector3 worldPos = tilemap.GetCellCenterWorld(CurrentTilePosition);
            GameObject fixedPanel = Instantiate(fixedPanelPrefab, worldPos, Quaternion.identity);
            panelObjects[CurrentTilePosition] = fixedPanel; // Update reference
        }

        fatigue--; // Deduct 1 fatigue for fixing panel
        Debug.Log("Electrician: Panel fixed!");
    }

    /// <summary>
    /// Calculates the best movement path, prioritizing orthogonal movement.
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

        for (int i = 0; i < Mathf.Abs(dx); i++)
        {
            x += stepX;
            Vector3Int nextPos = new Vector3Int(x, y, start.z);
            if (!IsMoveValid(nextPos)) break;
            path.Add(nextPos);
        }

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
            if (OccupiedTilesManager.Instance != null)
                OccupiedTilesManager.Instance.RemoveOccupiedPosition(CurrentTilePosition);

            yield return StartCoroutine(MoveToTile(tile));
            CurrentTilePosition = tile;

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
        if (!tilemap.HasTile(tilePos)) return false;
        if (OccupiedTilesManager.Instance != null && OccupiedTilesManager.Instance.IsTileOccupied(tilePos))
            return false;
        return !IsBlockedByObject(tilePos);
    }

    private bool IsBlockedByObject(Vector3Int tilePos)
    {
        Vector3 checkPos = tilemap.GetCellCenterWorld(tilePos);
        Collider2D[] colliders = Physics2D.OverlapPointAll(checkPos);
        foreach (var col in colliders)
        {
            if (col.CompareTag("Player") || (col.CompareTag("Enemy") && col.gameObject != this.gameObject))
                return true;
        }
        return false;
    }
}
