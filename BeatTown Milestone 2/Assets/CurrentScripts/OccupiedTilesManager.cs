using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class OccupiedTilesManager : MonoBehaviour
{
    private static OccupiedTilesManager _instance;
    public static OccupiedTilesManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<OccupiedTilesManager>();
            }
            return _instance;
        }
    }

    private HashSet<Vector3Int> occupiedPositions = new HashSet<Vector3Int>();

    public void AddOccupiedPosition(Vector3Int position)
    {
        occupiedPositions.Add(position);
    }

    public void RemoveOccupiedPosition(Vector3Int position)
    {
        occupiedPositions.Remove(position);
    }

    public bool IsTileOccupied(Vector3Int position)
    {
        return occupiedPositions.Contains(position);
    }

    // Ensure no AI or player spawns on an occupied tile
    public Vector3Int GetRandomAvailablePosition(Tilemap tilemap, Vector3Int playerPosition)
    {
        Vector3Int randomPosition;
        do
        {
            randomPosition = new Vector3Int(Random.Range(-10, 10), Random.Range(-10, 10), 0); // Adjust range as needed
        } while (IsTileOccupied(randomPosition) || !tilemap.HasTile(randomPosition) || randomPosition == playerPosition);

        return randomPosition;
    }

    public void RegisterAI(MonoBehaviour ai)
    {
        if (ai is AIMove aiMove)
        {
            AddOccupiedPosition(aiMove.CurrentTilePosition);
        }
        else if (ai is BarraAI barraAI)
        {
            AddOccupiedPosition(barraAI.CurrentTilePosition);
        }
    }

    public void RegisterPlayer(PlayerMove player)
    {
        AddOccupiedPosition(player.CurrentTilePosition);
    }
}
