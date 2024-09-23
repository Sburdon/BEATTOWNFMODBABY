using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public GameObject player1; // The human player
    public GameObject aiPlayer; // The AI-controlled player
    private GameObject currentPlayer;
    public Button moveButton;
    public Button punchButton;
    public Button endTurnButton;
    public Button spinButton;   // New Spin button
    public Tilemap tilemap;

    private bool isMoveModeActive = false;
    private bool isPunchModeActive = false;
    private bool isSpinModeActive = false; // New spin mode flag

    void Start()
    {
        currentPlayer = player1; // Start with player1
        SetupButtonListeners();
    }

    void SetupButtonListeners()
    {
        moveButton.onClick.AddListener(() => { isMoveModeActive = true; isPunchModeActive = false; isSpinModeActive = false; });
        punchButton.onClick.AddListener(() => { isPunchModeActive = true; isMoveModeActive = false; isSpinModeActive = false; });
        endTurnButton.onClick.AddListener(OnEndTurnClicked);
        spinButton.onClick.AddListener(() => { isSpinModeActive = true; isMoveModeActive = false; isPunchModeActive = false; }); // Spin button listener
    }

    void OnEndTurnClicked()
    {
        EndTurn();
    }

    void Update()
    {
        if (currentPlayer == player1 && Input.GetMouseButtonDown(0)) // Left mouse button
        {
            HandlePlayerInput();
        }
    }

    private void HandlePlayerInput()
    {
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int cellPosition = tilemap.WorldToCell(worldPosition);

        if (isMoveModeActive)
        {
            if (currentPlayer.GetComponent<PlayerMovement>())
            {
                currentPlayer.GetComponent<PlayerMovement>().MoveToCell(cellPosition);
            }
            isMoveModeActive = false; // Exit move mode after moving
        }
        else if (isPunchModeActive)
        {
            AttemptPunch(Input.mousePosition);
            isPunchModeActive = false; // Exit punch mode after attempting punch
        }
        else if (isSpinModeActive)
        {
            AttemptSpin(Input.mousePosition); // Handle the spin logic
            isSpinModeActive = false; // Exit spin mode after performing the spin
        }
    }

    private void AttemptPunch(Vector3 mousePosition)
    {
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
        Vector2 rayPosition = new Vector2(worldPosition.x, worldPosition.y);

        RaycastHit2D hit = Physics2D.Raycast(rayPosition, Vector2.zero);
        if (hit.collider != null)
        {
            GameObject clickedObject = hit.collider.gameObject;
            if (clickedObject == aiPlayer) // Only punch the aiPlayer
            {
                currentPlayer.GetComponent<PlayerPunch>().TryPunch(clickedObject);
                Debug.Log("Punch attempted on " + clickedObject.name);
            }
        }
    }

    private void AttemptSpin(Vector3 mousePosition)
    {
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
        Vector2 rayPosition = new Vector2(worldPosition.x, worldPosition.y);

        RaycastHit2D hit = Physics2D.Raycast(rayPosition, Vector2.zero);
        if (hit.collider != null && hit.collider.CompareTag("Enemy")) // Check if clicked object is tagged as "Enemy"
        {
            GameObject enemy = hit.collider.gameObject;
            MoveEnemyToAdjacentCell(enemy); // Move enemy to an adjacent empty cell
        }
    }

    private void MoveEnemyToAdjacentCell(GameObject enemy)
    {
        Vector3Int enemyCellPosition = tilemap.WorldToCell(enemy.transform.position);
        Vector3Int[] possibleDirections = {
            new Vector3Int(0, 1, 0),  // Up
            new Vector3Int(0, -1, 0), // Down
            new Vector3Int(1, 0, 0),  // Right
            new Vector3Int(-1, 0, 0)  // Left
        };

        foreach (Vector3Int direction in possibleDirections)
        {
            Vector3Int newPosition = enemyCellPosition + direction;
            if (IsCellEmpty(newPosition)) // Check if the target cell is empty
            {
                enemy.transform.position = tilemap.CellToWorld(newPosition) + new Vector3(0.5f, 0.5f, 0); // Move enemy to the new cell
                Debug.Log("Enemy moved to " + newPosition);
                return; // Exit after moving
            }
        }

        Debug.Log("No valid adjacent position found for enemy.");
    }

    private bool IsCellEmpty(Vector3Int cellPosition)
    {
        // Here you would check if the cell contains any other objects
        Collider2D collider = Physics2D.OverlapPoint(tilemap.CellToWorld(cellPosition));
        return collider == null; // True if no object occupies this cell
    }

    void EndTurn()
    {
        if (currentPlayer == player1)
        {
            currentPlayer = aiPlayer; // Switch to AI player
            AiTurn(); // AI performs its turn
        }
        else
        {
            currentPlayer = player1; // Switch back to human player
        }
        // Reset modes for the new turn
        isMoveModeActive = false;
        isPunchModeActive = false;
        isSpinModeActive = false;
    }

    void AiTurn()
    {
        if (aiPlayer == null)
        {
            Debug.LogError("AI Player is not set in GameManager.");
            return;
        }

        AiMovement aiMovement = aiPlayer.GetComponent<AiMovement>();
        if (aiMovement == null)
        {
            Debug.LogError("AiMovement component is missing on AI Player.");
            return;
        }

        AiPunch aiPunch = aiPlayer.GetComponent<AiPunch>();
        if (aiPunch == null)
        {
            Debug.LogError("AiPunch component is missing on AI Player.");
            return;
        }

        if (player1 == null)
        {
            Debug.LogError("Player1 is not set in GameManager.");
            return;
        }

        // Execute AI movement and punching logic
        aiMovement.MoveTowardsPlayer(player1); // AI decides if it moves or punches

        EndTurn(); // End AI turn automatically
    }
}