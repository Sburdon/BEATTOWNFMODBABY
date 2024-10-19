using UnityEngine;
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

    // Updated method to accept both AIMove and BarraAI
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

    // **Added RegisterPlayer method**
    public void RegisterPlayer(PlayerMove player)
    {
        AddOccupiedPosition(player.CurrentTilePosition);
    }
}
