using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;
using static StateMachine;

public class Swing : MonoBehaviour
{
    public Tilemap tilemap;
    public float swingSpeed = 5f;
    private GameObject enemyToSwing;
    private Vector3Int targetTilePosition;
    private bool isSwingMode = false;
    private bool isSwinging = false;
    private PlayerFatigue playerFatigue;
    public int swingFatigueCost = 2; // Fatigue cost for swinging
    private StateMachine stateMachine;

    void Start()
    {
        playerFatigue = GetComponent<PlayerFatigue>();
        stateMachine = GetComponent<StateMachine>();
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
                if (enemyToSwing == null)
                {
                    // First click: Select an adjacent enemy
                    SelectEnemy();
                }
                else
                {
                    // Second click: Select a target tile
                    Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    Vector3Int clickedTilePosition = tilemap.WorldToCell(mouseWorldPosition);
                    Vector3Int playerTilePosition = tilemap.WorldToCell(transform.position);

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
                        Debug.Log("Selected tile is not a valid target.");
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

    private void SelectEnemy()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

        if (hit.collider != null && hit.collider.CompareTag("Enemy"))
        {
            Vector3Int enemyPosition = tilemap.WorldToCell(hit.collider.transform.position);
            Vector3Int playerPosition = tilemap.WorldToCell(transform.position);

            // Ensure the enemy is within swinging range (1 tile in each direction)
            if (IsAdjacent(playerPosition, enemyPosition))
            {
                enemyToSwing = hit.collider.gameObject; // Select the enemy
                Debug.Log($"Selected enemy for swing: {enemyToSwing.name}");
            }
            else
            {
                Debug.Log("Selected enemy is out of swing range.");
            }
        }
        else
        {
            Debug.Log("No enemy selected.");
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
            if (Hook.Instance != null && Hook.Instance.GetHookPosition() == targetTilePosition)
            {
                return true; // Allow swinging into the hook's tile
            }
            else
            {
                return false; // Tile is occupied by another unit
            }
        }

        return true;
    }

    private IEnumerator SwingEnemy(GameObject enemy, Vector3Int targetTilePosition)
    {
        isSwinging = true;

        Vector3 startPos = enemy.transform.position;
        Vector3 endPos = tilemap.GetCellCenterWorld(targetTilePosition);
        stateMachine.ChangeState(WrestlerState.Swing);

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
