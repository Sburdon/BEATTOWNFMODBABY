using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyGridTracker : MonoBehaviour
{
    [SerializeField] private float gridCellSize = 1f; // Assuming each grid cell is 1x1 in size
    [SerializeField] private Transform feetTransform; // Reference to the "feet" GameObject
    public GridSpace currentGridSpace;

    public void Update()
    {
        // Constantly update the enemy's current grid space
        UpdateCurrentGridSpace();
    }

    void Start()
    {
        // Initialize the current grid space at the start of the game
        UpdateCurrentGridSpace();

        // Log the starting grid space
        if (currentGridSpace != null)
        {
            Debug.Log("Enemy starting at grid space: " + currentGridSpace.name);
        }
        else
        {
            Debug.LogWarning("Starting grid space not found!");
        }
    }

    void UpdateCurrentGridSpace()
    {
        if (feetTransform == null)
        {
            Debug.LogError("Feet transform not set for EnemyGridTracker.");
            return;
        }

         // Use the feet's position to snap to the nearest grid space
        Vector3 feetPosition = feetTransform.position;
        Vector3 snappedPosition = new Vector3(
            Mathf.Round(feetPosition.x / gridCellSize) * gridCellSize,
            Mathf.Round(feetPosition.y / gridCellSize) * gridCellSize,
            feetPosition.z);

        // Find the grid space corresponding to the snapped position
        foreach (var gridSpace in FindObjectsOfType<GridSpace>())
        {
            if (Vector2.Distance(gridSpace.transform.position, snappedPosition) < gridCellSize / 2)
            {
                if (gridSpace != currentGridSpace)
                {
                    currentGridSpace = gridSpace;
                    Debug.Log("Enemy's current grid space updated: " + currentGridSpace.name);
                }
                break;
            }
        }
    }

    public GridSpace GetCurrentGridSpace()
    {
        return currentGridSpace;
    }
}
