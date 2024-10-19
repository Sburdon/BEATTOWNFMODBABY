using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement; // For ending the game

public class Hook : MonoBehaviour
{
    public static Hook Instance { get; private set; } // Singleton instance

    public Tilemap tilemap;          // Reference to the tilemap for grid-based positioning
    public GameObject hookPrefab;    // Hook prefab for respawning hook
    public GameObject barraPrefab;   // BarraAI prefab
    public PlayerMove player;        // Reference to the player for position checks
    public int hookKillCount = 0;    // Counter for how many enemies have died from the hook

    private Vector3Int hookPosition; // Current position of the hook
    private bool barraSpawned = false; // To ensure BarraAI spawns only once

    void Awake()
    {
        // Implement singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Initialize hook position on the grid
        hookPosition = tilemap.WorldToCell(transform.position);
        OccupiedTilesManager.Instance.AddOccupiedPosition(hookPosition); // Register hook's position
    }

    public Vector3Int GetHookPosition()
    {
        return tilemap.WorldToCell(transform.position);
    }

    private Vector3Int GetRandomAvailablePosition()
    {
        Vector3Int randomPosition;
        do
        {
            randomPosition = new Vector3Int(Random.Range(-10, 10), Random.Range(-10, 10), 0); // Adjust range as needed
        }
        while (OccupiedTilesManager.Instance.IsTileOccupied(randomPosition)
               || !tilemap.HasTile(randomPosition)
               || randomPosition == player.CurrentTilePosition); // Ensure the position is available and not occupied by the player or other units

        return randomPosition;
    }

    private void RespawnHook()
    {
        // Remove old hook position from occupied positions
        Vector3Int oldHookPosition = tilemap.WorldToCell(transform.position);
        OccupiedTilesManager.Instance.RemoveOccupiedPosition(oldHookPosition);

        // Set a new random position for the hook
        hookPosition = GetRandomAvailablePosition();

        // Move the hook to the new position
        Vector3 worldPosition = tilemap.GetCellCenterWorld(hookPosition);
        transform.position = worldPosition;

        // Add new hook position to occupied positions
        OccupiedTilesManager.Instance.AddOccupiedPosition(hookPosition);

        Debug.Log($"Hook respawned at {hookPosition}");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy")) // Check if the object that hit the hook is an Enemy
        {
            HandleEnemyHit(other.gameObject);
        }
    }

    public void HandleSwingOrPushIntoHook(GameObject enemy)
    {
        // Check if the enemy is immune to hooks (BarraAI)
        if (IsEnemyImmuneToHook(enemy))
        {
            Debug.Log($"{enemy.name} is immune to hooks!");
            return;
        }

        Debug.Log($"{enemy.name} was swung/pushed into the hook and died!");
        HandleEnemyHit(enemy);
    }

    public void HandleEnemyHit(GameObject enemy)
    {
        // Check if the enemy is immune to hooks (BarraAI)
        if (IsEnemyImmuneToHook(enemy))
        {
            Debug.Log($"{enemy.name} is immune to hooks!");
            return;
        }

        // Enemy hits the hook - it dies
        Debug.Log($"{enemy.name} hit the hook and died!");

        // Increment the kill count
        hookKillCount++;
        Debug.Log($"Enemies killed by hook: {hookKillCount}");

        // Remove enemy's old position from occupied positions
        AIMove enemyMove = enemy.GetComponent<AIMove>();
        if (enemyMove != null)
        {
            OccupiedTilesManager.Instance.RemoveOccupiedPosition(enemyMove.CurrentTilePosition);
        }

        // Get EnemyHealth component and set health to zero
        EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
        if (enemyHealth != null)
        {
            enemyHealth.TakeDamage(enemyHealth.health); // Reduce health to zero
        }
        else
        {
            Debug.LogError("EnemyHealth component not found on enemy.");
        }

        // Respawn the hook at a new random location
        RespawnHook();

        // Spawn BarraAI after two kills
        if (!barraSpawned && hookKillCount >= 2)
        {
            SpawnBarra();
            barraSpawned = true;
        }

        // End game after six kills
        if (hookKillCount >= 6)
        {
            EndGame();
        }
    }

    private bool IsEnemyImmuneToHook(GameObject enemy)
    {
        // Check if the enemy has the BarraAI component
        return enemy.GetComponent<BarraAI>() != null;
    }

    private void SpawnBarra()
    {
        Vector3Int spawnPosition = GetRandomAvailablePosition();
        Vector3 worldPosition = tilemap.GetCellCenterWorld(spawnPosition);

        GameObject barra = Instantiate(barraPrefab, worldPosition, Quaternion.identity);
        BarraAI barraAI = barra.GetComponent<BarraAI>();

        if (barraAI != null)
        {
            barraAI.CurrentTilePosition = spawnPosition;
            barraAI.playerMove = player; // Assign the player reference
            barraAI.tilemap = tilemap;
            barraAI.followPlayer = false; // Set as needed
        }

        // Register BarraAI's position
        OccupiedTilesManager.Instance.AddOccupiedPosition(spawnPosition);

        // Add BarraAI to the turn system
        TempTurnBase turnBase = FindObjectOfType<TempTurnBase>();
        if (turnBase != null)
        {
            turnBase.AddBarraUnit(barraAI);
        }

        Debug.Log("BarraAI has spawned!");
    }

    private void EndGame()
    {
        Debug.Log("Game Over! You've caught 6 enemies.");
        // Implement your game over logic here
        // For example, load a game over scene or display a message

        // Example: Reload the current scene (replace with your game over logic)
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
