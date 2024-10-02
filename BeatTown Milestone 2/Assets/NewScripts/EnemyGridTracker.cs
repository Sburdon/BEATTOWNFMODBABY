using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyGridTracker : MonoBehaviour
{
    [SerializeField] private float gridCellSize = 1f; // Assuming each grid cell is 1x1 in size
    [SerializeField] private Transform feetTransform; // Reference to the "feet" GameObject
    public GridSpace currentGridSpace;

    void Update()
    {
        // Constantly update the enemy's current grid space
        UpdateCurrentGridSpace();
    }

    void UpdateCurrentGridSpace()
    {
        if (feetTransform == null)
        {
            Debug.LogError("Feet transform not set for EnemyGridTracker.");
            return;
        }

        // Snap the feet's position to the nearest grid space based on grid size
        Vector3 snappedPosition = new Vector3(
            Mathf.Round(feetTransform.position.x / gridCellSize) * gridCellSize,
            Mathf.Round(feetTransform.position.y / gridCellSize) * gridCellSize,
            feetTransform.position.z);

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
