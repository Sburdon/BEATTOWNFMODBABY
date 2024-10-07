using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;

public class AiMovement : MonoBehaviour
{
    public Tilemap tilemap;
    public float moveSpeed = 3.0f;
    public AiPunch aiPunch; // Reference to AiPunch script

    private void Awake()
    {
        if (!tilemap)
        {
            Debug.LogError("Tilemap reference not set in AiMovement script.");
        }

        if (!aiPunch)
        {
            Debug.LogError("AiPunch reference not set in AiMovement script.");
        }
    }

    public void MoveTowardsPlayer(GameObject player)
    {
        if (!player)
        {
            Debug.LogError("Player reference not set for AiMovement.");
            return;
        }

        Vector3Int aiCell = tilemap.WorldToCell(transform.position);
        Vector3Int playerCell = tilemap.WorldToCell(player.transform.position);
        int distanceX = Math.Abs(playerCell.x - aiCell.x);
        int distanceY = Math.Abs(playerCell.y - aiCell.y);
        int movesNeeded = Mathf.Max(distanceX, distanceY);

        // Call punch function if within range
        if (movesNeeded == 1)
        {
            aiPunch.TryPunchIfInRange(player);
        }
        else if (movesNeeded > 1)
        {
            MoveAI(aiCell, playerCell, movesNeeded);
        }
    }

    private void MoveAI(Vector3Int aiCell, Vector3Int playerCell, int movesNeeded)
    {
        Vector3Int directionToPlayer = new Vector3Int(
            Mathf.Clamp(playerCell.x - aiCell.x, -1, 1),
            Mathf.Clamp(playerCell.y - aiCell.y, -1, 1),
            0
        );

        int movesToMake = Mathf.Min(movesNeeded - 1, 2); // Move 1 or 2 steps max
        Vector3Int targetCell = aiCell + directionToPlayer * movesToMake;

        // Check if the target cell has a tile and is not occupied
        if (tilemap.HasTile(targetCell) && !GridManager.Instance.IsCellOccupied(targetCell))
        {
            StartCoroutine(SmoothMoveToCell(targetCell));
        }
        else
        {
            Debug.Log("Move failed, target cell is either occupied or does not contain a valid tile.");
        }
    }

    private IEnumerator SmoothMoveToCell(Vector3Int targetCell)
    {
        Vector3 targetPosition = tilemap.GetCellCenterWorld(targetCell);
        float startTime = Time.time;
        Vector3 startPosition = transform.position;
        float journeyLength = Vector3.Distance(startPosition, targetPosition);
        float fracJourney;

        while (Vector3.Distance(transform.position, targetPosition) > 0.01f)
        {
            float distCovered = (Time.time - startTime) * moveSpeed;
            fracJourney = distCovered / journeyLength;
            transform.position = Vector3.Lerp(startPosition, targetPosition, fracJourney);
            yield return null;
        }
        transform.position = targetPosition;
        // Update grid occupancy after movement
        Vector3Int oldCell = tilemap.WorldToCell(startPosition);
        GridManager.Instance.SetCellOccupied(oldCell, false);
        GridManager.Instance.SetCellOccupied(targetCell, true);
    }
}
