using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;

public class Swing : MonoBehaviour
{
    public Tilemap tilemap;
    public float swingSpeed = 5f;
    private bool isSwingMode = false;
    private bool isSwinging = false;
    private GameObject enemyToSwing;
    private Vector3Int targetTilePosition;

    private PlayerFatigue playerFatigue;
    public int swingFatigueCost = 2; // Fatigue cost for swinging

    void Start()
    {
        playerFatigue = GetComponent<PlayerFatigue>();
    }

    // Method to be called when the swing button is pressed
    public void OnSwingButtonPressed()
    {
        if (isSwinging)
        {
            Debug.Log("Already swinging.");
            return;
        }

        if (isSwingMode)
        {
            Debug.Log("Swing mode already active.");
            return;
        }

        // Check if the player has enough fatigue
        if (!playerFatigue.CanPerformAction(swingFatigueCost))
        {
            Debug.Log("Not enough fatigue to swing.");
            return;
        }

        isSwingMode = true;
        Debug.Log("Swing mode activated. Click on an adjacent enemy to swing.");
    }

    void Update()
    {
        if (isSwingMode && !isSwinging)
        {
            if (Input.GetMouseButtonDown(0)) // Left mouse button
            {
                Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector3Int clickedTilePosition = tilemap.WorldToCell(mouseWorldPosition);
                Vector3Int playerTilePosition = tilemap.WorldToCell(transform.position);

                if (enemyToSwing == null)
                {
                    // First click: Select an adjacent enemy
                    if (IsAdjacent(playerTilePosition, clickedTilePosition))
                    {
                        Collider2D collider = Physics2D.OverlapPoint(tilemap.GetCellCenterWorld(clickedTilePosition));
                        if (collider != null && collider.CompareTag("Enemy"))
                        {
                            enemyToSwing = collider.gameObject;
                            Debug.Log("Enemy selected. Click on a valid adjacent tile to swing the enemy.");
                        }
                        else
                        {
                            Debug.Log("No enemy at the selected tile or not adjacent.");
                        }
                    }
                    else
                    {
                        Debug.Log("Selected tile is not adjacent to the player.");
                    }
                }
                else
                {
                    // Second click: Select a target tile
                    if (IsAdjacent(playerTilePosition, clickedTilePosition) && clickedTilePosition != playerTilePosition && clickedTilePosition != tilemap.WorldToCell(enemyToSwing.transform.position))
                    {
                        if (IsValidSwingTarget(clickedTilePosition))
                        {
                            targetTilePosition = clickedTilePosition;

                            // Deduct fatigue
                            playerFatigue.UseFatigue(swingFatigueCost);

                            StartCoroutine(SwingEnemy(enemyToSwing, targetTilePosition));

                            // Reset swing mode
                            isSwingMode = false;
                            enemyToSwing = null;
                        }
                        else
                        {
                            Debug.Log("Invalid tile for swinging.");
                        }
                    }
                    else
                    {
                        Debug.Log("Selected tile is not a valid target. It must be adjacent to the player and not the same as the enemy's current position.");
                    }
                }
            }
            else if (Input.GetMouseButtonDown(1)) // Right mouse button to cancel
            {
                Debug.Log("Swing action canceled.");
                isSwingMode = false;
                enemyToSwing = null;
            }
        }
    }

    private bool IsAdjacent(Vector3Int origin, Vector3Int target)
    {
        int dx = Mathf.Abs(origin.x - target.x);
        int dy = Mathf.Abs(origin.y - target.y);
        return (dx + dy == 1);
    }

    private bool IsValidSwingTarget(Vector3Int targetTilePosition)
    {
        // Check if the tile is within bounds
        if (!tilemap.HasTile(targetTilePosition))
        {
            return false;
        }

        // Allow swinging into the hook's tile
        if (OccupiedTilesManager.Instance.IsTileOccupied(targetTilePosition))
        {
            // Check if the occupied tile is the hook's tile
            if (Hook.Instance != null && Hook.Instance.GetHookPosition() == targetTilePosition)
            {
                // Allow swinging into the hook's tile
                return true;
            }
            else
            {
                // Tile is occupied by another unit
                return false;
            }
        }

        return true;
    }

    private IEnumerator SwingEnemy(GameObject enemy, Vector3Int targetTilePosition)
    {
        isSwinging = true;

        Vector3 startPos = enemy.transform.position;
        Vector3 endPos = tilemap.GetCellCenterWorld(targetTilePosition);

        float elapsedTime = 0f;
        float duration = 1f / swingSpeed;

        while (elapsedTime < duration)
        {
            enemy.transform.position = Vector3.Lerp(startPos, endPos, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        enemy.transform.position = endPos;

        // Handle occupied positions
        AIMove enemyMove = enemy.GetComponent<AIMove>();
        if (enemyMove != null)
        {
            // Remove old position
            OccupiedTilesManager.Instance.RemoveOccupiedPosition(enemyMove.CurrentTilePosition);
            // Update position
            enemyMove.CurrentTilePosition = targetTilePosition;
            // Add new position
            OccupiedTilesManager.Instance.AddOccupiedPosition(enemyMove.CurrentTilePosition);
        }

        // Check for collision with hook
        if (Hook.Instance != null && Hook.Instance.GetHookPosition() == targetTilePosition)
        {
            Hook.Instance.HandleSwingOrPushIntoHook(enemy);
        }

        isSwinging = false;
        Debug.Log("Swing action completed.");
    }

    public bool IsSwinging()
    {
        return isSwinging;
    }

    public void CancelSwing()
    {
        isSwingMode = false;
        isSwinging = false;
        enemyToSwing = null;
        Debug.Log("Swing action canceled.");
    }
}
