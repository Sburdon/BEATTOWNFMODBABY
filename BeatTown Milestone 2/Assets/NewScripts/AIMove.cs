using UnityEngine;

public class AIMove : MonoBehaviour
{
    public Vector2Int currentGridPos;   // Current AI grid position
    private GameManager2 gameManager;
    private GameObject player;
    private int gridMin = 1;            // Grid minimum value (inclusive)
    private int gridMax = 5;            // Grid maximum value (inclusive)
    private Vector2 targetPosition;
    public bool isMoving = false;
    public float moveSpeed = 5f;

    private AIAttack aiAttack;          // Reference to AIAttack script

    // Original followPlayer flag
    public bool followPlayer = false;   // If true, AI follows the player; if false, moves randomly

    // Change from boolean to integer counter
    private int turnsToFollowPlayer = 0; // Number of turns to follow the player

   

    void Start()
    {
        gameManager = FindObjectOfType<GameManager2>();
        player = gameManager.player;  // Reference to the player GameObject
        targetPosition = transform.position;
        aiAttack = GetComponent<AIAttack>();

        if (aiAttack == null)
        {
            Debug.LogError("AIAttack script not found on the AI.");
        }
    }

    public void InitializeAIPosition(Vector2Int startGridPos)
    {
        currentGridPos = startGridPos;
        Vector3 startPosition = gameManager.gridSquares[startGridPos].transform.position;
        // Set the AI GameObject's position accordingly
        transform.position = new Vector3(startPosition.x, startPosition.y + GetComponent<SpriteRenderer>().bounds.size.y / 2, transform.position.z);
    }

    // Method to set the number of turns to follow the player
    public void SetFollowPlayerTurns(int turns)
    {
        turnsToFollowPlayer = turns;
    }

    public void MoveAI()
    {
        // Check if we need to follow the player on this turn
        if (turnsToFollowPlayer > 0)
        {
            followPlayer = true;
            turnsToFollowPlayer--; // Decrement the counter after this turn
            Debug.Log("AI is following the player. Turns remaining: " + turnsToFollowPlayer);
        }
        else
        {
            followPlayer = false;
        }

        if (followPlayer)
        {
            MoveTowardsPlayer();
        }
        else
        {
            MoveRandomly();
        }
    }

    void MoveTowardsPlayer()
    {
        // Get the player's current grid position
        Vector2Int playerGridPos = gameManager.playerMoveScript.currentGridPos;

        // Calculate difference in grid positions
        int xDiff = playerGridPos.x - currentGridPos.x;
        int yDiff = playerGridPos.y - currentGridPos.y;

        Vector2Int moveDirection = Vector2Int.zero;

        // Decide on moving along x or y axis
        if (Mathf.Abs(xDiff) >= Mathf.Abs(yDiff))
        {
            // Move along x axis
            if (xDiff > 0)
                moveDirection = Vector2Int.right;
            else if (xDiff < 0)
                moveDirection = Vector2Int.left;
        }
        else
        {
            // Move along y axis
            if (yDiff > 0)
                moveDirection = Vector2Int.up;
            else if (yDiff < 0)
                moveDirection = Vector2Int.down;
        }

        if (moveDirection != Vector2Int.zero)
        {
            // Try to move up to 2 spaces in that direction
            bool moved = TryMove(moveDirection);
            if (!moved)
            {
                Debug.Log("AI could not move towards the player without passing through the player or moving off the grid.");
                // Attempt to attack if in range
                AttemptAttack();
            }
        }
        else
        {
            // AI is on the same grid as player, so don't move
            Debug.Log("AI is on the same grid as player");
            // Attempt to attack
            AttemptAttack();
        }

        // No need to reset followPlayer here since it's controlled by the counter
    }

    void MoveRandomly()
    {
        // Randomly select a direction
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        // Shuffle directions
        for (int i = 0; i < directions.Length; i++)
        {
            int rnd = Random.Range(0, directions.Length);
            Vector2Int temp = directions[rnd];
            directions[rnd] = directions[i];
            directions[i] = temp;
        }

        // Try to move in any valid direction
        foreach (var dir in directions)
        {
            bool moved = TryMove(dir);
            if (moved)
            {
                return;
            }
        }

        // If no valid move found, attempt to attack
        Debug.Log("AI could not move randomly without passing through the player or moving off the grid.");
        AttemptAttack();
    }

    bool TryMove(Vector2Int direction)
    {
        // AI can move up to 2 spaces in a straight line
        for (int distance = 2; distance >= 1; distance--)
        {
            Vector2Int targetGridPos = currentGridPos + direction * distance;

            // Check if target position is within grid bounds
            if (targetGridPos.x < gridMin || targetGridPos.x > gridMax || targetGridPos.y < gridMin || targetGridPos.y > gridMax)
            {
                continue; // Invalid position, try a shorter distance
            }

            // Check if the path passes through the player's position
            bool pathClear = true;
            for (int i = 1; i <= distance; i++)
            {
                Vector2Int checkPos = currentGridPos + direction * i;
                if (checkPos == gameManager.playerMoveScript.currentGridPos)
                {
                    pathClear = false;
                    break;
                }
            }

            if (pathClear)
            {
                // Valid move
                MoveToGridPosition(targetGridPos);
                return true;
            }
        }

        // If no valid move found in this direction
        return false;
    }

    public void MoveToGridPosition(Vector2Int targetGridPos)
    {
        currentGridPos = targetGridPos;
        Vector3 gridWorldPosition = gameManager.gridSquares[targetGridPos].transform.position;
        SetTargetPosition(gridWorldPosition);
        isMoving = true;
    }

    void SetTargetPosition(Vector2 newPosition)
    {
        float aiHeight = GetComponent<SpriteRenderer>().bounds.size.y;
        Vector2 adjustedPosition = new Vector2(newPosition.x, newPosition.y + (aiHeight / 2));

        targetPosition = adjustedPosition;
        isMoving = true;
    }

    void Update()
    {
        if (isMoving)
        {
            MoveAICharacter();
        }
    }

    void MoveAICharacter()
    {
        transform.position = Vector2.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        if (Vector2.Distance(transform.position, targetPosition) < 0.1f)
        {
            transform.position = targetPosition;
            isMoving = false;

            Debug.Log("AI finished moving to position: " + currentGridPos);

            // After AI finishes moving, attempt to attack
            AttemptAttack();
        }
    }

    void AttemptAttack()
    {
        if (aiAttack != null)
        {
            aiAttack.Attack();
        }
        else
        {
            Debug.LogError("AIAttack script is not attached to the AI.");
        }

        // End the AI's turn after attacking
        gameManager.StartPlayerTurn();
    }
}
