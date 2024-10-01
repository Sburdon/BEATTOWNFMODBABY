using UnityEngine;

public class PlayerMove2 : MonoBehaviour
{
    public float moveSpeed = 5f;
    private GameManager2 gameManager;
    private Vector2 targetPosition;
    private bool isMoving = false;
    private bool movementEnabled = false;

    public Vector2Int currentGridPos;  // Current player grid position

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
        if (Input.GetMouseButtonDown(0) && !isMoving && movementEnabled)
        {
            DetectSquareClick();
        }

        if (isMoving)
        {
            MovePlayer();
        }
    }

    public void EnableMovement(bool enable)
    {
        movementEnabled = enable;
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

                    break;
                }
            }
        }
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
