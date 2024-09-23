using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinEnemy : MonoBehaviour
{
    public GameObject player;                  // Reference to the player character
    public GameObject[] emptyColliders;        // Array of empty colliders around the player
    private bool isSpinningEnabled = false;    // Whether the "Spin" button has been clicked

    void Update()
    {
        // Check if spin mode is enabled and the player clicks
        if (isSpinningEnabled && Input.GetMouseButtonDown(0))
        {
            // Cast a ray from the mouse position to detect objects
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

            if (hit.collider != null && hit.collider.CompareTag("Enemy"))
            {
                MoveEnemyToEmptySpace(hit.collider.gameObject);  // Move the enemy
            }
        }
    }

    // This method is called when the "Spin" button is clicked
    public void OnSpinButtonClicked()
    {
        isSpinningEnabled = true;  // Enable spin mode
    }

    // Moves the enemy to an empty collider space around the player
    private void MoveEnemyToEmptySpace(GameObject enemy)
    {
        // Find a new empty collider that is not the original position of the enemy
        foreach (GameObject emptyCollider in emptyColliders)
        {
            float distanceToEnemy = Vector2.Distance(enemy.transform.position, emptyCollider.transform.position);
            float distanceToPlayer = Vector2.Distance(player.transform.position, emptyCollider.transform.position);

            // Ensure the empty collider is not in the enemy's original position and is near the player
            if (distanceToEnemy > 0.1f && distanceToPlayer > 0.1f)
            {
                enemy.transform.position = emptyCollider.transform.position;  // Move enemy to the new location
                isSpinningEnabled = false;  // Disable spin mode after moving the enemy
                return;
            }
        }
    }
}