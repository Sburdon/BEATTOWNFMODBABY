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
    private GameObject punchIndicatorInstance;
    public bool IsPunchCharging => isPunchCharging;

    private bool canMove;
    public bool InPuddle;

    // ----------------------------------------------------
    //  AWAKE: attempt to auto-find references if not set
    // ----------------------------------------------------
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

    // ----------------------------------------------------
    //  START: Additional fallback for references
    // ----------------------------------------------------
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

    // --------------------------------------------------------------------
    // Movement with BFS to a tile adjacent to the Electrician (not the same tile).
    // --------------------------------------------------------------------
    public IEnumerator MoveUsingFatigue(int fatigue)
    {
        if (electricianMove == null)
        {
            Debug.LogWarning($"{name}: No ElectricianMove found, cannot chase.");
            yield break;
        }

        int stepsAllowed = fatigue * 2;

        Vector3Int startPos = CurrentTilePosition;
        Vector3Int electricianTile = electricianMove.CurrentTilePosition;

        // Instead of BFS to the Electrician's tile (which may be "occupied"),
        // we'll BFS until we find a tile that is "adjacent to ElectricianTile."
        // Then we treat that tile as our BFS "goal."
        Vector3Int goal = FindAdjacentGoalTile(electricianTile);
        if (goal == startPos)
        {
            // if we are already adjacent, no BFS needed
            Debug.Log($"{name}: Already adjacent to Electrician; no BFS needed.");
            yield break;
        }

        // Actually run BFS from 'startPos' to 'goal'
        List<Vector3Int> path = BFSPathToTile(startPos, goal, stepsAllowed);

        if (path.Count == 0)
        {
            Debug.Log($"{name}: No valid BFS path found to adjacency. Standing still.");
            yield break;
        }

        // Follow that path
        foreach (var stepTile in path)
        {
            // Unoccupy old tile
            if (OccupiedTilesManager.Instance != null)
                OccupiedTilesManager.Instance.RemoveOccupiedPosition(CurrentTilePosition);

            yield return StartCoroutine(MoveToTile(stepTile));

            CurrentTilePosition = stepTile;

            if (OccupiedTilesManager.Instance != null)
                OccupiedTilesManager.Instance.AddOccupiedPosition(CurrentTilePosition);

            // If we become adjacent at any point, can stop early
            if (IsAdjacentToElectrician())
            {
                Debug.Log($"{name}: Stopped BFS early upon reaching adjacency.");
                break;
            }
        }

        yield return null;
    }

    // --------------------------------------------------------------------
    // BFS from 'start' to 'goal', limited to maxSteps.
    // --------------------------------------------------------------------
    private List<Vector3Int> BFSPathToTile(Vector3Int start, Vector3Int goal, int maxSteps)
    {
        var path = new List<Vector3Int>();
        if (tilemap == null) return path;
        if (start == goal) return path;

        // BFS structures
        var queue = new Queue<Vector3Int>();
        var cameFrom = new Dictionary<Vector3Int, Vector3Int>();

        queue.Enqueue(start);
        cameFrom[start] = start; // visited

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (current == goal)
                break;

            foreach (var neighbor in GetNeighboringTiles(current))
            {
                if (!cameFrom.ContainsKey(neighbor) && IsMoveValid(neighbor))
                {
                    cameFrom[neighbor] = current;
                    queue.Enqueue(neighbor);
                }
            }
        }

        // If BFS never visited 'goal', no path
        if (!cameFrom.ContainsKey(goal))
            return path;

        // Reconstruct path
        var temp = goal;
        while (temp != start)
        {
            path.Add(temp);
            temp = cameFrom[temp];
        }
        path.Reverse();

        // If path is longer than maxSteps, trim
        if (path.Count > maxSteps)
            path = path.GetRange(0, maxSteps);

        return path;
    }

    // --------------------------------------------------------------------
    //  Find a tile that is adjacent to 'electricianTile' and also valid
    // --------------------------------------------------------------------
    private Vector3Int FindAdjacentGoalTile(Vector3Int electricianTile)
    {
        // If we're *already* adjacent, just return CurrentTilePosition
        if (IsAdjacentToElectrician())
            return CurrentTilePosition;

        // Check all 4 neighbors of the Electrician's tile
        foreach (var n in GetNeighboringTiles(electricianTile))
        {
            // If it's valid for movement, that can be our BFS "goal"
            if (IsMoveValid(n))
            {
                return n;
            }
        }

        // If all adjacent tiles are invalid/occupied, BFS won't succeed anyway.
        // Return current tile as a fallback to skip BFS entirely.
        return CurrentTilePosition;
    }

    // --------------------------------------------------------------------
    // Move smoothly to a tile
    // --------------------------------------------------------------------
    private IEnumerator MoveToTile(Vector3Int tilePos)
    {
        Vector3 startPos = transform.position;
        Vector3 endPos = tilemap.GetCellCenterWorld(tilePos);
        float elapsed = 0f;
        float travelTime = 1f / moveSpeed;

        // Flip sprite if needed
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

    // --------------------------------------------------------------------
    // Return orth-adj neighbors
    // --------------------------------------------------------------------
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

    // --------------------------------------------------------------------
    // Check if tile is valid
    // --------------------------------------------------------------------
    private bool IsMoveValid(Vector3Int tilePos)
    {
        if (tilemap == null || !tilemap.HasTile(tilePos))
            return false;

        if (OccupiedTilesManager.Instance != null &&
            OccupiedTilesManager.Instance.IsTileOccupied(tilePos))
        {
            return false;
        }

        return true;
    }

    // --------------------------------------------------------------------
    // Adjacent to Electrician?
    // --------------------------------------------------------------------
    public bool IsAdjacentToElectrician()
    {
        if (electricianMove == null) return false;
        Vector3Int eTile = electricianMove.CurrentTilePosition;
        int dx = Mathf.Abs(CurrentTilePosition.x - eTile.x);
        int dy = Mathf.Abs(CurrentTilePosition.y - eTile.y);
        return (dx + dy) == 1;
    }

    // --------------------------------------------------------------------
    // Punch Setup
    // --------------------------------------------------------------------
    public void SetupPunch()
    {
        if (isPunchCharging) return;
        isPunchCharging = true;

        Vector3Int eTile = (electricianMove != null)
            ? electricianMove.CurrentTilePosition
            : CurrentTilePosition;

        punchTargetTile = eTile;

        // Spawn telegraph (red square)
        if (punchIndicatorPrefab != null && tilemap != null)
        {
            Vector3 spawnPos = tilemap.GetCellCenterWorld(punchTargetTile);
            punchIndicatorInstance = Instantiate(punchIndicatorPrefab, spawnPos, Quaternion.identity);
        }

        Debug.Log($"{name} is telegraphing a punch at tile {punchTargetTile}!");
    }

    // --------------------------------------------------------------------
    // Punch Resolve
    // --------------------------------------------------------------------
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

        if (!hitSomeone)
        {
            // Miss => spawn hole
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
}
