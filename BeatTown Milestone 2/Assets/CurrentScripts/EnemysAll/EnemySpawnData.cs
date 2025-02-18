using UnityEngine;

[System.Serializable]
public class EnemySpawnData
{
    public GameObject enemyPrefab;
    public Vector3Int spawnGridPosition;
    public bool shouldRespawn = true;
}
