using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerMovement : MonoBehaviour
{
    public Tilemap tilemap;
    public float moveSpeed = 3.0f; // Speed of the movement

    public void MoveToCell(Vector3Int cellPosition)
    {
        if (tilemap.HasTile(cellPosition) && !GridManager.Instance.IsCellOccupied(cellPosition))
        {
            // If the cell is valid and not occupied, initiate movement
            StartCoroutine(SmoothMoveToCell(cellPosition));
            // Play the movement sound
            FMOD.Studio.EventInstance RingStep;
            RingStep = FMODUnity.RuntimeManager.CreateInstance("event:/Ring_Sounds/RingStep");
            RingStep.start();
        }
        else
        {
            // Log error if the tile is not available or the cell is occupied
            Debug.LogError("Cannot move to cell: It is either occupied or does not contain a tile.");
        }
    }

    public IEnumerator SmoothMoveToCell(Vector3Int targetCell)
    {
        Vector3 targetPosition = tilemap.GetCellCenterWorld(targetCell);
        float startTime = Time.time;
        Vector3 startPosition = transform.position;
        float journeyLength = Vector3.Distance(startPosition, targetPosition);
        float fracJourney = 0f;

        while (fracJourney < 1f)
        {
            float distCovered = (Time.time - startTime) * moveSpeed;
            fracJourney = distCovered / journeyLength;
            transform.position = Vector3.Lerp(startPosition, targetPosition, fracJourney);
            yield return null;
        }

        transform.position = targetPosition; // Ensure the player exactly reaches the target position

        // Update the grid occupancy
        Vector3Int oldCell = tilemap.WorldToCell(startPosition);
        GridManager.Instance.SetCellOccupied(oldCell, false);
        GridManager.Instance.SetCellOccupied(targetCell, true);
    }
}
