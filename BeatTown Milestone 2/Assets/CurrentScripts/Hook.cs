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
        OccupiedTilesManager.Instance.RemoveOccupiedPosition(oldHookPosition);

        Vector3Int newHookPosition = OccupiedTilesManager.Instance.GetRandomAvailablePosition(player.CurrentTilePosition);
        if (newHookPosition == Vector3Int.zero)
        {
            Debug.LogWarning("Hook: Unable to respawn due to no available positions.");
            return;
        }

        Vector3 worldPosition = tilemap.GetCellCenterWorld(newHookPosition);
        transform.position = worldPosition;
        OccupiedTilesManager.Instance.AddOccupiedPosition(newHookPosition);

        Debug.Log($"Hook respawned at {newHookPosition}");
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

        Destroy(enemy);

        RespawnHook();

        // Adjusted Barra spawning to occur at 2 kills and again at 4 kills
        if (hookKillCount == 2 || hookKillCount == 4)
        {
            RespawnManager.Instance.SpawnBarra();
            All_SFX.PlayCUANG();
        }

        if (hookKillCount >= 6)
        {
            EndGame();
        }
    }

    private void EndGame()
    {
        Debug.Log("Game Over! You've caught 6 enemies.");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
