using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawnManager : MonoBehaviour
{
    public static RespawnManager Instance { get; private set; }

    [Header("Prefabs")]
    public GameObject enemyPrefab;       // Prefab for regular enemies
    public GameObject barraAIPrefab;     // Prefab for Barra enemies

    [Header("Spawn Settings")]
    public int initialEnemiesToSpawn = 2; // Number of regular enemies to spawn at game start
    public float respawnDelay = 5f;       // Delay before respawning regular enemies

    private List<GameObject> enemies = new List<GameObject>(); // List to track active regular enemies
    private bool barraSpawned = false;    // Track if the Barra has been spawned

    private TempTurnBase tempTurnBase; // Reference to TempTurnBase for adding units to turn system

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
        // Assign TempTurnBase
        tempTurnBase = FindObjectOfType<TempTurnBase>();

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
        Destroy(enemy);

        // Get the tile position of the dead enemy
        Vector3Int enemyTilePosition = OccupiedTilesManager.Instance.tilemap.WorldToCell(enemy.transform.position);

        // Remove the occupied position
        OccupiedTilesManager.Instance.RemoveOccupiedPosition(enemyTilePosition);

        // Check if the dead enemy is NOT a Barra, only regular enemies respawn
        if (!enemy.CompareTag("Barra"))
        {
            StartCoroutine(RespawnCoroutine());
        }
        // If it's a Barra, do not respawn
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

        // Find the PlayerMove instance to get the player's tile position
        PlayerMove playerMove = FindObjectOfType<PlayerMove>();
        if (playerMove == null)
        {
            Debug.LogError("RespawnManager: PlayerMove instance not found in the scene.");
            return;
        }

        Vector3Int playerTile = playerMove.CurrentTilePosition;

        // Get a random available spawn position
        Vector3Int spawnTile = OccupiedTilesManager.Instance.GetRandomAvailablePosition(playerTile);
        if (spawnTile == Vector3Int.zero)
        {
            Debug.LogWarning("RespawnManager: Unable to spawn enemy due to no available positions.");
            return;
        }

        Vector3 worldPosition = OccupiedTilesManager.Instance.tilemap.GetCellCenterWorld(spawnTile);
        GameObject newEnemy = Instantiate(enemyPrefab, worldPosition, Quaternion.identity);
        enemies.Add(newEnemy);

        // Register the new enemy's tile as occupied
        AIMove aiMove = newEnemy.GetComponent<AIMove>();
        if (aiMove != null)
        {
            OccupiedTilesManager.Instance.RegisterAI(aiMove);
            tempTurnBase.AddAIUnit(aiMove); // Add to TempTurnBase for turn management
        }
    }

    /// <summary>
    /// Spawns a Barra enemy at a random unoccupied tile position and adds them to the turn system.
    /// </summary>
    public void SpawnBarra()
    {
        if (barraAIPrefab == null || barraSpawned) return;

        PlayerMove playerMove = FindObjectOfType<PlayerMove>();
        if (playerMove == null)
        {
            Debug.LogError("RespawnManager: PlayerMove instance not found in the scene.");
            return;
        }

        Vector3Int playerTile = playerMove.CurrentTilePosition;

        // Get a random available spawn position
        Vector3Int spawnTile = OccupiedTilesManager.Instance.GetRandomAvailablePosition(playerTile);
        if (spawnTile == Vector3Int.zero)
        {
            Debug.LogWarning("RespawnManager: Unable to spawn Barra due to no available positions.");
            return;
        }

        Vector3 worldPosition = OccupiedTilesManager.Instance.tilemap.GetCellCenterWorld(spawnTile);
        GameObject newBarra = Instantiate(barraAIPrefab, worldPosition, Quaternion.identity);

        BarraMove barraMove = newBarra.GetComponent<BarraMove>();
        if (barraMove != null)
        {
            OccupiedTilesManager.Instance.RegisterBarraMove(barraMove);
            tempTurnBase.AddBarraUnit(barraMove); // Add to TempTurnBase for turn management
        }

        barraSpawned = true; // Ensure Barra only spawns once
    }
}
