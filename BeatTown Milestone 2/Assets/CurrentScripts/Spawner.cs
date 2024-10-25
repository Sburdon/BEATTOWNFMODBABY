using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Manages the spawning of enemies in the game.
/// Handles both respawnable and non-respawnable enemies.
/// </summary>
public class Spawner : MonoBehaviour
{
    [Header("Tilemap Settings")]
    [Tooltip("Reference to the Tilemap used for grid positioning.")]
    public Tilemap tilemap; // Reference to the Tilemap

    [Header("Enemy Spawn Settings")]
    [Tooltip("List of enemies to spawn.")]
    public List<EnemySpawnData> enemiesToSpawn = new List<EnemySpawnData>(); // List of enemies to spawn

    [Header("Spawn Configuration")]
    [Tooltip("Option to clear existing enemies before spawning.")]
    public bool clearExistingEnemies = true; // Option to clear existing enemies before spawning

    [Header("Player Reference")]
    [Tooltip("Reference to the PlayerMove script.")]
    public PlayerMove playerMove; // Reference to the PlayerMove script

    [Header("Turn-Based Manager")]
    [Tooltip("Reference to the TempTurnBase script.")]
    public TempTurnBase tempTurnBase; // Reference to the TempTurnBase script

    // Track enemies that have been killed and should not respawn
    private HashSet<GameObject> killedEnemies = new HashSet<GameObject>();

    void Awake()
    {
        // Ensure the Tilemap is assigned
        if (tilemap == null)
        {
            Debug.LogError("Spawner: Tilemap reference is missing. Please assign a Tilemap.");
            return;
        }

        // Ensure the PlayerMove is assigned
        if (playerMove == null)
        {
            playerMove = FindObjectOfType<PlayerMove>();
            if (playerMove == null)
            {
                Debug.LogError("Spawner: PlayerMove reference is missing and no PlayerMove found in the scene.");
                return;
            }
            else
            {
                Debug.Log("Spawner: PlayerMove auto-assigned.");
            }
        }

        // Ensure the TempTurnBase is assigned
        if (tempTurnBase == null)
        {
            tempTurnBase = FindObjectOfType<TempTurnBase>();
            if (tempTurnBase == null)
            {
                Debug.LogError("Spawner: TempTurnBase reference is missing and no TempTurnBase found in the scene.");
                return;
            }
            else
            {
                Debug.Log("Spawner: TempTurnBase auto-assigned.");
            }
        }

        // Optionally clear existing enemies
        if (clearExistingEnemies)
        {
            ClearExistingEnemies();
        }

        // Spawn all enemies
        SpawnEnemies();
    }

    /// <summary>
    /// Spawns all enemies as per the enemiesToSpawn list, respecting the shouldRespawn flag.
    /// </summary>
    void SpawnEnemies()
    {
        foreach (EnemySpawnData spawnData in enemiesToSpawn)
        {
            if (spawnData.enemyPrefab == null)
            {
                Debug.LogWarning("Spawner: Enemy prefab is missing in one of the spawn data entries.");
                continue;
            }

            // Check if the enemy should respawn and if it has been killed before
            if (!spawnData.shouldRespawn && killedEnemies.Contains(spawnData.enemyPrefab))
            {
                Debug.Log($"Spawner: Skipping spawn for '{spawnData.enemyPrefab.name}' as it should not respawn.");
                continue;
            }

            // Convert grid position to world position
            Vector3 worldPosition = tilemap.GetCellCenterWorld(spawnData.spawnGridPosition);

            // Instantiate the enemy at the world position with no rotation
            GameObject spawnedEnemy = Instantiate(spawnData.enemyPrefab, worldPosition, Quaternion.identity);

            // Optional: Assign parent for better hierarchy organization
            spawnedEnemy.transform.parent = this.transform;

            // Assign Tilemap and PlayerMove references to AIMove or BarraMove component
            AIMove aiMove = spawnedEnemy.GetComponent<AIMove>();
            BarraMove barraMove = spawnedEnemy.GetComponent<BarraMove>(); // Change to BarraMove

            if (aiMove != null)
            {
                aiMove.tilemap = tilemap; // Assign the Tilemap reference
                aiMove.playerMove = playerMove; // Assign the PlayerMove reference

                aiMove.CurrentTilePosition = spawnData.spawnGridPosition;
                OccupiedTilesManager.Instance.RegisterAI(aiMove);

                // Add to TempTurnBase's aiUnits list
                tempTurnBase.AddAIUnit(aiMove);

                // Subscribe to the enemy's death event to track killed enemies
                EnemyHealth enemyHealth = spawnedEnemy.GetComponent<EnemyHealth>();
                if (enemyHealth != null)
                {
                    enemyHealth.OnDeath += () => OnEnemyKilled(spawnedEnemy, spawnData);
                }

                Debug.Log($"Spawner: Spawned AI '{spawnedEnemy.name}' at grid position {spawnData.spawnGridPosition} with Tilemap and PlayerMove assigned.");
            }
            else if (barraMove != null) // Check for BarraMove instead of BarraAI
            {
                barraMove.tilemap = tilemap; // Assign the Tilemap reference
                barraMove.playerMove = playerMove; // Assign the PlayerMove reference

                barraMove.CurrentTilePosition = spawnData.spawnGridPosition;
                OccupiedTilesManager.Instance.RegisterBarraMove(barraMove);


                // Add to TempTurnBase's barraUnits list
                tempTurnBase.AddBarraUnit(barraMove);

                // Subscribe to the enemy's death event to track killed enemies
                EnemyHealth enemyHealth = spawnedEnemy.GetComponent<EnemyHealth>();
                if (enemyHealth != null)
                {
                    enemyHealth.OnDeath += () => OnEnemyKilled(spawnedEnemy, spawnData);
                }

                Debug.Log($"Spawner: Spawned BarraMove '{spawnedEnemy.name}' at grid position {spawnData.spawnGridPosition} with Tilemap and PlayerMove assigned.");
            }
            else
            {
                Debug.LogWarning($"Spawner: Spawned enemy '{spawnedEnemy.name}' does not have an AIMove or BarraMove component.");
            }
        }
    }

    /// <summary>
    /// Handles enemy death by tracking killed enemies and preventing their respawn.
    /// </summary>
    /// <param name="enemy">The enemy GameObject that was killed.</param>
    /// <param name="spawnData">The spawn data associated with the enemy.</param>
    void OnEnemyKilled(GameObject enemy, EnemySpawnData spawnData)
    {
        Debug.Log($"Spawner: Enemy '{enemy.name}' has been killed.");
        if (!spawnData.shouldRespawn)
        {
            killedEnemies.Add(spawnData.enemyPrefab);
            Debug.Log($"Spawner: '{spawnData.enemyPrefab.name}' marked to not respawn.");
        }
    }

    /// <summary>
    /// Clears existing enemies in the scene. Useful for resetting the game.
    /// </summary>
    void ClearExistingEnemies()
    {
        GameObject[] existingEnemies = GameObject.FindGameObjectsWithTag("Enemy");

        foreach (GameObject enemy in existingEnemies)
        {
            Destroy(enemy);
            Debug.Log($"Spawner: Destroyed existing enemy '{enemy.name}'.");
        }
    }
}
