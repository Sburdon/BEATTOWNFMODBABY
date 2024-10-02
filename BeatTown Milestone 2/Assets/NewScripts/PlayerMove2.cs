using UnityEngine;

public class PlayerMove2 : MonoBehaviour
{
    public float moveSpeed = 5f;
    private GameManager2 gameManager;
    private Vector2 targetPosition;
    private bool isMoving = false;
    private bool movementEnabled = false;
    private bool jumpEnabled = false; // Flag for jump action

    public Vector2Int currentGridPos;  // Current player grid position
    private int gridMin = 1;           // Grid minimum value (inclusive)
    private int gridMax = 5;           // Grid maximum value (inclusive)

    void Start()
    {
        gameManager = FindObjectOfType<GameManager2>();
        targetPosition = transform.position;
    }

    public void InitializePlayerPosition(Vector2Int startGridPos)
    {
        currentGridPos = startGridPos;
        Debug.Log("Player starting at grid position: " + currentGridPos);
    }

    void Update()
    {
        // Check for movement or jumping based on the respective flags

        if (Input.GetMouseButtonDown(0) && !isMoving)
        {
            // If movement is enabled, handle movement; otherwise, handle jump
            if (movementEnabled)
            {
                DetectSquareClick();  // Handle regular movement
            }
            else if (jumpEnabled)
            {
                DetectJumpSquareClick();  // Handle jumping
            }
        }

        if (isMoving)
        {
            MovePlayer();
        }
    }

    public void EnableMovement(bool enable)
    {
        movementEnabled = enable;
        jumpEnabled = false; // reset jump move when normal move is enabled
    }

    public void EnableJump(bool enable)
    {
        Debug.Log("Jump enabled: "); // Debug log to check if the method is called (remove later)
        jumpEnabled = enable;
        movementEnabled = false; // reset normal move when jump is enabled
    }

    void DetectSquareClick()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D hit = Physics2D.GetRayIntersection(ray);

        if (hit.collider != null)
        {
            GameObject clickedSquare = hit.collider.gameObject;

            foreach (var square in gameManager.gridSquares)
            {
                if (square.Value == clickedSquare)
                {
                    Vector2Int targetGridPos = square.Key;  // This is the grid position of the clicked square

                    Debug.Log("Player current grid position: " + currentGridPos);
                    Debug.Log("Target grid position: " + targetGridPos);

                    if (jumpEnabled)
                    { // not implemented yet
                        Jump(targetGridPos); // Perform a jump
                    }
                    else if (movementEnabled)
                    {
                        // regular movement (already implemented)
                        // Check if the move is valid
                        if (gameManager.IsValidMove(currentGridPos, targetGridPos))
                        {
                            Debug.Log("Valid move!");
                            SetTargetPosition(clickedSquare.transform.position);
                            currentGridPos = targetGridPos;  // Update the player's current grid position
                            movementEnabled = false;  // Disable further movement until the next turn
                        }
                        else
                        {
                            Debug.Log("Invalid move: Too far or diagonal.");
                        }
                    }

                    break;
                }
            }
        }
    }

    // Handles clicking a square for jumping
    void DetectJumpSquareClick()
    {
        // Jump logic (not implemented in this context)
    }

    void Jump(Vector2Int targetGridPos)
    {
        // Jump logic (not implemented in this context)
    }

    void SetTargetPosition(Vector2 newPosition)
    {
        float playerHeight = GetComponent<SpriteRenderer>().bounds.size.y;
        Vector2 adjustedPosition = new Vector2(newPosition.x, newPosition.y + (playerHeight / 2));

        targetPosition = adjustedPosition;
        isMoving = true;
    }

    void MovePlayer()
    {
        transform.position = Vector2.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        if (Vector2.Distance(transform.position, targetPosition) < 0.1f)
        {
            transform.position = targetPosition;
            isMoving = false;
        }
    }
}
