using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GoonMove : MonoBehaviour
{
    [Header("Tilemap & Electrician References")]
    public Tilemap tilemap;
    public ElectricianMove electricianMove;
    public ElectricianHealth electricianHealth;

    [Header("Movement Speed")]
    [Tooltip("Movement speed: time in seconds to move 1 tile.")]
    public float moveSpeed = 1f;

    [HideInInspector]
    public Vector3Int CurrentTilePosition;

    [Header("Health (Optional)")]
    public EnemyHealth enemyHealth;

    private SpriteRenderer spriteRenderer;

    [Header("Punch Settings")]
    [Tooltip("Damage dealt by the Goon's punch.")]
    public int punchDamage = 2;

    [Tooltip("Prefab to spawn if the punch misses (e.g. hole).")]
    public GameObject holePrefab;

    [Tooltip("Prefab to spawn for punch telegraph (e.g. red square).")]
    public GameObject punchIndicatorPrefab;

    // Punch-charging state
    private bool isPunchCharging = false;
    private Vector3Int punchTargetTile;
    public GameObject punchIndicatorInstance;
    public bool IsPunchCharging => isPunchCharging;

    public bool InPuddle;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (tilemap == null)
            tilemap = FindObjectOfType<Tilemap>();

        if (electricianMove == null)
            electricianMove = FindObjectOfType<ElectricianMove>();

        if (electricianHealth == null && electricianMove != null)
            electricianHealth = electricianMove.GetComponent<ElectricianHealth>();

        if (enemyHealth == null)
            enemyHealth = GetComponent<EnemyHealth>();
    }

        public void CancelPunch()
    {
        isPunchCharging = false;

        if (punchIndicatorInstance != null)
        {
            Destroy(punchIndicatorInstance);
            punchIndicatorInstance = null;
        }

        Debug.Log($"{name}: Punch canceled.");
    }


    private void Start()
    {
        // If STILL no Electrician reference, try by tag
        if (electricianMove == null)
        {
            GameObject eObj = GameObject.FindWithTag("Electrician");
            if (eObj != null)
            {
                electricianMove = eObj.GetComponent<ElectricianMove>();
                electricianHealth = eObj.GetComponent<ElectricianHealth>();
            }
        }

        if (tilemap == null)
            tilemap = FindObjectOfType<Tilemap>();

        if (tilemap != null)
            CurrentTilePosition = tilemap.WorldToCell(transform.position);

        // Mark this initial tile as occupied
        if (OccupiedTilesManager.Instance != null)
        {
            OccupiedTilesManager.Instance.AddOccupiedPosition(CurrentTilePosition);
            Debug.Log($"{name} (Goon) => Occupied tile {CurrentTilePosition} at start.");
        }
    }

    // -----------------------------------------------------------------------------------
    // Movement with BFS
    // -----------------------------------------------------------------------------------
    public IEnumerator MoveUsingFatigue(int fatigue, bool limitToOneTile = false)
    {
        int stepsAllowed = limitToOneTile ? 1 : fatigue * 2;

        if (electricianMove == null)
        {
            Debug.LogWarning($"{name}: No ElectricianMove found, cannot chase.");
            yield break;
        }

        Vector3Int startPos = CurrentTilePosition;
        Vector3Int electricianTile = electricianMove.CurrentTilePosition;

        Vector3Int goal = FindAdjacentGoalTile(electricianTile);
        if (goal == startPos)
        {
            Debug.Log($"{name}: Already adjacent to Electrician; no BFS needed.");
            yield break;
        }

        List<Vector3Int> path = BFSPathToTile(startPos, goal, stepsAllowed);

        if (path.Count == 0)
        {
            Debug.Log($"{name}: No valid BFS path found. Standing still.");
            yield break;
        }

        foreach (var stepTile in path)
        {
            if (OccupiedTilesManager.Instance != null)
                OccupiedTilesManager.Instance.RemoveOccupiedPosition(CurrentTilePosition);

            yield return StartCoroutine(MoveToTile(stepTile));

            CurrentTilePosition = stepTile;

            if (OccupiedTilesManager.Instance != null)
                OccupiedTilesManager.Instance.AddOccupiedPosition(CurrentTilePosition);

            // ✅ NEW: Check if Goon fell into a hole mid-move and is now prone
            GoonFatigue fatigueComp = GetComponent<GoonFatigue>();
            if (fatigueComp != null && fatigueComp.prone)
            {
                Debug.Log($"{name} became prone after stepping into a hole. Ending movement early.");
                yield break;
            }

            if (IsAdjacentToElectrician())
            {
                Debug.Log($"{name}: Reached adjacency early, stopping BFS.");
                break;
            }
        }

        yield return null;
}


    private List<Vector3Int> BFSPathToTile(Vector3Int start, Vector3Int goal, int maxSteps)
{
    var path = new List<Vector3Int>();
    if (tilemap == null || start == goal)
        return path;

    var queue = new Queue<Vector3Int>();
    var cameFrom = new Dictionary<Vector3Int, Vector3Int>();
    var costSoFar = new Dictionary<Vector3Int, int>();

    queue.Enqueue(start);
    cameFrom[start] = start;
    costSoFar[start] = 0;

    while (queue.Count > 0)
    {
        var current = queue.Dequeue();

        foreach (var neighbor in GetNeighboringTiles(current))
        {
            if (!tilemap.HasTile(neighbor))
                continue;

            // Skip occupied tiles
            if (OccupiedTilesManager.Instance != null &&
                OccupiedTilesManager.Instance.IsTileOccupied(neighbor))
                continue;

            // Hole penalty
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

    // If no path to goal was found, fallback to closest reachable tile
    if (!cameFrom.ContainsKey(goal))
    {
        Debug.LogWarning($"{name}: No valid path to goal {goal}. Trying nearest reachable tile...");

        // Try any tile adjacent to the electrician
        Vector3Int best = start;
        int bestCost = int.MaxValue;

        foreach (var alt in GetNeighboringTiles(goal))
        {
            if (cameFrom.ContainsKey(alt) && costSoFar[alt] < bestCost)
            {
                best = alt;
                bestCost = costSoFar[alt];
            }
        }

        if (best == start)
        {
            Debug.Log($"{name}: Couldn't find any adjacent tile either. Skipping move.");
            return path;
        }

        goal = best;
    }

    // Reconstruct path from goal
    var temp = goal;
    while (temp != start)
    {
        path.Add(temp);
        temp = cameFrom[temp];
    }
    path.Reverse();

    // Limit to maxSteps allowed
    if (path.Count > maxSteps)
        path = path.GetRange(0, maxSteps);

    return path;
}


    private Vector3Int FindAdjacentGoalTile(Vector3Int electricianTile)
{
    Vector3Int bestTile = CurrentTilePosition;
    int bestCost = int.MaxValue;

    foreach (var n in GetNeighboringTiles(electricianTile))
    {
        if (!tilemap.HasTile(n) || !IsMoveValid(n))
            continue;

        // Use a very short BFS just to calculate path cost
        List<Vector3Int> testPath = BFSPathToTile(CurrentTilePosition, n, 999);
        if (testPath.Count > 0 && testPath.Count < bestCost)
        {
            bestCost = testPath.Count;
            bestTile = n;
        }
    }

    return bestTile;
}


    private IEnumerator MoveToTile(Vector3Int tilePos)
    {
        Vector3 startPos = transform.position;
        Vector3 endPos = tilemap.GetCellCenterWorld(tilePos);
        float elapsed = 0f;
        float travelTime = 1f / moveSpeed;

        if (tilePos.x < CurrentTilePosition.x)
            spriteRenderer.flipX = true;
        else if (tilePos.x > CurrentTilePosition.x)
            spriteRenderer.flipX = false;

        while (elapsed < travelTime)
        {
            transform.position = Vector3.Lerp(startPos, endPos, elapsed / travelTime);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = endPos;
    }

    private List<Vector3Int> GetNeighboringTiles(Vector3Int position)
    {
        return new List<Vector3Int>
        {
            position + Vector3Int.right,
            position + Vector3Int.left,
            position + Vector3Int.up,
            position + Vector3Int.down
        };
    }

    private bool IsMoveValid(Vector3Int tilePos)
    {
        if (tilemap == null || !tilemap.HasTile(tilePos))
            return false;

        if (OccupiedTilesManager.Instance != null &&
            OccupiedTilesManager.Instance.IsTileOccupied(tilePos))
        {
            return false;
        }

        // Hole tiles are allowed, but should be deprioritized — still return true
        return true;
    }


    public bool IsAdjacentToElectrician()
    {
        if (electricianMove == null) return false;
        Vector3Int eTile = electricianMove.CurrentTilePosition;
        int dx = Mathf.Abs(CurrentTilePosition.x - eTile.x);
        int dy = Mathf.Abs(CurrentTilePosition.y - eTile.y);
        return (dx + dy) == 1;
    }

    // -----------------------------------------------------------------------------------
    // PUNCH SETUP / RESOLVE
    // -----------------------------------------------------------------------------------
    public void SetupPunch()
    {
        if (isPunchCharging) return;
        isPunchCharging = true;

        Vector3Int eTile = electricianMove ? electricianMove.CurrentTilePosition : CurrentTilePosition;

        punchTargetTile = eTile;

        if (punchIndicatorInstance == null && punchIndicatorPrefab != null && tilemap != null)
        {
            Vector3 spawnPos = tilemap.GetCellCenterWorld(punchTargetTile);
            punchIndicatorInstance = Instantiate(punchIndicatorPrefab, spawnPos, Quaternion.identity);
        }
        Debug.Log($"{name} is telegraphing a punch at tile {punchTargetTile}!");
    }

    public void ResolvePunch()
{
    if (!isPunchCharging) return;
    isPunchCharging = false;

    if (punchIndicatorInstance != null)
    {
        Destroy(punchIndicatorInstance);
        punchIndicatorInstance = null;
    }

    if (tilemap == null) return;

    Vector3 worldPos = tilemap.GetCellCenterWorld(punchTargetTile);
    Collider2D[] hits = Physics2D.OverlapPointAll(worldPos);
    bool hitSomeone = false;

    foreach (var col in hits)
    {
        // Check if the player was hit
        PlayerHealth playerHealth = col.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(punchDamage);
            Debug.Log($"{name} punched the Player for {punchDamage} damage!");
            hitSomeone = true;
            break; // Exit loop since we hit the player
        }

        // Check if the Electrician was hit (keep existing logic)
        if (col.CompareTag("Electrician"))
        {
            if (electricianHealth != null)
            {
                electricianHealth.TakeDamage(punchDamage);
                Debug.Log($"{name} punched the Electrician for {punchDamage} damage!");
            }
            hitSomeone = true;
            break;
        }
    }

    // If the punch hit no one, spawn a hole (keep original functionality)
    if (!hitSomeone)
    {
        if (holePrefab != null)
        {
            Instantiate(holePrefab, worldPos, Quaternion.identity);

            if (OccupiedTilesManager.Instance != null)
            {
                OccupiedTilesManager.Instance.AddOccupiedPosition(punchTargetTile);
                Debug.Log($"{name} missed punch, created hole, marking {punchTargetTile} occupied.");
            }
        }
        else
        {
            Debug.Log($"{name} missed the punch, no holePrefab assigned.");
        }
    }
}


    // -----------------------------------------------------------------------------------
    //  METHODS FOR FORCED MOVEMENT (BY PLAYER)
    // -----------------------------------------------------------------------------------

    /// <summary>
    /// Called if the Goon is punched by the player. If it's telegraphing a punch,
    /// retarget the Electrician's position (so it tries to punch the player back).
    /// </summary>
public void OnPunchedByPlayer()
{
    Debug.Log($"{name} was punched! Checking if it is telegraphing a punch...");

    // Find the actual Player object
    PlayerMove player = FindObjectOfType<PlayerMove>();

    if (isPunchCharging && player != null && tilemap != null)
    {
        // Get the player's current tile position
        Vector3Int playerTile = tilemap.WorldToCell(player.transform.position);
        Debug.Log($"{name}: Player's ACTUAL position is {playerTile}, updating punch target...");

        // Ensure that we are actually setting a new target
        if (playerTile != punchTargetTile)
        {
            punchTargetTile = playerTile;
            Debug.Log($"{name}: Punch target CHANGED to {punchTargetTile}!");
        }
        else
        {
            Debug.Log($"{name}: Punch target is the same, something might be wrong.");
        }

        // Force the visual update
        UpdatePunchTelegraph();
    }
    else
    {
        Debug.Log($"{name}: Not telegraphing a punch OR Player not found, so nothing changed.");
    }
}






    /// <summary>
    /// Called if the Goon is swung by the player. We pass the old tile (before the move)
    /// and the new tile, so we can figure out the correct offset for the punch target.
    /// </summary>
    public void OnSwungByPlayer(Vector3Int oldTile, Vector3Int newTile)
    {
        if (isPunchCharging && tilemap != null)
        {
            Vector3Int offset = punchTargetTile - oldTile; 
            punchTargetTile = newTile + offset;

            if (punchIndicatorInstance != null)
            {
                punchIndicatorInstance.transform.position = tilemap.GetCellCenterWorld(punchTargetTile);
            }
            Debug.Log($"{name}: Punch telegraph repositioned after being swung by the player.");
        }
    }

    /// <summary>
    /// Called if the Goon is pushed by the player. Same idea: we pass old tile
    /// and new tile, so we can recalc the offset for the punch telegraph.
    /// </summary>
    public void OnPushedByPlayer(Vector3Int oldTile, Vector3Int newTile)
    {
        if (isPunchCharging && tilemap != null)
        {
            Vector3Int offset = punchTargetTile - oldTile;
            punchTargetTile = newTile + offset;

            if (punchIndicatorInstance != null)
            {
                punchIndicatorInstance.transform.position = tilemap.GetCellCenterWorld(punchTargetTile);
            }
            Debug.Log($"{name}: Punch telegraph repositioned after being pushed by the player.");
        }
    }

    public void UpdatePunchTelegraph()
    {
    if (tilemap == null) return;

    Vector3 worldPos = tilemap.GetCellCenterWorld(punchTargetTile);

    if (punchIndicatorInstance != null)
    {
        // Move the existing indicator
        punchIndicatorInstance.transform.position = worldPos;
        Debug.Log($"{name}: Punch indicator MOVED to {punchTargetTile}");
    }
    else if (punchIndicatorPrefab != null)
    {
        // If indicator was somehow destroyed, create a new one
        punchIndicatorInstance = Instantiate(punchIndicatorPrefab, worldPos, Quaternion.identity);
        Debug.Log($"{name}: Punch indicator RECREATED at {punchTargetTile}");
    }
    else
    {
        Debug.LogWarning($"{name}: Punch indicator prefab is NULL! Make sure it's assigned.");
    }
    }

    private bool IsHoleTile(Vector3Int tilePos)
    {
        Vector3 worldPos = tilemap.GetCellCenterWorld(tilePos);
        Collider2D[] hits = Physics2D.OverlapPointAll(worldPos);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Hole"))
            {
                return true;
            }
        }
        return false;
    }


}
