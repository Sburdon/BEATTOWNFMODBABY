using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;

public class Swing : MonoBehaviour
{
    public Tilemap tilemap; // Reference to the Tilemap
    private Transform selectedEnemy; // Currently selected enemy
    private bool isSwinging; // State to track if we are in swing mode
    private bool enemySelected; // Track if an enemy has been selected
    private PlayerMove playerMove; // Reference to PlayerMove instance

    private void Awake()
    {
        playerMove = GetComponent<PlayerMove>();
    }

    void Update()
    {
        if (isSwinging)
        {
            if (!enemySelected)
            {
                // Step 1: Select an enemy when the left mouse button is clicked
                if (Input.GetMouseButtonDown(0)) // Left mouse button
                {
                    SelectEnemy();
                }
            }
            else
            {
                // Step 2: Select a tile to swing the enemy to
                if (Input.GetMouseButtonDown(0)) // Left mouse button
                {
                    TrySwingEnemy();
                }
            }
        }
    }

    public void OnSwingButtonPressed()
    {
        isSwinging = true; // Activate swinging mode
        enemySelected = false; // Reset enemy selection
        selectedEnemy = null; // Reset selected enemy
        Debug.Log("Swing button pressed.");

        // Optionally, check if any enemies are already in range to swing
        CheckEnemiesInRange();
    }

    public bool IsSwinging()
    {
        return isSwinging;
    }

    public void CancelSwing()
    {
        isSwinging = false;
        enemySelected = false;
        selectedEnemy = null;
        Debug.Log("Swing action canceled.");
    }

    void SelectEnemy()
    {
        // Raycast to check if an enemy is clicked
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

        if (hit.collider != null && hit.collider.CompareTag("Enemy"))
        {
            selectedEnemy = hit.collider.transform; // Select the enemy
            enemySelected = true; // Mark enemy as selected
            Debug.Log($"Selected enemy: {selectedEnemy.name}");
        }
        else
        {
            Debug.Log("No enemy selected.");
        }
    }

    void TrySwingEnemy()
    {
        if (selectedEnemy != null)
        {
            Vector3Int targetTilePosition = tilemap.WorldToCell(Camera.main.ScreenToWorldPoint(Input.mousePosition));

            Debug.Log($"Attempting to swing enemy {selectedEnemy.name} to {targetTilePosition}");

            // Check if the target tile is valid and within range
            if (IsTileValid(targetTilePosition) && IsWithinSwingRange(targetTilePosition))
            {
                StartCoroutine(MoveEnemyToTile(selectedEnemy, targetTilePosition));
                ResetSwingState();
            }
            else
            {
                Debug.Log("Invalid target tile or out of swing range.");
            }
        }
        else
        {
            Debug.Log("No enemy selected to swing.");
        }
    }

    bool IsWithinSwingRange(Vector3Int targetTilePosition)
    {
        Vector3Int playerCurrentPosition = tilemap.WorldToCell(transform.position);
        // Check if the target position is within swinging range (1 tile in any direction)
        return (Mathf.Abs(playerCurrentPosition.x - targetTilePosition.x) + Mathf.Abs(playerCurrentPosition.y - targetTilePosition.y) == 1);
    }

    bool IsTileValid(Vector3Int position)
    {
        // Check if the tile at the position is valid (not occupied or out of bounds)
        return tilemap.HasTile(position) && !IsTileOccupied(position);
    }

    bool IsTileOccupied(Vector3Int position)
    {
        // Check if the tile is occupied by another enemy
        Collider2D[] colliders = Physics2D.OverlapCircleAll(tilemap.GetCellCenterWorld(position), 0.1f);
        foreach (Collider2D collider in colliders)
        {
            if (collider.CompareTag("Enemy"))
            {
                return true; // Tile is occupied by another enemy
            }
        }
        return false; // Tile is not occupied
    }

    private IEnumerator MoveEnemyToTile(Transform enemy, Vector3Int targetTilePosition)
    {
        // Calculate target position
        Vector3 targetPosition = tilemap.GetCellCenterWorld(targetTilePosition);
        float elapsedTime = 0f;
        float moveDuration = 0.5f; // Duration of the move
        Vector3 startPosition = enemy.position;

        // Move towards the target position
        while (elapsedTime < moveDuration)
        {
            enemy.position = Vector3.Lerp(startPosition, targetPosition, (elapsedTime / moveDuration)); // Lerp for smooth movement
            elapsedTime += Time.deltaTime; // Increment elapsed time
            yield return null; // Wait for the next frame
        }

        // Ensure the enemy ends up exactly at the target position
        enemy.position = targetPosition;
        Debug.Log($"{enemy.name} has been swung to {targetTilePosition}");
    }

    private void CheckEnemiesInRange()
    {
        Vector3Int playerCurrentPosition = tilemap.WorldToCell(transform.position);

        // Check each enemy if it is within swinging range
        foreach (GameObject enemyObj in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            Transform enemy = enemyObj.transform;
            Vector3Int enemyPosition = tilemap.WorldToCell(enemy.position);
            if (IsWithinSwingRange(enemyPosition))
            {
                Debug.Log($"{enemy.name} is within swing range.");
            }
        }
    }

    private void ResetSwingState()
    {
        // Reset swing state after the action is completed
        isSwinging = false;
        enemySelected = false;
        selectedEnemy = null;
        Debug.Log("Swing action completed.");
    }
}
