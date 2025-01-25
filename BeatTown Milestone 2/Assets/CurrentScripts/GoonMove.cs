using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

/// <summary>
/// A very simple Goon movement script with:
/// - Strict x-first-then-y approach (no BFS).
/// - Up to (fatigue * 2) tiles each MoveUsingFatigue call.
/// - Punch setup/resolve logic remains unchanged.
/// </summary>
public class GoonMove : MonoBehaviour
{
    [Header("Tilemap & Electrician References")]
    public Tilemap tilemap;
    public ElectricianMove electricianMove;    
    public ElectricianHealth electricianHealth; // optional for direct damage

    [Header("Movement Speed")]
    [Tooltip("Movement speed: time in seconds to move 1 tile.")]
    public float moveSpeed = 1f;

    // Track current tile position in the grid
    [HideInInspector]
    public Vector3Int CurrentTilePosition;

    [Header("Health (Optional)")]
    public EnemyHealth enemyHealth; // If the Goon itself can die

    private SpriteRenderer spriteRenderer;

    [Header("Punch Settings")]
    [Tooltip("Damage dealt by the Goon's punch.")]
    public int punchDamage = 2;

    [Tooltip("Prefab to spawn if the punch misses, creating a hole.")]
    public GameObject holePrefab;

    [Tooltip("Indicator prefab for the punch target area.")]
    public GameObject punchIndicatorPrefab;

    // Internal punch-charging state
    private bool isPunchCharging = false;
    private Vector3Int punchTargetTile;
    private GameObject punchIndicatorInstance;

    // Public so other scripts can check whether Goon is charging a punch
    public bool IsPunchCharging => isPunchCharging;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (tilemap == null)
        {
            tilemap = FindObjectOfType<Tilemap>();
            if (tilemap == null)
                Debug.LogError($"GoonMove: No Tilemap found for {name}!");
        }

        if (electricianMove == null)
        {
            electricianMove = FindObjectOfType<ElectricianMove>();
        }
        if (electricianHealth == null && electricianMove != null)
        {
            electricianHealth = electricianMove.GetComponent<ElectricianHealth>();
        }

