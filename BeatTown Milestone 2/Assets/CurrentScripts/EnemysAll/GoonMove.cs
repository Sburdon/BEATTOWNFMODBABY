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
    private StateMachine stateMachine;

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

    public bool InPuddle;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        stateMachine = GetComponent<StateMachine>();

        if (tilemap == null)
            tilemap = FindObjectOfType<Tilemap>();

        if (electricianMove == null)
            electricianMove = FindObjectOfType<ElectricianMove>();

        if (electricianHealth == null && electricianMove != null)
            electricianHealth = electricianMove.GetComponent<ElectricianHealth>();

        if (enemyHealth == null)
            enemyHealth = GetComponent<EnemyHealth>();
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
        if (tilemap == null) return path;
        if (start == goal) return path;

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

        if (!cameFrom.ContainsKey(goal))
            return path;

        var temp = goal;
        while (temp != start)
        {
            path.Add(temp);
            temp = cameFrom[temp];
        }
        path.Reverse();

        if (path.Count > maxSteps)
            path = path.GetRange(0, maxSteps);

        return path;
    }

    private Vector3Int FindAdjacentGoalTile(Vector3Int electricianTile)
    {
        if (IsAdjacentToElectrician())
            return CurrentTilePosition;

        foreach (var n in GetNeighboringTiles(electricianTile))
        {
            if (IsMoveValid(n)) return n;
        }
        return CurrentTilePosition;
    }

    private IEnumerator MoveToTile(Vector3Int tilePos)
    {
        Vector3 startPos = transform.position;
        Vector3 endPos = tilemap.GetCellCenterWorld(tilePos);
        float elapsed = 0f;
        float travelTime = 1f / moveSpeed;
        stateMachine.ChangeState(StateMachine.WrestlerState.Move);

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
        stateMachine.ChangeState(StateMachine.WrestlerState.Idle);
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
        if (isPunchCharging && electricianMove != null && tilemap != null)
        {
            punchTargetTile = electricianMove.CurrentTilePosition;

            if (punchIndicatorInstance != null)
            {
                punchIndicatorInstance.transform.position = tilemap.GetCellCenterWorld(punchTargetTile);
            }
            Debug.Log($"{name}: Punch target updated to player's tile after being punched.");
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
}
