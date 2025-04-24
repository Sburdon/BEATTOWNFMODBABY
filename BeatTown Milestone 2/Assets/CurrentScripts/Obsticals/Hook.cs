using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Hook : MonoBehaviour
{
    public static Hook Instance { get; private set; }
    private Score scores;

    public delegate void HookSpawnedEvent(Hook hook);
    public static event HookSpawnedEvent OnHookSpawned;

    public delegate void HookKillCountChangedEvent(int killCount);
    public static event HookKillCountChangedEvent OnHookKillCountChanged;

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
            
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        scores = FindObjectOfType<Score>();
        hookPosition = tilemap.WorldToCell(transform.position);
        OccupiedTilesManager.Instance.AddOccupiedPosition(hookPosition);
        UpdateFishCountText();

        // Notify listeners that the Hook has spawned
        OnHookSpawned?.Invoke(this);
    }
    public void CatchAI(AIMove ai)
    {
        ai.isDead = true; // Flag for respawn
        RespawnManager.Instance.EnemyDied(ai.gameObject, false); // Trigger standard respawn
                                                                 // Optional: Disable renderer instead of destroying
        ai.gameObject.SetActive(false);
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
                    Mathf.Abs(tilePosition.x - playerPosition.x) + Mathf.Abs(tilePosition.y - playerPosition.y) >= 2)
                {
                    return tilePosition;
                }
            }
        }
        return Vector3Int.zero;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy") || other.CompareTag("Barra"))
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
        if(enemy.tag == "Enemy")
        {
            scores.score = scores.score + 10;
        }
        if(enemy.tag == "Barra")
        {
            scores.score = scores.score +20;
        }

        hookKillCount++;
        UpdateFishCountText();

        // Notify that the hook kill count has changed
        OnHookKillCountChanged?.Invoke(hookKillCount);

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
            enemyHealth.TakeDamage(enemyHealth.maxHealth, false);
        }

        All_SFX.PlayCaught();

        Destroy(enemy);
        RespawnHook();

        if (hookKillCount >= 6 && SceneManager.GetActiveScene().name == "Brady")
        {
            EndGame();
        }
    }

    private void EndGame()
    {
        Debug.Log("Game Over! You've caught 6 enemies.");
        SceneManager.LoadScene("WinScreen2");//DOES NOT LOAD BC IT HAS TISM
    }
}
