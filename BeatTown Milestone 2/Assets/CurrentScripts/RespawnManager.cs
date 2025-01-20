using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class RespawnManager : MonoBehaviour
{
    public static RespawnManager Instance { get; private set; }

    [Header("Prefabs")]
    public GameObject enemyPrefab;
    public GameObject barraAIPrefab;
    public GameObject hookPrefab;

    [Header("Electrician Prefab & Spawn Settings")]
    [Tooltip("Prefab for your Electrician (with ElectricianMove script).")]
    public GameObject electricianPrefab;

    [Tooltip("Tile where the Electrician will spawn. E.g., (1, 3, 0).")]
    public Vector3Int electricianSpawnTile;

    [Header("Spawn Settings")]
    public int initialEnemiesToSpawn = 2;
    public int minEnemies = 2;
    public float respawnDelay = 5f;

    [Header("Hooks to Spawn")]
    public int hooksToSpawn = 1;

    private List<GameObject> enemies = new List<GameObject>();
    private TempTurnBase tempTurnBase;
    private Tilemap tilemap;
    private PlayerMove playerMove;
    private Text fishCountText;
    public All_SFX All_SFX;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        tempTurnBase = FindObjectOfType<TempTurnBase>();
        tilemap = FindObjectOfType<Tilemap>();
        playerMove = FindObjectOfType<PlayerMove>();
        fishCountText = GameObject.Find("FISH CAUGHT")?.GetComponent<Text>();

        // Check each reference individually to identify the missing component
        if (tempTurnBase == null) Debug.LogError("RespawnManager: TempTurnBase is missing.");
        if (tilemap == null) Debug.LogError("RespawnManager: Tilemap is missing.");
        if (playerMove == null) Debug.LogError("RespawnManager: PlayerMove is missing.");
        if (fishCountText == null) Debug.LogError("RespawnManager: FishCountText is missing.");

        if (tempTurnBase == null || tilemap == null || playerMove == null || fishCountText == null)
        {
            Debug.LogError("RespawnManager: One or more required references are missing.");
            return;
        }

        // 1) Spawn the Electrician immediately on scene start (if prefab is assigned)
        SpawnElectrician();

        // 2) Spawn the desired number of hooks (if any)
        SpawnHooks(hooksToSpawn);

        // 3) (Optional) Spawn initial enemies
        // for (int i = 0; i < initialEnemiesToSpawn; i++)
        // {
        //     SpawnEnemy();
        // }

        // Keep the enemy count as desired
        MaintainEnemyCount();
    }

    void Update()
    {
        // ...
    }

    /// <summary>
    /// Spawns a single Electrician at 'electricianSpawnTile' and adds to the turn order.
    /// </summary>
    private void SpawnElectrician()
    {
        // If you don't have an Electrician prefab assigned, just skip.
        if (electricianPrefab == null)
        {
            Debug.LogWarning("RespawnManager: No Electrician prefab assigned. Skipping spawn.");
            return;
        }

        // If the tile is occupied, skip spawning.
        if (OccupiedTilesManager.Instance.IsTileOccupied(electricianSpawnTile))
        {
            Debug.LogWarning($"RespawnManager: Electrician spawn tile {electricianSpawnTile} is occupied!");
            return;
        }

        // Convert tile to world position
        Vector3 spawnWorldPos = tilemap.GetCellCenterWorld(electricianSpawnTile);

        // Instantiate the Electrician
        GameObject electricianGO = Instantiate(electricianPrefab, spawnWorldPos, Quaternion.identity);

        // Get the ElectricianMove script
        ElectricianMove electricianMove = electricianGO.GetComponent<ElectricianMove>();
        if (electricianMove != null)
        {
            // Assign references
            electricianMove.tilemap = tilemap;
            electricianMove.playerMove = playerMove;
            electricianMove.CurrentTilePosition = electricianSpawnTile;

            // Mark the tile as occupied
            OccupiedTilesManager.Instance.AddOccupiedPosition(electricianSpawnTile);

            // Add to the turn order (method from your updated TempTurnBase)
            tempTurnBase.AddElectricianUnit(electricianMove);

            Debug.Log($"RespawnManager: Spawned Electrician at {electricianSpawnTile}");
        }
        else
        {
            Debug.LogError("RespawnManager: Electrician prefab has no ElectricianMove component!");
        }
    }

    /// <summary>
    /// Spawns the specified number of hooks. Assigns the first hook
    /// to Swing/Push if needed.
    /// </summary>
    private void SpawnHooks(int count)
    {
        if (hookPrefab == null)
        {
            Debug.LogError("RespawnManager: hookPrefab is not assigned.");
            return;
        }

        for (int i = 0; i < count; i++)
        {
            Vector3Int hookSpawnTile = OccupiedTilesManager.Instance.GetRandomAvailablePosition(Vector3Int.zero);
            Vector3 hookWorldPosition = OccupiedTilesManager.Instance.tilemap.GetCellCenterWorld(hookSpawnTile);

            GameObject hookInstance = Instantiate(hookPrefab, hookWorldPosition, Quaternion.identity);
            OccupiedTilesManager.Instance.AddOccupiedPosition(hookSpawnTile);

            Hook hookScript = hookInstance.GetComponent<Hook>();
            if (hookScript != null)
            {
                hookScript.tilemap = tilemap;
                hookScript.player = playerMove;
                hookScript.fishCountText = fishCountText;
            }
            else
            {
                Debug.LogError("RespawnManager: Hook prefab missing Hook component!");
            }

            // Assign the first spawned Hook to Swing/Push
            if (i == 0)
            {
                Swing swingScript = FindObjectOfType<Swing>();
                Push pushScript = FindObjectOfType<Push>();

                if (swingScript != null) swingScript.hook = hookScript;
                if (pushScript != null) pushScript.hook = hookScript;
            }
        }
    }

    public void MaintainEnemyCount()
    {
        int currentEnemyCount = enemies.FindAll(enemy => enemy != null && !enemy.CompareTag("Barra")).Count;

        while (currentEnemyCount < minEnemies)
        {
            SpawnEnemy();
            currentEnemyCount++;
        }
    }

    /// <summary>
    /// Called when an enemy dies. Determines if the enemy should respawn.
    /// </summary>
    public void EnemyDied(GameObject enemy)
    {
        if (enemy == null)
        {
            Debug.LogWarning("RespawnManager: EnemyDied called with a null enemy.");
            return;
        }

        enemies.Remove(enemy);

        Vector3Int enemyTilePosition = OccupiedTilesManager.Instance.tilemap.WorldToCell(enemy.transform.position);
        OccupiedTilesManager.Instance.RemoveOccupiedPosition(enemyTilePosition);

        Destroy(enemy);

        // If you want them to respawn after a delay, uncomment:
        // if (!enemy.CompareTag("Barra"))
        // {
        //     StartCoroutine(RespawnCoroutine());
        // }
    }

    /// <summary>
    /// Coroutine to handle respawning of regular enemies after a delay.
    /// </summary>
    public IEnumerator RespawnCoroutine()
    {
        yield return new WaitForSeconds(respawnDelay);
        SpawnEnemy();
    }

    /// <summary>
    /// Spawns a regular enemy at a random unoccupied tile position and adds them to the turn system.
    /// </summary>
    public void SpawnEnemy()
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("RespawnManager: enemyPrefab is not assigned.");
            return;
        }

        PlayerMove playerMove = FindObjectOfType<PlayerMove>();
        if (playerMove == null)
        {
            Debug.LogError("RespawnManager: PlayerMove instance not found in the scene.");
            return;
        }

        Vector3Int playerTile = playerMove.CurrentTilePosition;
        Vector3Int spawnTile = OccupiedTilesManager.Instance.GetRandomAvailablePosition(playerTile);

        if (OccupiedTilesManager.Instance.IsTileOccupied(spawnTile))
        {
            Debug.LogWarning("RespawnManager: Spawn tile is occupied. Trying another position.");
            return;
        }

        Vector3 worldPosition = OccupiedTilesManager.Instance.tilemap.GetCellCenterWorld(spawnTile);
        GameObject newEnemy = Instantiate(enemyPrefab, worldPosition, Quaternion.identity);

        enemies.Add(newEnemy);

        AIMove aiMove = newEnemy.GetComponent<AIMove>();
        if (aiMove != null)
        {
            aiMove.CurrentTilePosition = spawnTile;
            OccupiedTilesManager.Instance.RegisterAI(aiMove);
            tempTurnBase.AddAIUnit(aiMove);
        }
        else
        {
            Debug.LogError("RespawnManager: Spawned enemy missing AIMove component.");
        }
    }

    /// <summary>
    /// Spawns a Barra enemy at a random unoccupied tile position and adds them to the turn system.
    /// </summary>
    public void SpawnBarra()
    {
        if (barraAIPrefab == null) return;

        PlayerMove playerMove = FindObjectOfType<PlayerMove>();
        if (playerMove == null)
        {
            Debug.LogError("RespawnManager: PlayerMove instance not found in the scene.");
            return;
        }

        Vector3Int playerTile = playerMove.CurrentTilePosition;
        Vector3Int spawnTile = OccupiedTilesManager.Instance.GetRandomAvailablePosition(playerTile);

        if (OccupiedTilesManager.Instance.IsTileOccupied(spawnTile))
        {
            Debug.LogWarning("RespawnManager: No available spawn for Barra.");
            return;
        }

        Vector3 worldPosition = OccupiedTilesManager.Instance.tilemap.GetCellCenterWorld(spawnTile);
        GameObject newBarra = Instantiate(barraAIPrefab, worldPosition, Quaternion.identity);

        BarraMove barraMove = newBarra.GetComponent<BarraMove>();
        if (barraMove != null)
        {
            barraMove.CurrentTilePosition = spawnTile;
            OccupiedTilesManager.Instance.RegisterBarraMove(barraMove);
            tempTurnBase.AddBarraUnit(barraMove);
        }

        Debug.Log("RespawnManager: Spawned a new Barra at " + spawnTile);
    }
}
