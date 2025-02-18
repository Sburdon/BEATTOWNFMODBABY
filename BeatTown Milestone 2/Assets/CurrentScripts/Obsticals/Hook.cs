using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Hook : MonoBehaviour
{
    public static Hook Instance { get; private set; }

    [Header("References")]
    public Tilemap tilemap;
    public PlayerMove player;
    public Text fishCountText;
    public int hookKillCount = 0;

    [Header("Audio")]
    public All_SFX All_SFX;

    private Vector3Int hookPosition;
    private bool barraSpawned = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        hookPosition = tilemap.WorldToCell(transform.position);
        OccupiedTilesManager.Instance.AddOccupiedPosition(hookPosition);
        UpdateFishCountText();
    }

    private void UpdateFishCountText()
    {
        if (fishCountText != null)
        {
            fishCountText.text = $"Fish Caught: {hookKillCount}";
        }
        else
        {
            Debug.LogError("FishCountText is not assigned!");
        }
    }

    public Vector3Int GetHookPosition()
    {
        return tilemap.WorldToCell(transform.position);
    }

    private void RespawnHook()
    {
        Vector3Int oldHookPosition = tilemap.WorldToCell(transform.position);
        OccupiedTilesManager.Instance.RemoveOccupiedPosition(oldHookPosition); // Ensure the old position is removed

        // Attempt to get a random available position for the hook
        Vector3Int newHookPosition = OccupiedTilesManager.Instance.GetRandomAvailablePosition(player.CurrentTilePosition);

        // Fallback if no available position is found
        if (newHookPosition == Vector3Int.zero)
        {
            Debug.LogWarning("Hook: No available positions found to respawn. Trying nearest available tile.");

            // Retry by finding the first available tile in tilemap bounds
            newHookPosition = FindFirstAvailableTile(player.CurrentTilePosition);
        }

        if (newHookPosition == Vector3Int.zero)
        {
            Debug.LogError("Hook: No available positions even after fallback. Hook will not respawn.");
            return;
        }

        // Update hook position and occupied position
        Vector3 worldPosition = tilemap.GetCellCenterWorld(newHookPosition);
        transform.position = worldPosition;
        OccupiedTilesManager.Instance.AddOccupiedPosition(newHookPosition);

        Debug.Log($"Hook respawned at {newHookPosition}");
    }

    // Helper method to find the first available tile within tilemap bounds
    private Vector3Int FindFirstAvailableTile(Vector3Int playerPosition)
    {
        BoundsInt bounds = tilemap.cellBounds;

        for (int x = bounds.xMin; x <= bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y <= bounds.yMax; y++)
            {
                Vector3Int tilePosition = new Vector3Int(x, y, 0);
                if (tilemap.HasTile(tilePosition) &&
                    !OccupiedTilesManager.Instance.IsTileOccupied(tilePosition) &&
                    Mathf.Abs(tilePosition.x - playerPosition.x) + Mathf.Abs(tilePosition.y - playerPosition.y) >= 2) // Ensures at least 2 tiles away from player
                {
                    return tilePosition; // Return the first available tile found
                }
            }
        }
        return Vector3Int.zero; // Return zero if no available position is found
    }





    // Fallback method to find the nearest available position within tilemap bounds
    private Vector3Int FindNearestAvailablePosition(Vector3Int referencePosition)
    {
        BoundsInt bounds = tilemap.cellBounds;

        // Iterate over the tilemap bounds to find the first unoccupied tile that meets the criteria
        for (int x = bounds.xMin; x <= bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y <= bounds.yMax; y++)
            {
                Vector3Int tilePosition = new Vector3Int(x, y, 0);
                if (tilemap.HasTile(tilePosition) &&
                    !OccupiedTilesManager.Instance.IsTileOccupied(tilePosition) &&
                    Mathf.Abs(tilePosition.x - referencePosition.x) + Mathf.Abs(tilePosition.y - referencePosition.y) >= 2) // Ensures at least 2 tiles away
                {
                    return tilePosition; // Return the first available tile found
                }
            }
        }

        return Vector3Int.zero; // Return zero if no available position is found
    }



    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy") || other.CompareTag("Barra")) // Check if the object that hit the hook is an Enemy or Barra
        {
            HandleEnemyHit(other.gameObject);
        }
    }

    public void HandleSwingOrPushIntoHook(GameObject enemy)
    {
        Debug.Log($"{enemy.name} was swung/pushed into the hook and died!");
        HandleEnemyHit(enemy);
    }

    public void HandleEnemyHit(GameObject enemy)
    {
        if (enemy == null)
        {
            Debug.LogError("HandleEnemyHit called with a null enemy.");
            return;
        }

        hookKillCount++;
        UpdateFishCountText();

        AIMove aiMove = enemy.GetComponent<AIMove>();
        BarraMove barraMove = enemy.GetComponent<BarraMove>();

        TempTurnBase tempTurnBase = FindObjectOfType<TempTurnBase>();
        if (tempTurnBase != null)
        {
            if (aiMove != null)
            {
                tempTurnBase.RemoveAIUnit(aiMove);
                OccupiedTilesManager.Instance.RemoveOccupiedPosition(aiMove.CurrentTilePosition);
            }
            else if (barraMove != null)
            {
                tempTurnBase.RemoveBarraUnit(barraMove);
                OccupiedTilesManager.Instance.RemoveOccupiedPosition(barraMove.CurrentTilePosition);
            }
        }

        EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
        if (enemyHealth != null)
        {
            enemyHealth.TakeDamage(enemyHealth.maxHealth);
        }

        All_SFX.PlayCaught();

        Destroy(enemy);

        RespawnHook();


        if (hookKillCount >= 6)
        {
            EndGame();
        }
    }

    private void EndGame()
    {
        Debug.Log("Game Over! You've caught 6 enemies.");
        SceneManager.LoadScene(0);
    }
}
