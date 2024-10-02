using UnityEngine;

public class PlayerMove2 : MonoBehaviour
{
    public float moveSpeed = 5f;
    public GameManager2 gameManager;
    private Vector2 targetPosition;
    private bool isMoving = false;
    private bool movementEnabled = false;
    private bool jumpEnabled = false; // Flag for jump action

    public Vector2Int currentGridPos;  // Current player grid position
    private int gridSize = 5;  // Assuming a 5x5 grid

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
        jumpEnabled = false; // reset jump mopve when normal move is enabled
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

                    // Perform a jump (ignore whether the move is valid for now, jump logic applies)
                    Jump(targetGridPos);  // Perform the jump

                    break;
                }
            }
        }
    }

    void Jump(Vector2Int targetGridPos)
    {
        // Calculate the difference between the player's current grid position and the target grid position
        Vector2Int gridDifference = targetGridPos - currentGridPos;

        // Ensure the player only jumps in the allowed directions (no diagonals)
        if (gridDifference.x == 2 && gridDifference.y == 0)
        {
            // Moving right 2 tiles
            HandleJump(Vector2Int.right);
        }
        else if (gridDifference.x == -2 && gridDifference.y == 0)
        {
            // Moving left 2 tiles
            HandleJump(Vector2Int.left);
        }
        else if (gridDifference.y == 2 && gridDifference.x == 0)
        {
            // Moving up 2 tiles
            HandleJump(Vector2Int.up);
        }
        else if (gridDifference.y == -2 && gridDifference.x == 0)
        {
            // Moving down 2 tiles
            HandleJump(Vector2Int.down);
        }
        else
        {
            Debug.Log("Invalid jump: You can only jump exactly 2 tiles up, down, left, or right.");
        }
    }


    void HandleJump(Vector2Int direction)
    {
        // Calculate the target position 2 tiles ahead in the chosen direction
        Vector2Int targetPos = currentGridPos + direction * 2;

        // Ensure the target position is within the bounds of the 5x5 grid
        targetPos.x = Mathf.Clamp(targetPos.x, 0, gridSize - 1);
        targetPos.y = Mathf.Clamp(targetPos.y, 0, gridSize - 1);

        // Set the player's target position
        SetTargetPosition(gameManager.gridSquares[targetPos].transform.position);
        currentGridPos = targetPos;  // Update player's current grid position

        Debug.Log("Player jumped to: " + currentGridPos);
        jumpEnabled = false;  // Disable jump after it's executed
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
