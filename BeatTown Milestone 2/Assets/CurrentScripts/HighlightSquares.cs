using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighlightSquares : MonoBehaviour
{
    public LayerMask wallLayer; // The layer to check for walls
    public float checkRadius = 0.1f; // The radius for the overlap check
    private SpriteRenderer spriteRenderer; // Reference to the SpriteRenderer component
    private bool isCollidingWithWall = false; // Track collision state

    private void Start()
    {
        // Get the SpriteRenderer component attached to this GameObject
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Check if the square is already colliding with a wall at the time of spawn
        CheckForWallCollision();
    }

    private void Update()
    {
        // Continuously check for wall collisions
        CheckForWallCollision();
    }

    private void CheckForWallCollision()
    {
        // Check for collisions within a circle around the object's position
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, checkRadius, wallLayer);

        if (hitColliders.Length > 0) // If any colliders are found in the wall layer
        {
            if (!isCollidingWithWall)
            {
                Debug.Log("Colliding with Wall! Disabling sprite.");
                spriteRenderer.enabled = false; // Disable the SpriteRenderer if colliding with a wall
                isCollidingWithWall = true; // Update the collision state
            }
        }
        else
        {
            if (isCollidingWithWall)
            {
                Debug.Log("Not colliding with Wall. Re-enabling sprite.");
                spriteRenderer.enabled = true; // Reactivate the SpriteRenderer if not colliding
                isCollidingWithWall = false; // Update the collision state
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