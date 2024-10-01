using UnityEngine;
using UnityEngine.UI;  // For Canvas buttons
using System.Collections.Generic;

public class GameManager2 : MonoBehaviour
{
    public Dictionary<Vector2Int, GameObject> gridSquares;

    [System.Serializable]
    public class GridSquare
    {
        public Vector2Int gridPosition;
        public GameObject squareObject;
    }

    public GridSquare[] squares; // Assign manually in the Inspector

    public Button moveButton;     // Button for moving
    public Button attackButton;   // Button for attacking
    public Button endTurnButton;  // Button to end the turn

    private bool isPlayerTurn = true;  // True if it's the player's turn
    private bool canMove = false;      // Player can move only once per turn
    private bool canAttack = false;    // Player can attack only once per turn
    public GameObject player;  // Reference to the player GameObject
    public Vector2Int playerStartGridPosition = new Vector2Int(2, 3);  // Starting grid position
    private PlayerMove2 playerMoveScript;

    void Start()
    {
        gridSquares = new Dictionary<Vector2Int, GameObject>();
        InitializeGrid();

        playerMoveScript = FindObjectOfType<PlayerMove2>();

        // Set player initial position
        if (gridSquares.ContainsKey(playerStartGridPosition))
        {
            Vector3 startPosition = gridSquares[playerStartGridPosition].transform.position;
            player.transform.position = new Vector3(startPosition.x, startPosition.y + player.GetComponent<SpriteRenderer>().bounds.size.y / 2, player.transform.position.z);

            // Initialize the player's grid position in the PlayerMove2 script
            playerMoveScript.InitializePlayerPosition(playerStartGridPosition);
        }

        moveButton.onClick.AddListener(EnableMove);
        attackButton.onClick.AddListener(EnableAttack);
        endTurnButton.onClick.AddListener(EndTurn);

        StartPlayerTurn();  // Start with player's turn
    }

    void InitializeGrid()
    {
        foreach (GridSquare square in squares)
        {
            if (!gridSquares.ContainsKey(square.gridPosition))
            {
                gridSquares[square.gridPosition] = square.squareObject;
                square.squareObject.name = "Square (" + square.gridPosition.x + ", " + square.gridPosition.y + ")";
            }
        }
    }

    // Called when the player clicks the move button
    void EnableMove()
    {
        if (isPlayerTurn && !canMove)
        {
            playerMoveScript.EnableMovement(true);  // Enable movement in the PlayerMove2 script
            canMove = true;
        }
    }

    // Placeholder for enabling attack
    void EnableAttack()
    {
        if (isPlayerTurn && !canAttack)
        {
            Debug.Log("Attack enabled");  // Not implemented yet
            canAttack = true;
        }
    }

    // Called when the player ends their turn
    void EndTurn()
    {
        if (isPlayerTurn)
        {
            isPlayerTurn = false;
            Debug.Log("Player turn ended.");
            StartAITurn();
        }
    }

    // AI Turn logic
    void StartAITurn()
    {
        Debug.Log("AI turn success");
        Invoke("StartPlayerTurn", 2f);  // Wait 2 seconds and then start the player's turn again
    }

    // Starts the player's turn
    void StartPlayerTurn()
    {
        Debug.Log("Player's turn started.");
        isPlayerTurn = true;
        canMove = false;
        canAttack = false;
    }

    // Check if the player can move within 2 squares and not diagonally
    public bool IsValidMove(Vector2Int currentPos, Vector2Int targetPos)
    {
        int xDistance = Mathf.Abs(currentPos.x - targetPos.x);
        int yDistance = Mathf.Abs(currentPos.y - targetPos.y);

        Debug.Log("xDistance: " + xDistance + ", yDistance: " + yDistance);

        // Valid if it's within 2 squares and not diagonal
        if ((xDistance == 0 && yDistance > 0 && yDistance <= 2) ||
            (yDistance == 0 && xDistance > 0 && xDistance <= 2))
        {
            return true;
        }
        return false;
    }

}
