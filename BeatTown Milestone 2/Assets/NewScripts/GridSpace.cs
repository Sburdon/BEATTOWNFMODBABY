using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridSpace : MonoBehaviour
{
    [SerializeField] private int x;
    [SerializeField] private int y;

    // Public properties to access the grid position
    public int X => x;
    public int Y => y;

    // Pretty stuff: Visualize the coordinates in the Editor when selected
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, new Vector3(1, 1, 1));
        Gizmos.color = Color.black;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * 0.5f);
        UnityEditor.Handles.Label(transform.position + Vector3.up * 0.6f, $"({x}, {y})");
    }
}
