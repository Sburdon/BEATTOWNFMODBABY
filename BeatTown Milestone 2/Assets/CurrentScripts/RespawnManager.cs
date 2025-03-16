using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class RespawnManager : MonoBehaviour
{
    public static RespawnManager Instance { get; private set; }


    [Header("Prefabs")]
    public GameObject enemyPrefab;
    public GameObject barraAIPrefab;
    public GameObject hookPrefab;

    [Header("Goon Prefab & Spawn Settings")]
    [Tooltip("Prefab for your Goon (with GoonMove & GoonFatigue scripts).")]
    public GameObject goonPrefab;

    [Tooltip("Tile where the Goon will spawn. E.g., (5, 5, 0).")]
    public Vector3Int goonSpawnTile;

    [Header("Electrician Prefab & Spawn Settings")]
    public GameObject electricianPrefab;
    public Vector3Int electricianSpawnTile;

    [Header("Spawn Settings")]
    public int initialEnemiesToSpawn = 2;
    public int minEnemies = 2;
    public float respawnDelay = 5f;

    [Header("Hooks to Spawn")]
    public int hooksToSpawn = 1;

    private List<GameObject> enemies = new List<GameObject>();
    private TempTurnBase tempTurnBase;
    private EnemyHealth enemyHealth;
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
        enemyHealth = FindAnyObjectByType<EnemyHealth>();
        tempTurnBase = FindObjectOfType<TempTurnBase>();
        tilemap = FindObjectOfType<Tilemap>();
        playerMove = FindObjectOfType<PlayerMove>();
        fishCountText = GameObject.Find("FISH CAUGHT")?.GetComponent<Text>();

        if (tempTurnBase == null) Debug.LogError("RespawnManager: TempTurnBase is missing.");
        if (tilemap == null) Debug.LogError("RespawnManager: Tilemap is missing.");
        if (playerMove == null) Debug.LogError("RespawnManager: PlayerMove is missing.");
        if (fishCountText == null) Debug.LogError("RespawnManager: FishCountText is missing.");

        if (tempTurnBase == null || tilemap == null || playerMove == null || fishCountText == null)
        {
            Debug.LogError("RespawnManager: One or more required references are missing.");
            return;
        }
        if (SceneManager.GetActiveScene().buildIndex == 1)
        {
            MaintainEnemyCount();
        }

        // Example: spawn the Goon on scene start
        SpawnGoon();

        // Example: spawn the Electrician on scene start
        SpawnElectrician();

        // Spawn the desired number of hooks
        SpawnHooks(hooksToSpawn);

        // (Optional) Spawn some initial enemies
        // for (int i = 0; i < initialEnemiesToSpawn; i++)
        // {
        //    SpawnEnemy();
        // }

        MaintainEnemyCount();
    }

    void Update()
    {
        // ...
    }

    // ------------------------------------------------------------------
    // Goon Spawning
    // ------------------------------------------------------------------
    public void SpawnGoon()
    {
        if (goonPrefab == null)
        {
            Debug.LogWarning("RespawnManager: No Goon prefab assigned.");
            return;
        }
        if (OccupiedTilesManager.Instance.IsTileOccupied(goonSpawnTile))
        {
            Debug.LogWarning($"RespawnManager: Goon spawn tile {goonSpawnTile} is occupied!");
            return;
        }

        Vector3 spawnWorldPos = tilemap.GetCellCenterWorld(goonSpawnTile);
        GameObject goonGO = Instantiate(goonPrefab, spawnWorldPos, Quaternion.identity);

        GoonMove goonMove = goonGO.GetComponent<GoonMove>();
        if (goonMove != null)
        {
            // Assign references the Goon might need:
            // goonMove.playerMove = playerMove; // REMOVED since new GoonMove doesn't need it
            goonMove.tilemap = tilemap;
            goonMove.CurrentTilePosition = goonSpawnTile;

            // Occupy the tile
            OccupiedTilesManager.Instance.AddOccupiedPosition(goonSpawnTile);

            // Add to turn system
            tempTurnBase.AddGoonUnit(goonMove);
        }
        else
        {
            Debug.LogError("RespawnManager: Goon prefab is missing GoonMove component.");
        }
    }

    // ------------------------------------------------------------------
    // Electrician Spawning
    // ------------------------------------------------------------------
    public void SpawnElectrician()
    {
        if (electricianPrefab == null)
        {
            Debug.LogWarning("RespawnManager: No Electrician prefab assigned.");
            return;
        }
        if (OccupiedTilesManager.Instance.IsTileOccupied(electricianSpawnTile))
        {
            Debug.LogWarning($"RespawnManager: Electrician spawn tile {electricianSpawnTile} is occupied!");
            return;
        }

        Vector3 spawnWorldPos = tilemap.GetCellCenterWorld(electricianSpawnTile);
        GameObject electricianGO = Instantiate(electricianPrefab, spawnWorldPos, Quaternion.identity);

        ElectricianMove electricianMove = electricianGO.GetComponent<ElectricianMove>();
        if (electricianMove != null)
        {
            electricianMove.tilemap = tilemap;
            electricianMove.CurrentTilePosition = electricianSpawnTile;
            // electricianMove.playerMove = playerMove; // REMOVED if new ElectricianMove doesn't need it

            OccupiedTilesManager.Instance.AddOccupiedPosition(electricianSpawnTile);
            tempTurnBase.AddElectricianUnit(electricianMove);
        }
        else
        {
            Debug.LogError("RespawnManager: Electrician prefab has no ElectricianMove component!");
        }
    }

    // ------------------------------------------------------------------
    // Hook Spawning
    // ------------------------------------------------------------------
    private void SpawnHooks(int count)
    { // If there’s no hook prefab, we still stop here if (hookPrefab == null) { Debug.LogError("RespawnManager: hookPrefab is not assigned."); return; }


    // If count is zero, skip everything else
        if (count <= 0)
        {
            Debug.Log("RespawnManager: No hooks to spawn. Skipping hook spawning.");
            return;
        }

        // If count > 0, run the original loop
        for (int i = 0; i < count; i++)
        {
            Vector3Int hookSpawnTile = OccupiedTilesManager.Instance.GetRandomAvailablePosition(Vector3Int.zero);
            Vector3 hookWorldPosition = OccupiedTilesManager.Instance.tilemap.GetCellCenterWorld(hookSpawnTile);

            GameObject hookInstance = Instantiate(hookPrefab, hookWorldPosition, Quaternion.identity);
            OccupiedTilesManager.Instance.AddOccupiedPosition(hookSpawnTile);

            // Set references
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

            // Only set Swing/Push references on the very first hook
            if (i == 0)
            {
                Swing swingScript = FindObjectOfType<Swing>();
                Push pushScript = FindObjectOfType<Push>();

                if (swingScript != null) swingScript.hook = hookScript;
                if (pushScript != null) pushScript.hook = hookScript;
            }
        }
    }


