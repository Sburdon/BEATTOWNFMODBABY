using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighlightSquares : MonoBehaviour
{
    public LayerMask wallLayer; // The layer to check for walls
    public LayerMask enemyLayer; // The layer to check for enemies
    public LayerMask hook; // The layer to check for the hook
    public float checkRadius = 0.1f; // The radius for the overlap check
    private SpriteRenderer spriteRenderer; // Reference to the SpriteRenderer component
    private bool isCollidingWithObstacle = false; // Track collision state with wall or enemy

    private void Start()
    {
        // Get the SpriteRenderer component attached to this GameObject
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Check if the square is already colliding with a wall or enemy at the time of spawn
        CheckForObstacleCollision();
    }

    private void Update()
    {
        // Continuously check for wall and enemy collisions
        CheckForObstacleCollision();
    }

    private void CheckForObstacleCollision()
    {
        // Check for collisions within a circle around the object's position for walls and enemies
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, checkRadius, wallLayer | enemyLayer | hook);

        if (hitColliders.Length > 0) // If any colliders are found in the wall or enemy layer
        {
            if (!isCollidingWithObstacle)
            {
                Debug.Log("Colliding with an obstacle! Disabling sprite.");
                spriteRenderer.enabled = false; // Disable the SpriteRenderer if colliding with a wall or enemy
                isCollidingWithObstacle = true; // Update the collision state
            }
        }
        else
        {
            if (isCollidingWithObstacle)
            {
                Debug.Log("Not colliding with any obstacles. Re-enabling sprite.");
                spriteRenderer.enabled = true; // Reactivate the SpriteRenderer if not colliding
                isCollidingWithObstacle = false; // Update the collision state
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