using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spin : MonoBehaviour
{
    private Collider2D selectedEnemy; // Reference to the currently selected enemy
    private PlayerFatigue playerFatigue; // Reference to PlayerFatigue script
    private PlayerMove2 playerMove; // Reference to PlayerMove2 script
    private bool isSelectingEnemy = false; // State to check if we are selecting an enemy
    private bool isSelectingTarget = false; // State to check if we are selecting where to move the enemy

    void Start()
    {
        playerFatigue = FindObjectOfType<PlayerFatigue>();
        playerMove = FindObjectOfType<PlayerMove2>();

        if (playerFatigue == null)
        {
            Debug.LogError("PlayerFatigue not found");
        }

        if (playerMove == null)
        {
            Debug.LogError("PlayerMove2 not found");
        }
    }

    // Call this when the spin button is clicked
    public void OnSpinButtonClick()
    {
        if (playerFatigue != null && playerFatigue.currentFatigue > 0)
        {
            isSelectingEnemy = true; // Start the enemy selection process
            Debug.Log("Select an enemy to move.");
        }
        else
        {
            Debug.LogWarning("Not enough fatigue to perform the spin action.");
        }
    }

    void Update()
    {
        // Check for mouse input if we're in enemy selection mode
        if (isSelectingEnemy && Input.GetMouseButtonDown(0))
        {
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

            if (hit.collider != null && hit.collider.CompareTag("Enemy"))
            {
                // If clicked on an enemy, select it and move to selecting the grid
                selectedEnemy = hit.collider;
                isSelectingEnemy = false;
                isSelectingTarget = true;
                Debug.Log("Selected enemy: " + selectedEnemy.name);
            }
        }
        // If we've selected the enemy, let the player select the grid to move to
        else if (isSelectingTarget && Input.GetMouseButtonDown(0))
        {
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

            if (hit.collider != null && hit.collider.CompareTag("Grid"))
            {
                // Get the grid position of the clicked tile
                Vector2Int targetGridPos = GetGridPositionFromCollider(hit.collider);

                // Get player's current grid position
                Vector2Int playerGridPos = playerMove.currentGridPos;

                // Validate the target position is one tile away from the player's position
                if (IsAdjacent(playerGridPos.x, playerGridPos.y, targetGridPos.x, targetGridPos.y))
                {
                    MoveEnemyToCollider(selectedEnemy, hit.collider);
                    playerFatigue.currentFatigue -= 2; // Reduce fatigue after the spin action
                    selectedEnemy = null; // Reset the selected enemy
                    isSelectingTarget = false; // Stop selecting the target
                    Debug.Log("Enemy moved to: " + hit.collider.name);
                }
                else
                {
                    Debug.Log("Invalid move: The target position is not adjacent to the player.");
                }
            }
        }
    }

    private void MoveEnemyToCollider(Collider2D enemyCollider, Collider2D newCollider)
    {
        // Move the enemy to the center of the new collider
        Vector3 newPosition = newCollider.bounds.center;
        newPosition.y += 0.5f; // Adjust the Y position to fit in the grid
        enemyCollider.transform.position = newPosition;

        Debug.Log("Enemy moved to: " + newCollider.name); // Log successful move
    }

    private Vector2Int GetGridPositionFromCollider(Collider2D collider)
    {
        // Simple extraction of grid position from the collider name
        string colliderName = collider.name; // e.g., "Square (3, 2)"
        string[] parts = colliderName.Replace("Square (", "").Replace(")", "").Split(',');

        int x = int.Parse(parts[0].Trim());
        int y = int.Parse(parts[1].Trim());

        return new Vector2Int(x, y);
    }

    private bool IsAdjacent(int playerX, int playerY, int targetX, int targetY)
    {
        return (Mathf.Abs(playerX - targetX) == 1 && playerY == targetY) ||
               (Mathf.Abs(playerY - targetY) == 1 && playerX == targetX);
    }
}