        if (enemyHealth == null)
        {
            enemyHealth = GetComponent<EnemyHealth>();
        }
    }

    private void Start()
    {
        // If still missing references, attempt to find them
        if (electricianMove == null)
        {
            electricianMove = FindObjectOfType<ElectricianMove>();
            if (electricianMove != null && electricianHealth == null)
            {
                electricianHealth = electricianMove.GetComponent<ElectricianHealth>();
            }
        }

        // Convert our starting world position to a tile coordinate
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
    // Movement (NO BFS) - Strict x-first-then-y approach
    // --------------------------------------------------------------------
    /// <summary>
    /// Moves the Goon up to (fatigue * 2) tiles in a direct x-first, then y approach.
    /// If a tile is blocked (OccupiedTilesManager) or outside tilemap, we stop early.
    /// </summary>
// Store tiles the AI cannot return to until it contacts the Electrician
public IEnumerator MoveUsingFatigue(int fatigue)
{
    if (electricianMove == null)
    {
        Debug.LogWarning("GoonMove: No Electrician to chase!");
        yield break;
    }

    int stepsAllowed = fatigue * 2;
    Vector3Int lastPosition = CurrentTilePosition; // Keep track of the last position to prevent backtracking

    for (int i = 0; i < stepsAllowed; i++)
    {
        if (IsAdjacentToElectrician())
        {
            Debug.Log($"{name}: Reached the Electrician, stopping movement.");
            break; // Stop moving if adjacent to the Electrician
        }

        // Calculate the next move based on the heuristic
        Vector3Int nextTile = GetNextMoveTowardsElectrician(lastPosition);

        if (nextTile == CurrentTilePosition)
        {
            Debug.Log($"{name}: No valid moves, stopping movement.");
            break; // No valid moves, stop movement
        }

        // Update occupied tiles and move
        if (OccupiedTilesManager.Instance != null)
        {
            OccupiedTilesManager.Instance.RemoveOccupiedPosition(CurrentTilePosition);
        }

        yield return StartCoroutine(MoveToTile(nextTile));
        lastPosition = CurrentTilePosition; // Update the last position
        CurrentTilePosition = nextTile;

        if (OccupiedTilesManager.Instance != null)
        {
            OccupiedTilesManager.Instance.AddOccupiedPosition(CurrentTilePosition);
        }
    }

    yield return null;
}

/// <summary>
/// Determines the next move toward the Electrician, avoiding the last position.
/// </summary>
private Vector3Int GetNextMoveTowardsElectrician(Vector3Int lastPosition)
{
    Vector3Int target = electricianMove.CurrentTilePosition;
    List<Vector3Int> possibleMoves = GetNeighboringTiles(CurrentTilePosition);

    // Sort possible moves by proximity to the Electrician
    possibleMoves.Sort((a, b) =>
    {
        float distanceA = Vector3Int.Distance(a, target);
        float distanceB = Vector3Int.Distance(b, target);
        return distanceA.CompareTo(distanceB);
    });

    // Choose the first valid move that isn't the last position
    foreach (var move in possibleMoves)
    {
        if (IsMoveValid(move) && move != lastPosition)
        {
            return move;
        }
    }

    // No valid moves found
    return CurrentTilePosition;
}

/// <summary>
/// Returns a list of neighboring tiles in cardinal directions.
/// </summary>
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

/// <summary>
/// Validates whether the AI can move onto the specified tile.
/// </summary>
private bool IsMoveValid(Vector3Int tilePos)
{
    if (tilemap == null || !tilemap.HasTile(tilePos)) return false;

    if (OccupiedTilesManager.Instance != null &&
        OccupiedTilesManager.Instance.IsTileOccupied(tilePos))
    {
        return false; // Tile is occupied
    }

    return true; // Tile is valid
}

/// <summary>
/// Moves the AI to the specified tile.
/// </summary>
private IEnumerator MoveToTile(Vector3Int tilePos)
{
    Vector3 startPos = transform.position;
    Vector3 endPos = tilemap.GetCellCenterWorld(tilePos);
    float elapsed = 0f;
    float travelTime = 1f / moveSpeed; // Adjust move speed here

    // Flip sprite horizontally if needed
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
    // Punch Handling
    // --------------------------------------------------------------------
    public bool IsAdjacentToElectrician()
    {
        if (electricianMove == null) return false;
        Vector3Int eTile = electricianMove.CurrentTilePosition;
        int dx = Mathf.Abs(CurrentTilePosition.x - eTile.x);
        int dy = Mathf.Abs(CurrentTilePosition.y - eTile.y);
        return (dx + dy) == 1;
    }

    public void SetupPunch()
    {
        if (isPunchCharging) return;
        isPunchCharging = true;

        Vector3Int eTile = (electricianMove != null)
            ? electricianMove.CurrentTilePosition
            : CurrentTilePosition;

        punchTargetTile = eTile;

        if (punchIndicatorPrefab != null && tilemap != null)
        {
            Vector3 indicatorPos = tilemap.GetCellCenterWorld(eTile);
            punchIndicatorInstance = Instantiate(punchIndicatorPrefab, indicatorPos, Quaternion.identity);
        }

        Debug.Log($"{name} is charging punch at tile {punchTargetTile}!");
    }

    /// <summary>
    /// Called at the end of Player's turn to finalize the punch damage or create a hole if missed.
    /// </summary>
    public void ResolvePunch()
    {
        if (!isPunchCharging) return;
        isPunchCharging = false;

        // Remove punch indicator
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
            // Check Electrician
            ElectricianHealth eh = col.GetComponent<ElectricianHealth>();
            if (eh != null && eh.currentHealth > 0)
            {
                eh.TakeDamage(punchDamage);
                hitSomeone = true;
                Debug.Log($"{name} punched Electrician for {punchDamage}!");
            }

            // Or other potential targets
            PlayerHealth pHealth = col.GetComponent<PlayerHealth>();
            if (pHealth != null)
            {
                pHealth.TakeDamage(punchDamage);
                hitSomeone = true;
                Debug.Log($"{name} punched Player for {punchDamage}!");
            }
        }

        if (!hitSomeone && holePrefab != null)
        {
            // Miss => spawn hole
            Instantiate(holePrefab, worldPos, Quaternion.identity);
            if (OccupiedTilesManager.Instance != null)
            {
                OccupiedTilesManager.Instance.AddOccupiedPosition(punchTargetTile);
            }
            Debug.Log($"{name} missed punch. Created hole at {punchTargetTile}.");
        }
    }

    public void ResetGoon()
    {
        StopAllCoroutines();
    }
}
