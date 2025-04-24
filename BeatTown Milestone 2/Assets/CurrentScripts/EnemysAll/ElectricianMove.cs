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

    private All_SFX allSFX; // Reference to the sound manager

    private void Awake()
    {
        allSFX = FindObjectOfType<All_SFX>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (tilemap == null)
            tilemap = FindObjectOfType<Tilemap>();
    }

    private bool HasAnyBrokenPanelsLeft()
    {
        foreach (var entry in panelObjects)
        {
            if (entry.Value != null && entry.Value.CompareTag("BrokenPanel"))
            {
                return true;
            }
        }
        return false;
    }

    private void Start()
    {
        if (tilemap != null)
            CurrentTilePosition = tilemap.WorldToCell(transform.position);

        if (OccupiedTilesManager.Instance != null)
        {
            OccupiedTilesManager.Instance.AddOccupiedPosition(CurrentTilePosition);
        }

        StartCoroutine(RemoveSpawnOccupiedTile());
        SpawnBrokenPanels();
    }

    private IEnumerator RemoveSpawnOccupiedTile()
    {
        yield return new WaitForSeconds(0.1f);
        if (OccupiedTilesManager.Instance != null)
        {
            OccupiedTilesManager.Instance.RemoveOccupiedPosition(CurrentTilePosition);
        }
    }

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

    public IEnumerator FixPanel()
    {
        Vector3 worldPos = tilemap.GetCellCenterWorld(CurrentTilePosition);
        Collider2D[] hits = Physics2D.OverlapBoxAll(worldPos, new Vector2(0.6f, 0.6f), 0f);

        GameObject brokenPanel = null;

        foreach (var hit in hits)
        {
            if (hit.CompareTag("BrokenPanel"))
            {
                brokenPanel = hit.gameObject;
                break;
            }
        }

        if (brokenPanel == null)
        {
            Debug.Log("No broken panel found at current tile.");
            yield break;
        }

        Debug.Log($"[FixPanel] Fixing {brokenPanel.name} at {CurrentTilePosition}");
        allSFX?.PlayPanelFix();

        yield return new WaitForSeconds(1.5f);

        Destroy(brokenPanel);

        if (fixedPanelPrefab != null)
        {
            GameObject fixedPanel = Instantiate(fixedPanelPrefab, worldPos, Quaternion.identity);
            // Optional: update panelObjects dictionary if you still use it
        }

        allSFX?.PlayPanelAdded();
        Debug.Log("Electrician: Panel fixed!");

        if (!HasAnyBrokenPanelsLeft())
        {
            Debug.Log("All panels fixed! Player wins!");
            UnityEngine.SceneManagement.SceneManager.LoadScene("WinScreen");
        }
    }

    public IEnumerator FixPanelAtRoutePosition()
{
    Vector3Int tile = routePositions[currentRouteIndex];

    if (!panelObjects.ContainsKey(tile) || panelObjects[tile] == null)
    {
        Debug.Log("No panel found at route tile.");
        yield break;
    }

    GameObject currentPanel = panelObjects[tile];

    if (!currentPanel.CompareTag("BrokenPanel"))
    {
        Debug.Log("Panel at route tile is already fixed.");
        yield break;
    }

    Debug.Log($"[FixPanel] Fixing panel at route tile {tile}.");
    allSFX?.PlayPanelFix();

    yield return new WaitForSeconds(1.5f);

    Destroy(currentPanel);

    if (fixedPanelPrefab != null)
    {
        Vector3 worldPos = tilemap.GetCellCenterWorld(tile);
        GameObject fixedPanel = Instantiate(fixedPanelPrefab, worldPos, Quaternion.identity);
        panelObjects[tile] = fixedPanel;
    }

    allSFX?.PlayPanelAdded();
    Debug.Log("Electrician: Panel fixed!");

    currentRouteIndex++;

    if (!HasAnyBrokenPanelsLeft())
    {
        Debug.Log("All panels fixed! Player wins!");
        UnityEngine.SceneManagement.SceneManager.LoadScene("WinScreen");
    }
}




    private List<Vector3Int> CalculateOrthPathAvoidingHoles(Vector3Int start, Vector3Int end)
    {
        Queue<Vector3Int> queue = new Queue<Vector3Int>();
        Dictionary<Vector3Int, Vector3Int> cameFrom = new Dictionary<Vector3Int, Vector3Int>();
        Dictionary<Vector3Int, int> costSoFar = new Dictionary<Vector3Int, int>();

        queue.Enqueue(start);
        cameFrom[start] = start;
        costSoFar[start] = 0;

        while (queue.Count > 0)
        {
            Vector3Int current = queue.Dequeue();
            foreach (Vector3Int neighbor in GetNeighbors(current))
            {
                if (!tilemap.HasTile(neighbor)) continue;
                if (!IsMoveValid(neighbor)) continue;

                int moveCost = IsHoleTile(neighbor) ? 2 : 1;
                int newCost = costSoFar[current] + moveCost;

                if (!costSoFar.ContainsKey(neighbor) || newCost < costSoFar[neighbor])
                {
                    costSoFar[neighbor] = newCost;
                    cameFrom[neighbor] = current;
                    queue.Enqueue(neighbor);
                }
            }
        }

        if (!cameFrom.ContainsKey(end)) return new List<Vector3Int>();

        List<Vector3Int> path = new List<Vector3Int>();
        Vector3Int temp = end;
        while (temp != start)
        {
            path.Add(temp);
            temp = cameFrom[temp];
        }
        path.Reverse();
        return path;
    }

    private List<Vector3Int> GetNeighbors(Vector3Int tile)
    {
        return new List<Vector3Int>
        {
            tile + Vector3Int.up,
            tile + Vector3Int.down,
            tile + Vector3Int.left,
            tile + Vector3Int.right
        };
    }

    private bool IsHoleTile(Vector3Int tilePos)
    {
        Vector3 center = tilemap.GetCellCenterWorld(tilePos);
        Collider2D[] hits = Physics2D.OverlapBoxAll(center, new Vector2(0.6f, 0.6f), 0f);

        foreach (var hit in hits)
        {
            if (hit != null && hit.CompareTag("Hole"))
            {
                Debug.Log($"[IsHoleTile] Hit: {hit.name} at {tilePos}");
                return true;
            }
        }

        return false;
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

    if (IsHoleTile(tile))
    {
        Debug.Log($"{name}: Fell into a hole at {tile}.");
        var fatigueScript = GetComponent<ElectricianFatigue>();
        if (fatigueScript != null)
        {
            fatigueScript.prone = true;
        }

        // ✅ Let the move complete, but stop any further movement
        break; // not yield break
    }
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
        if (tilemap == null || !tilemap.HasTile(tilePos))
            return false;

        Vector3 checkPos = tilemap.GetCellCenterWorld(tilePos);
        Collider2D[] hits = Physics2D.OverlapPointAll(checkPos);
        foreach (var col in hits)
        {
            if (col.CompareTag("Player") || col.CompareTag("Enemy"))
                return false;

            if (col.CompareTag("BrokenPanel"))
                continue;
        }
        return true;
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

    public bool IsOnBrokenPanel()
    {
        Vector3 worldPos = tilemap.GetCellCenterWorld(CurrentTilePosition);
        Collider2D[] hits = Physics2D.OverlapBoxAll(worldPos, new Vector2(0.6f, 0.6f), 0f);

        foreach (var hit in hits)
        {
            if (hit.CompareTag("BrokenPanel"))
                return true;
        }
        return false;
    }

    public IEnumerator MoveUsingFatigue(int fatigueToSpend, bool limitToOneTile = false)
    {
        int stepsAllowed = limitToOneTile
            ? 1
            : Mathf.Min(fatigueToSpend, moveDistance);

        if (currentRouteIndex >= routePositions.Count)
            yield break;

        Vector3Int destinationTile = routePositions[currentRouteIndex];
        List<Vector3Int> path = CalculateOrthPathAvoidingHoles(CurrentTilePosition, destinationTile);

        if (path.Count > stepsAllowed)
            path = path.GetRange(0, stepsAllowed);

        Debug.Log($"{name} MoveUsingFatigue: stepsAllowed={stepsAllowed}, pathCount={path.Count}");

        bool fellInHole = false;
        foreach (Vector3Int tile in path)
        {
            OccupiedTilesManager.Instance?.RemoveOccupiedPosition(CurrentTilePosition);

            yield return StartCoroutine(MoveToTile(tile));
            CurrentTilePosition = tile;

            OccupiedTilesManager.Instance?.AddOccupiedPosition(CurrentTilePosition);

            if (IsHoleTile(tile))
            {
                Debug.Log($"{name}: Fell into a hole at {tile}!");
                var fatigueScript = GetComponent<ElectricianFatigue>();
                if (fatigueScript != null)
                    fatigueScript.prone = true;

                fellInHole = true;
                break;
            }
        }

        if (fellInHole)
            yield break;
    }

    public GameObject GetPanelAtTile(Vector3Int tile)
        {
            if (panelObjects.ContainsKey(tile))
                return panelObjects[tile];
            return null;
        }

}
