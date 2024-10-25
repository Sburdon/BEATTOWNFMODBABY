using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RedSquare : MonoBehaviour
{
    public LayerMask enemyLayer; // The layer to check for enemies
    public LayerMask hook; // The layer for hook
    public float checkRadius = 0.1f; // The radius for the overlap check
    private SpriteRenderer spriteRenderer; // Reference to the SpriteRenderer component
    private bool isCollidingWithEnemy = false; // Track collision state with enemy

    private void Start()
    {
        // Get the SpriteRenderer component attached to this GameObject
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Initially disable the sprite
        spriteRenderer.enabled = false;
    }

    private void Update()
    {
        // Continuously check for enemy collisions
        CheckForEnemyCollision();
    }

    private void CheckForEnemyCollision()
    {
        // Check for collisions within a circle around the object's position for enemies
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, checkRadius, enemyLayer | hook);

        if (hitColliders.Length > 0) // If any colliders are found in the enemy layer
        {
            if (!isCollidingWithEnemy)
            {
                Debug.Log("Colliding with an enemy! Activating sprite.");
                spriteRenderer.enabled = true; // Enable the SpriteRenderer if colliding with an enemy
                isCollidingWithEnemy = true; // Update the collision state
            }
        }
        else
        {
            if (isCollidingWithEnemy)
            {
                Debug.Log("Not colliding with any enemies. Deactivating sprite.");
                spriteRenderer.enabled = false; // Disable the SpriteRenderer if not colliding
                isCollidingWithEnemy = false; // Update the collision state
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