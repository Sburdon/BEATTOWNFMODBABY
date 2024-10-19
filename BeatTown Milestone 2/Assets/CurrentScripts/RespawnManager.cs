using UnityEngine;
using UnityEngine.Tilemaps;

public class RespawnManager : MonoBehaviour
{
    public static RespawnManager Instance;

    public Tilemap tilemap;          // Reference to the tilemap for grid-based positioning
    public PlayerMove player;        // Reference to the player for position checks

    void Awake()
    {
        // Implement singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    public void RespawnEnemy(GameObject enemy)
    {
        // Remove enemy's old position from occupied positions
        Vector3Int oldPosition = tilemap.WorldToCell(enemy.transform.position);
        OccupiedTilesManager.Instance.RemoveOccupiedPosition(oldPosition);

        EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
        if (enemyHealth != null)
        {
            enemyHealth.ResetHealth(); // Reset health and IsDead flag
        }

        // Re-enable enemy's behavior scripts
        AIMove enemyMove = enemy.GetComponent<AIMove>();
        if (enemyMove != null)
        {
            enemyMove.ResetAI();         // Reset AI state
            enemyMove.enabled = true;    // Ensure the script is enabled
        }

        // Move the enemy to a new position
        Vector3Int newEnemyPosition = GetRandomAvailablePosition();
        Vector3 worldPosition = tilemap.GetCellCenterWorld(newEnemyPosition);
        enemy.transform.position = worldPosition;

        // Update the enemy's current tile position
        if (enemyMove != null)
        {
            enemyMove.CurrentTilePosition = newEnemyPosition;
        }

        // Register enemy's new position
        OccupiedTilesManager.Instance.AddOccupiedPosition(newEnemyPosition);

        // Ensure enemy is active
        enemy.SetActive(true);

        Debug.Log($"{enemy.name} respawned at {newEnemyPosition}");
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
}