// ------------------------------------------------------------------
// Regular Enemy Logic
// ------------------------------------------------------------------
    public void MaintainEnemyCount()
    {
        List<AIMove> enemiesToRespawn = new List<AIMove>();

        foreach (var enemy in enemies)
        {
            if (enemy == null) continue;

            AIMove aiMove = enemy.GetComponent<AIMove>();
            if (aiMove != null && aiMove.isDead)
            {
                if (aiMove.turnsUntilRespawn > 0)
                {
                    aiMove.turnsUntilRespawn--; // Decrement timer
                }
                else
                {
                    enemiesToRespawn.Add(aiMove); // Queue for respawn
                }
            }
        }

        // Respawn queued units
        foreach (var aiMove in enemiesToRespawn)
        {
            RespawnAIUnit(aiMove);
            aiMove.isDead = false;
        }

        // Maintain the minimum enemy count
        int currentEnemyCount = enemies.FindAll(enemy => enemy != null && !enemy.CompareTag("Barra")).Count;

        while (currentEnemyCount < minEnemies)
        {
            SpawnEnemy();
            currentEnemyCount++;
        }
    }
    private void RespawnAIUnit(AIMove aiMove)
{
    // Pick your spawnTile as you do now
    Vector3Int spawnTile = OccupiedTilesManager.Instance.GetRandomAvailablePosition(Vector3Int.zero);

    // Simply tell the AI to perform its own “Respawn” steps
    aiMove.RespawnAI(spawnTile);
}

    public void EnemyDied(GameObject enemy, bool fromPlayerOrBarra)
    {
        if (enemy == null)
        {
            Debug.LogWarning("RespawnManager: EnemyDied called with a null enemy.");
            return;
        }

        AIMove aiMove = enemy.GetComponent<AIMove>();
        if (aiMove != null)
        {
            // Free up the tile the AI was occupying
            Vector3Int enemyTile = tilemap.WorldToCell(enemy.transform.position);
            OccupiedTilesManager.Instance.RemoveOccupiedPosition(enemyTile);

            // Mark for respawn with 1-turn delay
            aiMove.isDead = true;
            aiMove.turnsUntilRespawn = 1;
            Debug.Log($"{enemy.name} removed from tile {enemyTile}.");
        }
        else
        {
            Debug.LogWarning("RespawnManager: Enemy missing AIMove component.");
        }
    }

    public IEnumerator RespawnCoroutine()
    {
        yield return new WaitForSeconds(respawnDelay);
        SpawnEnemy();
    }

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

    // ------------------------------------------------------------------
    // Barra Spawning
    // ------------------------------------------------------------------
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