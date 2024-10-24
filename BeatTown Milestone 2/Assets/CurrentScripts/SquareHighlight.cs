using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquareHighlight : MonoBehaviour
{
    public LayerMask wallLayer; // The layer to check for walls
    public float checkRadius = 0.1f; // The radius for the overlap check

    private void Start()
    {
        // Check if the square is already colliding with a wall at the time of spawn
        CheckForWallCollision();
    }

    private void Update()
    {
        CheckForWallCollision();
    }

    private void CheckForWallCollision()
    {
        // Check for collisions within a circle around the object's position
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, checkRadius, wallLayer);

        if (hitColliders.Length > 0) // If any colliders are found in the wall layer
        {
            Debug.Log("Colliding with Wall!");
            // Disable the object if it's colliding with a wall
            gameObject.SetActive(false);
        }
        else
        {
            // Only re-enable the object if it was previously disabled
            if (!gameObject.activeSelf)
            {
                Debug.Log("Not colliding with Wall. Re-enabling square.");
                gameObject.SetActive(true); // Reactivate the object if not colliding
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Draw a wire sphere in the editor for visual debugging
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, checkRadius);
    }
}