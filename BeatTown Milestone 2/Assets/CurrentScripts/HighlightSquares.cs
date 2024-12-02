using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighlightSquares : MonoBehaviour
{
    public LayerMask wallLayer; // The layer to check for walls
    public LayerMask enemyLayer; // The layer to check for enemies
    public LayerMask hookLayer; // The layer to check for the hook
    private SpriteRenderer spriteRenderer; // Reference to the SpriteRenderer component
    private bool isCollidingWithObstacle = false; // Track collision state with wall or enemy

    private void Start()
    {
        // Get the SpriteRenderer component attached to this GameObject
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if the colliding object is in the specified layers
        if (IsInLayerMask(collision.gameObject, wallLayer | enemyLayer | hookLayer))
        {
            Debug.Log("Colliding with an obstacle! Disabling sprite.");
            spriteRenderer.enabled = false; // Disable the SpriteRenderer
            isCollidingWithObstacle = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // Check if the colliding object was in the specified layers
        if (IsInLayerMask(collision.gameObject, wallLayer | enemyLayer | hookLayer))
        {
            Debug.Log("No longer colliding with obstacles. Re-enabling sprite.");
            spriteRenderer.enabled = true; // Re-enable the SpriteRenderer
            isCollidingWithObstacle = false;
        }
    }

    private bool IsInLayerMask(GameObject obj, LayerMask layerMask)
    {
        // Check if the object's layer is in the specified LayerMask
        return (layerMask.value & (1 << obj.layer)) > 0;
    }
}