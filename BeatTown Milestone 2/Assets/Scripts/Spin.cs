using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spin : MonoBehaviour
{
    private Collider2D selectedEnemy; // Reference to the currently selected enemy
    private PlayerFatigue playerFatigue; // Reference to PlayerFatigue script
    private bool isSelectingEnemy = false; // State to check if we are selecting an enemy
    private bool isSelectingTarget = false; // State to check if we are selecting where to move the enemy

    void Start()
    {
        playerFatigue = FindObjectOfType<PlayerFatigue>();
        if (playerFatigue == null)
        {
            Debug.LogError("PlayerFatigue not found");
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
                // Move enemy to the selected grid
                MoveEnemyToCollider(selectedEnemy, hit.collider);
                playerFatigue.currentFatigue = playerFatigue.currentFatigue - 2; // Reduce fatigue after the spin action
                selectedEnemy = null; // Reset the selected enemy
                isSelectingTarget = false; // Stop selecting the target
                Debug.Log("Enemy moved to: " + hit.collider.name);
            }
        }
    }

    private void MoveEnemyToCollider(Collider2D enemyCollider, Collider2D newCollider)
    {
        // Move the enemy to the center of the new collider
        Vector3 newPosition = newCollider.bounds.center;
        newPosition.y += 0.5f; // Move up by 0.5 units
        enemyCollider.transform.position = newPosition;

        Debug.Log("Enemy moved to: " + newCollider.name); // Log successful move
    }
}