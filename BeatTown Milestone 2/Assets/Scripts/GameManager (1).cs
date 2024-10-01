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
    public Tilemap tilemap;

    private bool isMoveModeActive = false;
    private bool isPunchModeActive = false;

    void Start()
    {
        currentPlayer = player1; // Start with player1
        SetupButtonListeners();
    }

    void SetupButtonListeners()
    {
        moveButton.onClick.AddListener(() => { isMoveModeActive = true; isPunchModeActive = false; });
        punchButton.onClick.AddListener(() => { isPunchModeActive = true; isMoveModeActive = false; });
        endTurnButton.onClick.AddListener(OnEndTurnClicked);
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
