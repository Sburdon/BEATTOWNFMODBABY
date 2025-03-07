using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using static StateMachine;

public class Jump : MonoBehaviour
{
    public TempTurnBase tempTurnBase;
    public Tilemap tilemap;
    public float jumpHeight = 5f; // Adjust this value as needed
    public float jumpSpeed = 2f;
    private Vector3Int targetTilePosition;
    private GameObject targetToJumpOn;
    private bool isJumpMode = false;
    private bool isJumping = false;
    private PlayerFatigue playerFatigue;
    public int jumpFatigueCost = 2;
    private StateMachine stateMachine;
    public All_SFX All_SFX;

    private OccupiedTilesManager occupiedTilesManager;
    private PlayerMove playerMove; // Reference to the PlayerMove script
    Vector3Int removePlayerOldPos;
    public GameObject SwingHighlight;
    public GameObject PPShighlight;
    public GameObject moveMentHighlight;
    public GameObject jumpHighlight;
    public GameObject RealMoveHighlight;

    private void Start()
    {
        if (SceneManager.GetActiveScene().buildIndex == 1) // Change to your scene index
        {
            this.enabled = false;
        }
    }
    void Awake()
    {
        occupiedTilesManager = GetComponent<OccupiedTilesManager>();
        playerMove = GetComponent<PlayerMove>();
        playerFatigue = GetComponent<PlayerFatigue>();
        stateMachine = GetComponent<StateMachine>();
        tempTurnBase = FindObjectOfType<TempTurnBase>();
    }

    public void OnJumpButtonPressed()
    {
        

        if (playerFatigue.CanPerformAction(jumpFatigueCost) && !playerFatigue.lockedMovement)
        {
            RealMoveHighlight.SetActive(false);
            SwingHighlight.SetActive(false);
            PPShighlight.SetActive(false);
            moveMentHighlight.SetActive(false);
            jumpHighlight.SetActive(true);
            removePlayerOldPos = playerMove.CurrentTilePosition;
            tempTurnBase.ResetAllColliders();

            if (isJumping)
            {
                Debug.Log("Already jumping.");
                return;
            }

            if (isJumpMode)
            {
                Debug.Log("Jump mode already active.");
                return;
            }

            isJumpMode = true;
            Debug.Log("Jump mode activated. Click on a valid tile to jump.");
        }
        else
        {
            Debug.Log("Not enough fatigue to jump.");
        }
    }
    private bool IsTilePresent(Vector3Int tilePosition)
    {
        return tilemap.HasTile(tilePosition);
    }

