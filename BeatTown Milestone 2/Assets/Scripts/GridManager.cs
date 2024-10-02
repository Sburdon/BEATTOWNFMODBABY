using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; } // Singleton instance

    public Tilemap tilemap; // Reference to your Tilemap
    private Dictionary<Vector3Int, bool> occupiedCells = new Dictionary<Vector3Int, bool>();

    void Awake()
    {
        // Ensure that there's only one instance of GridManager
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        InitializeGrid();
    }

    void Start()
    {
        // If tilemap is not assigned in the inspector, find it
        if (tilemap == null)
        {
            tilemap = GetComponent<Tilemap>();
            if (tilemap == null)
            {
                Debug.LogError("Tilemap not found! Please assign a Tilemap to the GridManager.");
            }
        }
    }

    void InitializeGrid()
    {
        foreach (var position in tilemap.cellBounds.allPositionsWithin)
        {
            if (tilemap.HasTile(position))
            {
                occupiedCells[position] = false;  // Initialize all cells as unoccupied
            }
        }
    }

    public bool IsCellOccupied(Vector3Int cellPosition)
    {
        // Check if the position has a tile and if it's marked as occupied
        return tilemap.HasTile(cellPosition) && (occupiedCells.ContainsKey(cellPosition) && occupiedCells[cellPosition]);
    }

    public void SetCellOccupied(Vector3Int cellPosition, bool occupied)
    {
        if (occupiedCells.ContainsKey(cellPosition))
        {
            occupiedCells[cellPosition] = occupied;
        }
        else if (tilemap.HasTile(cellPosition))
        {
            // If the cell is valid but not in the dictionary, add it as occupied
            occupiedCells[cellPosition] = occupied;
        }
    }
}
