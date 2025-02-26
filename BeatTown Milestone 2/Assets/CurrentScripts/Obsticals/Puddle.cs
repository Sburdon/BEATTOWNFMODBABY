using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Puddle : MonoBehaviour
{


    [Header("ScriptRefs")] 
    private ElectricianMove electricianMove;
    private GoonMove goonMove;
    private PlayerMove playerMove;
    private GameObject thingInPuddle; // Electrician, Goon, or Player

    [Header("References")]
    public Tilemap tilemap;

    private void Awake()
    {
        if (tilemap == null)
        {
            tilemap = FindObjectOfType<Tilemap>();
            if (tilemap == null)
                Debug.LogError($"GoonMove: No Tilemap found for {name}!");
        }
    }

    // Called when something enters the puddle('s trigger)
    private void OnTriggerEnter2D(Collider2D collision) 
    {
        // Set the proper reference and flag depending on what type of object hit the puddle
        if (collision.gameObject.CompareTag("Electrician"))
        {
            electricianMove = collision.gameObject.GetComponent<ElectricianMove>();
            thingInPuddle = collision.gameObject;
            electricianMove.InPuddle = true; // bool for Electrician script (not used for anything yet)
        }
        else if (collision.gameObject.CompareTag("Goon"))
        {
            goonMove = collision.gameObject.GetComponent<GoonMove>();
            thingInPuddle = collision.gameObject;
            goonMove.InPuddle = true; // bool for Goon script (not used for anything yet)
        }
        else if (collision.gameObject.CompareTag("Player"))
        {
            playerMove = collision.gameObject.GetComponent<PlayerMove>();
            thingInPuddle = collision.gameObject;
            playerMove.InPuddle = true; // bool for Player script (not used for anything yet)
        }
        else
        {
            return;
        }

        // the object “as far as possible” in the direction it’s (supposed to be) facing.
        ThingInPuddle(thingInPuddle);
    }

    /// <summary>
    /// Determines the slide direction (always right for now), computes the furthest valid tile, and starts the sliding coroutine.
    /// </summary>
    /// <param name="thing">The object (Player, Goon, or Electrician) in the puddle.</param>
    /// 
    private void ThingInPuddle(GameObject thing)
    {
        // Get the object's current tile and determine the slide direction 
        Vector3Int startTile = Vector3Int.zero;
        Vector3Int direction = Vector3Int.zero;

        if (thing.CompareTag("Player"))
        {
            startTile = playerMove.CurrentTilePosition;
            // Default to sliding right
            direction = Vector3Int.right;

        }
        else if (thing.CompareTag("Goon"))
        {
            startTile = goonMove.CurrentTilePosition;
            // Default to sliding right
            direction = Vector3Int.right;

        }

        else if (thing.CompareTag("Electrician"))
        {
            startTile = electricianMove.CurrentTilePosition;
            // Default to sliding right
            direction = Vector3Int.right;

        }

        // In case something went wrong
        Debug.Log("Something went wrong with puddle;");

        // Remove the current tile from the occupied–tiles list so the sliding object does not block itself.
        if (OccupiedTilesManager.Instance != null)
        {
            OccupiedTilesManager.Instance.RemoveOccupiedPosition(startTile);
        }


        // Step tile by tile in the desired direction until we hit a “wall” (no tile) or an occupied tile.
        Vector3Int currentTile = startTile;
        
        while (IsTileValid(currentTile + direction))
        {
            currentTile += direction;
        }
        Vector3Int targetTile = currentTile;

        // Start Coroutine that will animate the sldie 
        StartCoroutine(SlideObjectToTile(thing, targetTile));
    }


    /// <summary>
    /// Checks whether the given tile is “valid” for sliding into:
    /// (1) It must exist on the tilemap (i.e. not be a wall)
    /// (2) It must not already be occupied.
    /// </summary>
    private bool IsTileValid(Vector3Int tilePos)
    {
        if (tilemap == null)
        {
            Debug.LogError("Puddle: Tilemap not set!");
            return false;
        }
        if (!tilemap.HasTile(tilePos)) // no tile means a wall or non-traversable cell
        {
            return false;
        }
        if (OccupiedTilesManager.Instance != null && OccupiedTilesManager.Instance.IsTileOccupied(tilePos))
        {
            return false;
        }
        return true;
    }


    /// <summary>
    /// Slides the object from its current world position to the center of targetTile.
    /// Updates its current tile position and re–marks the target tile as occupied.
    /// </summary>
    private IEnumerator SlideObjectToTile(GameObject thing, Vector3Int targetTile)
    {
        Vector3 startPos = thing.transform.position;
        Vector3 endPos = tilemap.GetCellCenterWorld(targetTile);
        float travelTime = 0.5f;
        float elapsedTime = 0f;

        while (elapsedTime < travelTime)
        {
            thing.transform.position = Vector3.Lerp(startPos, endPos, elapsedTime / travelTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        thing.transform.position = endPos;

        // Now update the object’s movement script to reflect its new tile.
        if (thing.CompareTag("Player") && playerMove != null)
        {
            playerMove.CurrentTilePosition = targetTile;
            if (OccupiedTilesManager.Instance != null)
                OccupiedTilesManager.Instance.AddOccupiedPosition(targetTile);
           // playerMove.InPuddle = false; // not used for anything yet
        }
        else if (thing.CompareTag("Goon") && goonMove != null)
        {
            goonMove.CurrentTilePosition = targetTile;
            if (OccupiedTilesManager.Instance != null)
                OccupiedTilesManager.Instance.AddOccupiedPosition(targetTile);
          //  goonMove.InPuddle = false; // not used for anything yet
        }
        else if (thing.CompareTag("Electrician") && electricianMove != null)
        {
            electricianMove.CurrentTilePosition = targetTile;
            if (OccupiedTilesManager.Instance != null)
                OccupiedTilesManager.Instance.AddOccupiedPosition(targetTile);
           // electricianMove.InPuddle = false; // not used for anything yet
        }
        yield break;
    }
}