    void Update()
    {
        if (isJumpMode && !isJumping)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector3Int clickedTilePosition = tilemap.WorldToCell(mouseWorldPosition);
                Vector3Int playerTilePosition = tilemap.WorldToCell(transform.position);
                int deltaX = Mathf.Abs(clickedTilePosition.x - playerTilePosition.x);
                int deltaY = Mathf.Abs(clickedTilePosition.y - playerTilePosition.y);

                bool isDiagonalMove = (deltaX == 1 && deltaY == 1);

                // Only allow jump if it is a valid straight jump (2 tiles) or a valid diagonal (1 tile)
                if ((IsValidStraightJump(playerTilePosition, clickedTilePosition) || isDiagonalMove) && IsTilePresent(clickedTilePosition))
                {
                    targetTilePosition = clickedTilePosition;
                    playerFatigue.UseFatigue(jumpFatigueCost);
                    StartCoroutine(JumpToTarget(targetTilePosition));

                    isJumpMode = false;
                    jumpHighlight.SetActive(false);
                }
                else
                {
                    Debug.Log("Invalid jump target.");
                }
            }
            else if (Input.GetMouseButtonDown(1)) // Right-click to cancel
            {
                Debug.Log("Jump action canceled.");
                isJumpMode = false;

            }
        }
    }

    private bool IsValidStraightJump(Vector3Int playerTilePosition, Vector3Int targetTilePosition)
    {
        // Check if the jump is exactly 2 tiles in a straight line (no diagonal)
        return (playerTilePosition.x == targetTilePosition.x && Mathf.Abs(playerTilePosition.y - targetTilePosition.y) == 2) ||
               (playerTilePosition.y == targetTilePosition.y && Mathf.Abs(playerTilePosition.x - targetTilePosition.x) == 2);
    }
    private bool IsWithinTilemapBounds(Vector3Int position)
    {
        return tilemap.cellBounds.Contains(position);
    }
    private IEnumerator JumpToTarget(Vector3Int targetTilePosition)
    {
        if (!IsWithinTilemapBounds(targetTilePosition))
        {
            yield break;
        }

        isJumping = true;

        Vector3 startPos = transform.position;
        Vector3 endPos = tilemap.GetCellCenterWorld(targetTilePosition);

        float elapsedTime = 0f;
        float duration = 1f / jumpSpeed;

        // Move player to target position
        while (elapsedTime < duration)
        {
            transform.position = Vector3.Lerp(startPos, endPos, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = endPos;

        // Check if there's an enemy or other objects on the target tile
        bool bounced = false;

        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 0.5f);
        foreach (var collider in colliders)
        {
            if (collider.CompareTag("Electrician") || collider.CompareTag("Goon"))
            {
                Debug.Log("Landed on an enemy or Barra, no damage dealt.");

                Vector3Int bounceTile = GetRandomAdjacentTile(transform.position);

                // Ensure the bounce tile is within bounds and valid before moving
                int maxAttempts = 5;
                int attempts = 0;
                while ((!IsValidTile(bounceTile) || !IsWithinTilemapBounds(bounceTile)) && attempts < maxAttempts)
                {
                    bounceTile = GetRandomAdjacentTile(transform.position);
                    attempts++;
                }

                // Final safety check
                if (!IsWithinTilemapBounds(bounceTile))
                {
                    Debug.Log("Bounce location is out of bounds, staying in current position.");
                    transform.position = startPos; // Stay in the original position instead of bouncing off the map
                }
                else
                {
                    transform.position = tilemap.GetCellCenterWorld(bounceTile);
                    Debug.Log("Bounced to a valid adjacent tile.");
                }

                bounced = true;
                break;
            }
        }

        jumpHighlight.SetActive(false);
        OccupiedTilesManager.Instance.AddOccupiedPosition(tilemap.WorldToCell(playerMove.CurrentTilePosition));
        OccupiedTilesManager.Instance.RemoveOccupiedPosition(tilemap.WorldToCell(removePlayerOldPos));

        // If no bounce was needed (no enemy or Barra), just finish the jump normally
        if (!bounced)
        {
            Debug.Log("Jump action completed.");
        }

        isJumping = false;
    }


    // Check if the tile is valid for landing or bouncing (no enemy or other obstacle)
    private bool IsValidTile(Vector3Int tilePosition)
    {
        // Check if the tile is not occupied by an enemy or Barra and is valid
        Collider2D[] colliders = Physics2D.OverlapCircleAll(tilemap.GetCellCenterWorld(tilePosition), 0.5f);
        foreach (var collider in colliders)
        {
            if (collider.CompareTag("Electrician") || collider.CompareTag("Goon"))
            {
                return false; // Tile is not valid if it contains an enemy or Barra
            }
        }

        return true; // Tile is valid if it doesn't contain any enemy or Barra
    }

    // Get a random adjacent tile to bounce to
    private Vector3Int GetRandomAdjacentTile(Vector3 currentPos)
    {
        Vector3Int currentTile = tilemap.WorldToCell(currentPos);

        // Define the adjacent tiles (up, down, left, right)
        Vector3Int[] adjacentTiles = new Vector3Int[]
        {
        currentTile + Vector3Int.up,
        currentTile + Vector3Int.down,
        currentTile + Vector3Int.left,
        currentTile + Vector3Int.right,
        };

        // Shuffle the array to ensure randomness
        System.Random rng = new System.Random();
        adjacentTiles = adjacentTiles.OrderBy(tile => rng.Next()).ToArray();

        // Try to find a valid tile that is within bounds and unoccupied
        foreach (var tile in adjacentTiles)
        {
            if (IsValidTile(tile) && !IsCollidingWithWall(tile))
            {
                OccupiedTilesManager.Instance.AddOccupiedPosition(tile);
                return tile;
            }
        }

        // If no valid tile is found, return the player's current tile (so they don't fall off)
        return currentTile;
    }

    // Check if the tile is colliding with a wall
    private bool IsCollidingWithWall(Vector3Int tilePosition)
    {
        Vector3 worldPos = tilemap.GetCellCenterWorld(tilePosition);
        Collider2D[] colliders = Physics2D.OverlapCircleAll(worldPos, 0.1f);

        foreach (var collider in colliders)
        {
            if (collider.CompareTag("Wall")) // Ensure your wall colliders are tagged correctly
            {
                Debug.Log("DAMN WALL GET OUT OF MY WAY!");
                return true; // Tile is not valid if it's a wall
            }
        }

        return false; // Tile is valid if no walls were found
    }
    public void CancelJump()
    {
        isJumpMode = false;
        isJumping = false;
        Debug.Log("Jump action canceled.");
    }
}