using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Puddle : MonoBehaviour
{
    [Header("References")]
    public Tilemap tilemap;
    public Hook hook;
    public StateMachine stateMachine; // For animation states (optional)

    [Header("Child Colliders")]
    [SerializeField] private Collider2D slideDown;
    [SerializeField] private Collider2D slideUp;
    [SerializeField] private Collider2D slideLeft;
    [SerializeField] private Collider2D slideRight;

    private GameObject thingInPuddle; // Tracks who's sliding

    private void Awake()
    {
        if (tilemap == null) tilemap = FindObjectOfType<Tilemap>();

        // Assign directions to child colliders (via tags or manual assignment)
        slideDown.gameObject.AddComponent<PuddleChildCollider>().Initialize(this, Vector3Int.down);
        slideUp.gameObject.AddComponent<PuddleChildCollider>().Initialize(this, Vector3Int.up);
        slideLeft.gameObject.AddComponent<PuddleChildCollider>().Initialize(this, Vector3Int.left);
        slideRight.gameObject.AddComponent<PuddleChildCollider>().Initialize(this, Vector3Int.right);
    }

    // Called by child colliders when something enters
    public void OnObjectEntered(Transform target, Vector3Int slideDirection)
    {
        if (thingInPuddle != null) return; // Only one at a time

        
        thingInPuddle = target.gameObject;
        Vector3Int targetTile = tilemap.WorldToCell(target.position);
        Vector3Int furthestTile = FindFurthestTile(targetTile, slideDirection);

        if (furthestTile != targetTile)
        {
            StartCoroutine(SlideObject(target, furthestTile));
        }
    }

    // Reused from Push.cs: Finds the farthest valid tile in a direction
    private Vector3Int FindFurthestTile(Vector3Int startTile, Vector3Int direction)
    {
        Vector3Int currentTile = startTile;
        while (AIUtils.IsTileValid(tilemap, OccupiedTilesManager.Instance, currentTile + direction, hook))
        {
            currentTile += direction;
        }
        return currentTile;
    }

    // Reused from Push.cs: Smoothly slides the object
    private IEnumerator SlideObject(Transform target, Vector3Int targetTilePosition)
    {
        Vector3 startPos = target.position;
        Vector3 endPos = tilemap.GetCellCenterWorld(targetTilePosition);
        float duration = 0.5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            target.position = Vector3.Lerp(startPos, endPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        target.position = endPos;
        UpdateTilePosition(target, targetTilePosition); // Update occupied tiles
        thingInPuddle = null; // Reset
    }

    // Handles updating tile positions for AI/Player/Goon (reused from Push.cs)
    private void UpdateTilePosition(Transform target, Vector3Int newTile)
    {
        // Player
        PlayerMove playerMove = target.GetComponent<PlayerMove>();
        if (playerMove != null)
        {
            OccupiedTilesManager.Instance.RemoveOccupiedPosition(playerMove.CurrentTilePosition);
            playerMove.CurrentTilePosition = newTile;
            OccupiedTilesManager.Instance.AddOccupiedPosition(newTile);
            return;
        }

        // AI (Electrician, etc.)
        AIMove aiMove = target.GetComponent<AIMove>();
        if (aiMove != null)
        {
            OccupiedTilesManager.Instance.RemoveOccupiedPosition(aiMove.CurrentTilePosition);
            aiMove.CurrentTilePosition = newTile;
            OccupiedTilesManager.Instance.AddOccupiedPosition(newTile);
            return;
        }

        // Goon
        GoonMove goonMove = target.GetComponent<GoonMove>();
        if (goonMove != null)
        {
            Vector3Int oldTile = goonMove.CurrentTilePosition;
            OccupiedTilesManager.Instance.RemoveOccupiedPosition(oldTile);
            goonMove.CurrentTilePosition = newTile;
            OccupiedTilesManager.Instance.AddOccupiedPosition(newTile);
            goonMove.OnPushedByPlayer(oldTile, newTile); // Notify Goon
        }
    }
}
