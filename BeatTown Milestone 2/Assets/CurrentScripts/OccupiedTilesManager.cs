using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class OccupiedTilesManager : MonoBehaviour
{
    // Singleton Instance
    public static OccupiedTilesManager Instance { get; private set; }

    [Header("Tilemap Reference")]
    public Tilemap tilemap; // Assign this in the Inspector

    // HashSet to track occupied tile positions
    private HashSet<Vector3Int> occupiedTiles = new HashSet<Vector3Int>();

    void Awake()
    {
        // Implement Singleton Pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Optional: Persist across scenes
            Debug.Log("OccupiedTilesManager: Singleton instance initialized.");
        }
        else
        {
            Destroy(gameObject);
            Debug.LogWarning("OccupiedTilesManager: Duplicate instance detected and destroyed.");
        }
    }

    /// <summary>
    /// Registers the player by marking their current tile as occupied.
    /// </summary>
    /// <param name="playerMove">Reference to the PlayerMove script.</param>
    public void RegisterPlayer(PlayerMove playerMove)
    {
        if (playerMove != null)
        {
            Vector3Int playerTile = playerMove.CurrentTilePosition;
            AddOccupiedPosition(playerTile);
            Debug.Log($"OccupiedTilesManager: Player registered at {playerTile}");
        }
        else
        {
            Debug.LogWarning("OccupiedTilesManager: Attempted to register a null PlayerMove.");
        }
    }

    /// <summary>
    /// Unregisters the player by removing their current tile from occupied tiles.
    /// </summary>
    /// <param name="playerMove">Reference to the PlayerMove script.</param>
    public void UnregisterPlayer(PlayerMove playerMove)
    {
        if (playerMove != null)
        {
            Vector3Int playerTile = playerMove.CurrentTilePosition;
            RemoveOccupiedPosition(playerTile);
            Debug.Log($"OccupiedTilesManager: Player unregistered from {playerTile}");
        }
        else
        {
            Debug.LogWarning("OccupiedTilesManager: Attempted to unregister a null PlayerMove.");
        }
    }

    /// <summary>
    /// Registers an AIMove unit by adding its current tile position to occupied tiles.
    /// </summary>
    /// <param name="aiMove">Reference to the AIMove script.</param>
    public void RegisterAI(AIMove aiMove)
    {
        if (aiMove != null)
        {
            Vector3Int aiTile = aiMove.CurrentTilePosition;
            AddOccupiedPosition(aiTile);
            Debug.Log($"OccupiedTilesManager: AIMove registered at {aiTile}");
        }
        else
        {
            Debug.LogWarning("OccupiedTilesManager: Attempted to register a null AIMove.");
        }
    }

    /// <summary>
    /// Unregisters an AIMove unit by removing its current tile position from occupied tiles.
    /// </summary>
    /// <param name="aiMove">Reference to the AIMove script.</param>
    public void UnregisterAI(AIMove aiMove)
    {
        if (aiMove != null)
        {
            Vector3Int aiTile = aiMove.CurrentTilePosition;
            RemoveOccupiedPosition(aiTile);
            Debug.Log($"OccupiedTilesManager: AIMove unregistered from {aiTile}");
        }
        else
        {
            Debug.LogWarning("OccupiedTilesManager: Attempted to unregister a null AIMove.");
        }
    }

    /// <summary>
    /// Registers a BarraMove unit by adding its current tile position to occupied tiles.
    /// </summary>
    /// <param name="barraMove">Reference to the BarraMove script.</param>
    public void RegisterBarraMove(BarraMove barraMove)
    {
        if (barraMove != null)
        {
            Vector3Int barraTile = barraMove.CurrentTilePosition;
            AddOccupiedPosition(barraTile);
            Debug.Log($"OccupiedTilesManager: BarraMove registered at {barraTile}");
        }
        else
        {
            Debug.LogWarning("OccupiedTilesManager: Attempted to register a null BarraMove.");
        }
    }

    /// <summary>
    /// Unregisters a BarraMove unit by removing its current tile position from occupied tiles.
    /// </summary>
    /// <param name="barraMove">Reference to the BarraMove script.</param>
    public void UnregisterBarraMove(BarraMove barraMove)
    {
        if (barraMove != null)
        {
            Vector3Int barraTile = barraMove.CurrentTilePosition;
            RemoveOccupiedPosition(barraTile);
            Debug.Log($"OccupiedTilesManager: BarraMove unregistered from {barraTile}");
        }
        else
        {
            Debug.LogWarning("OccupiedTilesManager: Attempted to unregister a null BarraMove.");
        }
    }

    /// <summary>
    /// Adds a tile position to the occupied tiles set.
    /// </summary>
    /// <param name="position">Tile position to add.</param>
    public void AddOccupiedPosition(Vector3Int position)
    {
        if (!occupiedTiles.Contains(position))
        {
            occupiedTiles.Add(position);
            Debug.Log($"OccupiedTilesManager: Added occupied position {position}");
        }
        else
        {
            Debug.LogWarning($"OccupiedTilesManager: Position {position} is already occupied.");
        }
    }

    /// <summary>
    /// Removes a tile position from the occupied tiles set.
    /// </summary>
    /// <param name="position">Tile position to remove.</param>
    public void RemoveOccupiedPosition(Vector3Int position)
    {
        if (occupiedTiles.Contains(position))
        {
            occupiedTiles.Remove(position);
            Debug.Log($"OccupiedTilesManager: Removed occupied position {position}");
        }
        else
        {
            Debug.LogWarning($"OccupiedTilesManager: Position {position} was not occupied.");
        }
    }

    /// <summary>
    /// Checks if a tile position is occupied.
    /// </summary>
    /// <param name="tilePosition">Tile position to check.</param>
    /// <returns>True if occupied; otherwise, false.</returns>
    public bool IsTileOccupied(Vector3Int tilePosition)
    {
        return occupiedTiles.Contains(tilePosition);
    }

    /// <summary>
    /// Finds a random available tile position within the tilemap, avoiding occupied tiles and the player's position.
    /// </summary>
    /// <param name="playerTilePosition">Player's current tile position.</param>
    /// <returns>A random available Vector3Int position. Returns Vector3Int.zero if no positions are available.</returns>
    public Vector3Int GetRandomAvailablePosition(Vector3Int playerTilePosition)
    {
        if (tilemap == null)
        {
            Debug.LogError("OccupiedTilesManager: Tilemap reference is missing.");
            return Vector3Int.zero;
        }

        List<Vector3Int> availablePositions = new List<Vector3Int>();

        BoundsInt bounds = tilemap.cellBounds;

        for (int x = bounds.xMin; x <= bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y <= bounds.yMax; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);
                if (tilemap.HasTile(pos) && !occupiedTiles.Contains(pos))
                {
                    // Ensure the spawn position is at least 2 tiles away from the player
                    int deltaX = Mathf.Abs(pos.x - playerTilePosition.x);
                    int deltaY = Mathf.Abs(pos.y - playerTilePosition.y);
                    if (deltaX + deltaY >= 2)
                    {
                        availablePositions.Add(pos);
                    }
                }
            }
        }

        if (availablePositions.Count == 0)
        {
            Debug.LogWarning("OccupiedTilesManager: No available positions found for spawning.");
            return Vector3Int.zero; // Indicate no available position
        }

        // Select a random position from the available list
        int randomIndex = Random.Range(0, availablePositions.Count);
        Vector3Int selectedPosition = availablePositions[randomIndex];
        Debug.Log($"OccupiedTilesManager: Selected spawn position {selectedPosition}");
        return selectedPosition;
    }
}
