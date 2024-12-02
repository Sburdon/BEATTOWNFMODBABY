using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiagSquar : MonoBehaviour
{
    public LayerMask wallLayer; // The layer to check for walls
    public LayerMask enemyLayer; // The layer to check for enemies
    public LayerMask hookLayer; // The layer to check for the hook
    public RedSquare redSquareScript; // Reference to the RedSquare script
    private SpriteRenderer spriteRenderer; // Reference to the SpriteRenderer component
    private bool isCollidingWithObstacle = false; // Track collision state with walls, enemies, or other conditions
    private HashSet<GameObject> walls = new HashSet<GameObject>(); // Track walls in collision
    private HashSet<GameObject> enemies = new HashSet<GameObject>(); // Track enemies in collision
    private HashSet<GameObject> hooks = new HashSet<GameObject>(); // Track hooks in collision

    private void Start()
    {
        // Get the SpriteRenderer component attached to this GameObject
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Ensure the RedSquare script reference is set
        if (redSquareScript == null)
        {
            Debug.LogError("RedSquare script reference is missing!");
        }
    }

    private void Update()
    {
        // Check for collision states and RedSquare condition
        bool isCollidingWithWall = walls.Count > 0;
        bool isCollidingWithBoth = enemies.Count > 0 && hooks.Count > 0;
        bool hasTwoOrMoreEnemies = enemies.Count >= 2;
        bool redSquareCondition = redSquareScript != null && redSquareScript.isCollidingWithEnemy;

        // Disable sprite if any of the conditions are met
        if (isCollidingWithWall || isCollidingWithBoth || hasTwoOrMoreEnemies || redSquareCondition)
        {
            if (!isCollidingWithObstacle)
            {
                Debug.Log("Conditions met. Disabling sprite.");
                spriteRenderer.enabled = false; // Disable the sprite
                isCollidingWithObstacle = true; // Update the collision state
            }
        }
        else
        {
            if (isCollidingWithObstacle)
            {
                Debug.Log("Conditions not met. Enabling sprite.");
                spriteRenderer.enabled = true; // Enable the sprite
                isCollidingWithObstacle = false; // Update the collision state
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Add objects to the appropriate set based on their layer
        if (IsInLayerMask(other.gameObject, wallLayer))
        {
            walls.Add(other.gameObject);
        }
        else if (IsInLayerMask(other.gameObject, enemyLayer))
        {
            enemies.Add(other.gameObject);
        }
        else if (IsInLayerMask(other.gameObject, hookLayer))
        {
            hooks.Add(other.gameObject);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // Remove objects from the appropriate set when they exit
        if (IsInLayerMask(other.gameObject, wallLayer))
        {
            walls.Remove(other.gameObject);
        }
        else if (IsInLayerMask(other.gameObject, enemyLayer))
        {
            enemies.Remove(other.gameObject);
        }
        else if (IsInLayerMask(other.gameObject, hookLayer))
        {
            hooks.Remove(other.gameObject);
        }
    }

    private bool IsInLayerMask(GameObject obj, LayerMask layerMask)
    {
        // Check if a GameObject's layer is in the specified LayerMask
        return (layerMask.value & (1 << obj.layer)) > 0;
    }
}