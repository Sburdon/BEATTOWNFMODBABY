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
    public Button spinButton;     // Button for spinning the enemy (new)

    private bool isPlayerTurn = true;  // True if it's the player's turn
    private bool canMove = false;      // Player can move only once per turn
    private bool canAttack = false;    // Player can attack only once per turn
    private bool isSpinMode = false;   // True if the player has enabled spin mode (new)

    public GameObject player;  // Reference to the player GameObject
    public GameObject enemy;   // Reference to the enemy GameObject (new)

    public Vector2Int playerStartGridPosition = new Vector2Int(2, 3);  // Starting grid position
    public Vector2Int enemyStartGridPosition = new Vector2Int(5, 3);   // Enemy starting grid position (new)

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
            playerMoveScript.InitializePlayerPosition(playerStartGridPosition);
        }

        // Set enemy initial position (new)
        if (gridSquares.ContainsKey(enemyStartGridPosition))
        {
            Vector3 enemyPosition = gridSquares[enemyStartGridPosition].transform.position;
            enemy.transform.position = new Vector3(enemyPosition.x, enemyPosition.y + enemy.GetComponent<SpriteRenderer>().bounds.size.y / 2, enemy.transform.position.z);
        }

        moveButton.onClick.AddListener(EnableMove);
        attackButton.onClick.AddListener(EnableAttack);
        endTurnButton.onClick.AddListener(EndTurn);
        //spinButton.onClick.AddListener(EnableSpin);  // New spin button listener

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
            isSpinMode = false;  // Disable spin mode if moving
        }
    }

    // Placeholder for enabling attack
    void EnableAttack()
    {
        if (isPlayerTurn && !canAttack)
        {
            Debug.Log("Attack enabled");  // Not implemented yet
            canAttack = true;
            isSpinMode = false;  // Disable spin mode if attacking
        }
    }

    // Enable spin mode
    void EnableSpin()
    {
        if (isPlayerTurn)
        {
            isSpinMode = true;
            canMove = false;  // Disable movement if in spin mode
            canAttack = false;  // Disable attack if in spin mode
            Debug.Log("Spin mode enabled");
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
        isSpinMode = false;
    }

    // Handle mouse input during spin mode
    void Update()
    {
        if (isSpinMode && isPlayerTurn && Input.GetMouseButtonDown(0))
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2Int clickedGridPosition = GetGridPositionFromWorldPosition(mousePosition);

            // Check if clicked on the enemy and move the enemy to a nearby valid square
            if (gridSquares.ContainsKey(clickedGridPosition) && enemy.transform.position == gridSquares[clickedGridPosition].transform.position)
            {
                MoveEnemyToAdjacentSquare(clickedGridPosition);
            }
        }
    }

    // Move enemy to an adjacent valid square
    void MoveEnemyToAdjacentSquare(Vector2Int enemyGridPosition)
    {
        Vector2Int[] adjacentSquares = {
            new Vector2Int(enemyGridPosition.x, enemyGridPosition.y + 1),  // Up
            new Vector2Int(enemyGridPosition.x, enemyGridPosition.y - 1),  // Down
            new Vector2Int(enemyGridPosition.x + 1, enemyGridPosition.y),  // Right
            new Vector2Int(enemyGridPosition.x - 1, enemyGridPosition.y)   // Left
        };

        foreach (Vector2Int adjacentSquare in adjacentSquares)
        {
            if (gridSquares.ContainsKey(adjacentSquare) && IsSquareEmpty(adjacentSquare))
            {
                Vector3 newPosition = gridSquares[adjacentSquare].transform.position;
                enemy.transform.position = new Vector3(newPosition.x, newPosition.y + enemy.GetComponent<SpriteRenderer>().bounds.size.y / 2, enemy.transform.position.z);
                Debug.Log("Enemy moved to " + adjacentSquare);
                return;  // Move to the first valid square
            }
        }

        Debug.Log("No valid adjacent squares to move the enemy.");
    }

    // Check if a grid square is empty
    bool IsSquareEmpty(Vector2Int gridPosition)
    {
        Collider2D collider = Physics2D.OverlapPoint(gridSquares[gridPosition].transform.position);
        return collider == null;  // Returns true if no object is in this square
    }

    // Convert world position to grid position
    Vector2Int GetGridPositionFromWorldPosition(Vector3 worldPosition)
    {
        foreach (KeyValuePair<Vector2Int, GameObject> entry in gridSquares)
        {
            if (Vector3.Distance(entry.Value.transform.position, worldPosition) < 0.5f) // Small threshold to account for precision
            {
                return entry.Key;
            }
        }
        return Vector2Int.zero; // Default value if no valid grid position is found
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