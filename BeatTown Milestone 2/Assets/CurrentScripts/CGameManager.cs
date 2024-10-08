using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps; // Added Tilemap namespace

public class CGameManager : MonoBehaviour
{
    public GameObject playerPrefab;
    public GameObject aiPrefab;
    public Tilemap gridTilemap; // Reference to existing grid Tilemap
    public Vector2Int playerSpawnPoint; // Player spawn point settable in inspector
    public List<Vector2Int> aiSpawnPoints; // AI spawn points settable in inspector

    void Start()
    {
        SpawnPlayer(playerSpawnPoint);
        SpawnAI(aiSpawnPoints);
    }

    public void SpawnPlayer(Vector2Int gridPosition)
    {
        if (IsValidGridPosition(gridPosition))
        {
            Vector3 spawnPosition = gridTilemap.GetCellCenterWorld((Vector3Int)gridPosition);
            Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
        }
        else
        {
            Debug.LogError("Invalid player spawn position");
        }
    }

    public void SpawnAI(List<Vector2Int> aiPositions)
    {
        foreach (Vector2Int gridPosition in aiPositions)
        {
            if (IsValidGridPosition(gridPosition))
            {
                Vector3 spawnPosition = gridTilemap.GetCellCenterWorld((Vector3Int)gridPosition);
                Instantiate(aiPrefab, spawnPosition, Quaternion.identity);
            }
            else
            {
                Debug.LogError("Invalid AI spawn position at " + gridPosition);
            }
        }
    }

    private bool IsValidGridPosition(Vector2Int gridPosition)
    {
        Vector3Int tilePosition = (Vector3Int)gridPosition;
        return gridTilemap.HasTile(tilePosition);
    }

    public bool IsWithinBounds(Vector3 position)
    {
        Vector3Int gridPosition = gridTilemap.WorldToCell(position);
        return gridTilemap.HasTile(gridPosition);
    }
}