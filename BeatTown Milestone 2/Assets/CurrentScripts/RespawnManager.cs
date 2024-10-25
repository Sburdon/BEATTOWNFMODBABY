using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class RespawnManager : MonoBehaviour
{
    public static RespawnManager Instance { get; private set; }

    [Header("Prefabs")]
    public GameObject enemyPrefab;       // Prefab for regular enemies
    public GameObject barraAIPrefab;     // Prefab for Barra enemies
    public GameObject hookPrefab;        // Prefab for Hook

    [Header("Spawn Settings")]
    public int initialEnemiesToSpawn = 2; // Number of regular enemies to spawn at game start
    public float respawnDelay = 5f;       // Delay before respawning regular enemies

    private List<GameObject> enemies = new List<GameObject>(); // List to track active regular enemies
    private bool barraSpawned = false;    // Track if the Barra has been spawned

    private TempTurnBase tempTurnBase;    // Reference to TempTurnBase for adding units to turn system
    private Tilemap tilemap;
    private PlayerMove playerMove;
    private Text fishCountText;

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
        // Assign TempTurnBase and other references
        tempTurnBase = FindObjectOfType<TempTurnBase>();
        tilemap = FindObjectOfType<Tilemap>();
        playerMove = FindObjectOfType<PlayerMove>();
        fishCountText = GameObject.Find("FISH CAUGHT")?.GetComponent<Text>();

        // Spawn initial hook if hookPrefab is assigned
        if (hookPrefab != null)
        {
            Vector3Int hookSpawnTile = OccupiedTilesManager.Instance.GetRandomAvailablePosition(Vector3Int.zero);
            Vector3 hookWorldPosition = OccupiedTilesManager.Instance.tilemap.GetCellCenterWorld(hookSpawnTile);
            GameObject hookInstance = Instantiate(hookPrefab, hookWorldPosition, Quaternion.identity);

            // Assign references to the Hook instance
            Hook hookScript = hookInstance.GetComponent<Hook>();
            if (hookScript != null)
            {
                hookScript.tilemap = tilemap;
                hookScript.player = playerMove;
                hookScript.fishCountText = fishCountText;
            }

            OccupiedTilesManager.Instance.AddOccupiedPosition(hookSpawnTile);
        }

        // Spawn initial regular enemies
        for (int i = 0; i < initialEnemiesToSpawn; i++)
        {
            SpawnEnemy();
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

        if (!enemy.CompareTag("Barra"))
        {
            StartCoroutine(RespawnCoroutine());
        }
    }

    /// <summary>
    /// Coroutine to handle respawning of regular enemies after a delay.
    /// </summary>
    private IEnumerator RespawnCoroutine()
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

        // Ensure that a valid spawn tile is found and is unoccupied
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
            Debug.LogError("RespawnManager: Spawned enemy does not have an AIMove component.");
        }
    }

    /// <summary>
    /// Spawns a Barra enemy at a random unoccupied tile position and adds them to the turn system.
    /// </summary>
    public void SpawnBarra()
    {
        if (barraAIPrefab == null) return; // Ensure prefab exists

        PlayerMove playerMove = FindObjectOfType<PlayerMove>();
        if (playerMove == null)
        {
            Debug.LogError("RespawnManager: PlayerMove instance not found in the scene.");
            return;
        }

        Vector3Int playerTile = playerMove.CurrentTilePosition;
        Vector3Int spawnTile = OccupiedTilesManager.Instance.GetRandomAvailablePosition(playerTile);

        // Ensure that a valid spawn tile is found and is unoccupied
        if (OccupiedTilesManager.Instance.IsTileOccupied(spawnTile))
        {
            Debug.LogWarning("RespawnManager: Unable to spawn Barra due to no available positions.");
            return;
        }

        Vector3 worldPosition = OccupiedTilesManager.Instance.tilemap.GetCellCenterWorld(spawnTile);
        GameObject newBarra = Instantiate(barraAIPrefab, worldPosition, Quaternion.identity);

        BarraMove barraMove = newBarra.GetComponent<BarraMove>();
        if (barraMove != null)
        {
            barraMove.CurrentTilePosition = spawnTile;
            OccupiedTilesManager.Instance.RegisterBarraMove(barraMove);
            tempTurnBase.AddBarraUnit(barraMove); // Add to TempTurnBase for turn management
        }

        Debug.Log("RespawnManager: Spawned a new Barra at " + spawnTile);
    }
}
