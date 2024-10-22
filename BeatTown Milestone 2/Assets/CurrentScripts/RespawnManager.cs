using UnityEngine;
using UnityEngine.Tilemaps;

public class RespawnManager : MonoBehaviour
{
    public static RespawnManager Instance;

    public Tilemap tilemap;
    public PlayerMove player;

    void Awake()
    {
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
        Vector3Int oldPosition = tilemap.WorldToCell(enemy.transform.position);
        OccupiedTilesManager.Instance.RemoveOccupiedPosition(oldPosition);

        EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
        if (enemyHealth != null)
        {
            enemyHealth.ResetHealth();
        }

        AIMove enemyMove = enemy.GetComponent<AIMove>();
        if (enemyMove != null)
        {
            enemyMove.ResetAI();
            enemyMove.enabled = true;
        }

        Vector3Int newEnemyPosition = GetRandomAvailablePosition();
        Vector3 worldPosition = tilemap.GetCellCenterWorld(newEnemyPosition);
        enemy.transform.position = worldPosition;

        if (enemyMove != null)
        {
            enemyMove.CurrentTilePosition = newEnemyPosition;
        }

        OccupiedTilesManager.Instance.AddOccupiedPosition(newEnemyPosition);
        enemy.SetActive(true);
    }

    private Vector3Int GetRandomAvailablePosition()
    {
        Vector3Int randomPosition;
        do
        {
            randomPosition = new Vector3Int(Random.Range(-10, 10), Random.Range(-10, 10), 0);
        } while (OccupiedTilesManager.Instance.IsTileOccupied(randomPosition) || !tilemap.HasTile(randomPosition)
                 || randomPosition == player.CurrentTilePosition);

        return randomPosition;
    }
}