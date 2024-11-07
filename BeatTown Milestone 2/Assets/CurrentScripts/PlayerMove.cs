using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using static StateMachine;

public class PlayerMove : MonoBehaviour
{
    public GameObject PPShighlight;
    public GameObject moveMentHighlight;
    public Tilemap tilemap; // Reference to the Tilemap
    public float moveSpeed = 1f; // Speed of movement
    public int maxMoves = 2; // Maximum moves allowed in a turn
    public int remainingMoves; // Count of remaining moves in the current turn
    private bool canMove = false; // Flag to control movement
    private ActionType currentAction; // Current action type for the player
    private Swing swingScript; // Reference to the Swing script
    private StateMachine stateMachine;
    public Vector3Int CurrentTilePosition { get; private set; } // Current tile position in grid coordinates
    private Coroutine currentMoveCoroutine; // Store reference to the current move coroutine
    private PlayerFatigue playerFatigue; // Reference to the PlayerFatigue script
    public int moveFatigueCost = 1; // Fatigue cost for movement
    public All_SFX All_SFX; // Reference to FMOD Script
    public GameObject SwingHighlight;
    public GameObject RealMoveHighlight;

    private bool hasFatigueBeenDeductedForMove = false; // Flag to ensure fatigue is only deducted once per move action

    public GameObject noMoveImage;    // Image for no moves left
    public GameObject oneMoveImage;   // Image for one move left
    public GameObject twoMoveImage;   // Image for both moves left

    private void Awake()
    {
        CurrentTilePosition = tilemap.WorldToCell(transform.position);
        UpdatePlayerPosition();
    }
    void Start()
    {
        // Initialize the current tile position based on the player's starting position
        CurrentTilePosition = tilemap.WorldToCell(transform.position);
        UpdatePlayerPosition();
        remainingMoves = 0; // Initialize remaining moves
        UpdateMoveImages();
        swingScript = GetComponent<Swing>(); // Get reference to Swing script
        playerFatigue = GetComponent<PlayerFatigue>(); // Get reference to PlayerFatigue script
        stateMachine = GetComponent<StateMachine>();

        // Register the player with the OccupiedTilesManager
        OccupiedTilesManager.Instance.RegisterPlayer(this);

        All_SFX.PlayFishBattle();
        All_SFX.UpdateCudaCount();
    }

    void Update()
    {
        CurrentTilePosition = tilemap.WorldToCell(transform.position);

        if (canMove && Input.GetMouseButtonDown(0)) // Left mouse button
        {
            Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int clickedTilePosition = tilemap.WorldToCell(mouseWorldPosition);

            // Check if the clicked tile is within the bounds of the tilemap
            if (!IsWithinTilemapBounds(clickedTilePosition) || !tilemap.HasTile(clickedTilePosition))
            {
                Debug.Log("Clicked tile is outside of the tilemap bounds or has no valid tile.");
                return; // Exit if clicked tile is out of bounds or has no tile
            }

            int deltaX = Mathf.Abs(clickedTilePosition.x - CurrentTilePosition.x);
            int deltaY = Mathf.Abs(clickedTilePosition.y - CurrentTilePosition.y);
            bool isDiagonalMove = (deltaX == 1 && deltaY == 1);

            // Validate move range
            if ((deltaX + deltaY <= remainingMoves && (deltaX == 0 || deltaY == 0)) || (isDiagonalMove && remainingMoves >= 2))
            {
                bool moveAllowed = false;

                // Check if the move is valid
                if (isDiagonalMove)
                {
                    Vector3Int? clearIntermediateTile = CheckDiagonalPaths(CurrentTilePosition, clickedTilePosition);
                    if (clearIntermediateTile.HasValue)
                    {
                        moveAllowed = true;
                        Vector3Int intermediateTile = clearIntermediateTile.Value;
                        currentMoveCoroutine = StartCoroutine(MoveAlongPath(intermediateTile, clickedTilePosition));
                    }
                }
                else if (IsPathClear(CurrentTilePosition, clickedTilePosition))
                {
                    moveAllowed = true;
                    currentMoveCoroutine = StartCoroutine(MoveToTile(clickedTilePosition));
                }

                if (moveAllowed)
                {
                    // Deduct fatigue if the move is allowed
                    if (!hasFatigueBeenDeductedForMove && playerFatigue.CanPerformAction(moveFatigueCost))
                    {
                        playerFatigue.UseFatigue(moveFatigueCost);
                        hasFatigueBeenDeductedForMove = true;
                    }
                }
                else
                {
                    Debug.Log("Move is blocked; no movement executed.");
                }
            }
            else
            {
                Debug.Log("Clicked tile is out of range or no moves remaining.");
            }
        }
    }

    // Helper method to check if a tile position is within the tilemap bounds
    private bool IsWithinTilemapBounds(Vector3Int position)
    {
        return tilemap.cellBounds.Contains(position);
    }


    private void UpdateMoveImages()
    {
        noMoveImage.SetActive(remainingMoves == 0);
        oneMoveImage.SetActive(remainingMoves == 1);
        twoMoveImage.SetActive(remainingMoves == 2);
    }

    // Helper method to check both diagonal paths and ensure one is clear
    private Vector3Int? CheckDiagonalPaths(Vector3Int startTile, Vector3Int endTile)
    {
        Vector3Int intermediateTile1 = new Vector3Int(endTile.x, startTile.y, 0); // Horizontal first
        Vector3Int intermediateTile2 = new Vector3Int(startTile.x, endTile.y, 0); // Vertical first

        // Check each path individually; return the intermediate tile if one is clear
        if (IsPathClear(startTile, intermediateTile1) && !IsTileOccupied(endTile))
        {
            return intermediateTile1;
        }
        else if (IsPathClear(startTile, intermediateTile2) && !IsTileOccupied(endTile))
        {
            return intermediateTile2;
        }

        // If both paths are blocked, return null
        return null;
    }

    // Helper method to check if all tiles along the path are unoccupied
    private bool IsPathClear(Vector3Int startTile, Vector3Int endTile)
    {
        // Directly check if moving only one tile
        if (Mathf.Abs(startTile.x - endTile.x) + Mathf.Abs(startTile.y - endTile.y) == 1)
        {
            return !IsTileOccupied(endTile);
        }

        // For diagonal moves, check both intermediate tiles if moving two tiles
        if (Mathf.Abs(startTile.x - endTile.x) + Mathf.Abs(startTile.y - endTile.y) == 2)
        {
            // Determine the intermediate tile based on the direction of movement
            Vector3Int intermediateTile = new Vector3Int((startTile.x + endTile.x) / 2, (startTile.y + endTile.y) / 2, 0);

            // Check if the path is clear by checking the intermediate and target tiles
            return !IsTileOccupied(intermediateTile) && !IsTileOccupied(endTile);
        }

        // If more than two tiles are moved, get all intermediate tiles
        List<Vector3Int> pathTiles = GetTilesInPath(startTile, endTile);
        foreach (Vector3Int tile in pathTiles)
        {
            if (IsTileOccupied(tile))
            {
                return false; // Path is blocked if any tile is occupied
            }
        }
        return true;
    }

    // Gets the list of intermediate tiles between the start and end tiles (excluding start and end)
    private List<Vector3Int> GetTilesInPath(Vector3Int startTile, Vector3Int endTile)
    {
        List<Vector3Int> pathTiles = new List<Vector3Int>();

        if (startTile.x == endTile.x)
        {
            int yMin = Mathf.Min(startTile.y, endTile.y) + 1;
            int yMax = Mathf.Max(startTile.y, endTile.y) - 1;
            for (int y = yMin; y <= yMax; y++)
            {
                pathTiles.Add(new Vector3Int(startTile.x, y, 0));
            }
        }
        else if (startTile.y == endTile.y)
        {
            int xMin = Mathf.Min(startTile.x, endTile.x) + 1;
            int xMax = Mathf.Max(startTile.x, endTile.x) - 1;
            for (int x = xMin; x <= xMax; x++)
            {
                pathTiles.Add(new Vector3Int(x, startTile.y, 0));
            }
        }

        return pathTiles;
    }

    private IEnumerator MoveAlongPath(Vector3Int intermediateTilePosition, Vector3Int targetTilePosition)
    {
        if (IsTileOccupied(intermediateTilePosition))
        {
            Debug.Log("Path blocked by an enemy at " + intermediateTilePosition);
            yield break; // Stop movement if path is blocked
        }

        yield return MoveToTile(intermediateTilePosition);

        if (IsTileOccupied(targetTilePosition))
        {
            Debug.Log("Path blocked by an enemy at " + targetTilePosition);
            yield break;
        }

        yield return MoveToTile(targetTilePosition);
    }

    public void OnMoveButtonPressed()
    {
        Debug.Log("Move button pressed.");
        SwingHighlight.SetActive(false);
        RealMoveHighlight.SetActive(true);
        PPShighlight.SetActive(false);
        moveMentHighlight.SetActive(false);

        if (swingScript != null && swingScript.IsSwinging())
        {
            swingScript.CancelSwing();
        }

        // Check if the player has moves left
        if (remainingMoves > 0)
        {
            canMove = true;
            Debug.Log("Move button pressed. You have " + remainingMoves + " moves available.");
            currentAction = ActionType.Move;
        }
        else if (remainingMoves <= 0 && !hasFatigueBeenDeductedForMove)
        {
            // Deduct fatigue if player wants to gain more moves
            if (playerFatigue.CanPerformAction(moveFatigueCost))
            {
                RealMoveHighlight.SetActive(true);
                playerFatigue.UseFatigue(moveFatigueCost);
                remainingMoves = maxMoves;
                UpdateMoveImages();
                hasFatigueBeenDeductedForMove = true;

                Debug.Log("Fatigue used to gain more moves. You now have " + remainingMoves + " moves.");
                canMove = true;
            }
            else
            {
                Debug.Log("Not enough fatigue to gain more moves.");
            }
        }
    }

    public void CancelMove()
    {
        if (currentMoveCoroutine != null)
        {
            StopCoroutine(currentMoveCoroutine);
            currentMoveCoroutine = null;
        }
        canMove = false;
        Debug.Log("Move action canceled, but further movement is allowed if remaining moves exist.");
    }

    private IEnumerator MoveToTile(Vector3Int targetTilePosition)
    {
        OccupiedTilesManager.Instance.RemoveOccupiedPosition(CurrentTilePosition);
        Vector3 targetPosition = tilemap.GetCellCenterWorld(targetTilePosition);
        float elapsedTime = 0f;
        if (IsTileOccupied(targetTilePosition))
        {
            Debug.Log("Target tile is occupied by an enemy.");
            yield break; // Stop movement if the target tile is occupied
        }
        if (targetTilePosition.x < CurrentTilePosition.x)
        {
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
        else if (targetTilePosition.x > CurrentTilePosition.x)
        {
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }

        Vector3 startPosition = transform.position;
        stateMachine.ChangeState(WrestlerState.Move);

        while (elapsedTime < 1f)
        {
            RealMoveHighlight.SetActive(false);
            transform.position = Vector3.Lerp(startPosition, targetPosition, (elapsedTime / 1f));
            elapsedTime += Time.deltaTime * moveSpeed;
            yield return null;
        }

        transform.position = targetPosition;
        All_SFX.PlayWalkingSound();

        CurrentTilePosition = targetTilePosition;
        OccupiedTilesManager.Instance.AddOccupiedPosition(CurrentTilePosition);

        int deltaX = Mathf.Abs(targetTilePosition.x - tilemap.WorldToCell(startPosition).x);
        int deltaY = Mathf.Abs(targetTilePosition.y - tilemap.WorldToCell(startPosition).y);

        if (deltaX + deltaY == 1)
        {
            remainingMoves -= 1;
        }
        else if (deltaX + deltaY == 2)
        {
            remainingMoves -= 2;
        }
        UpdateMoveImages();
        if (remainingMoves <= 0)
        {
            RealMoveHighlight.SetActive(false);
            canMove = false;
            hasFatigueBeenDeductedForMove = false;
            Debug.Log("Movement complete. No moves remaining.");
        }
        else
        {
            RealMoveHighlight.SetActive(true);
            Debug.Log($"Remaining moves: {remainingMoves}");
        }
    }

    public void RefreshSpaceCount()
    {
        remainingMoves = maxMoves;
        canMove = true;
        hasFatigueBeenDeductedForMove = false;
        UpdateMoveImages();
        Debug.Log("Movement reset for the next turn.");
    }

    void UpdatePlayerPosition()
    {
        transform.position = tilemap.GetCellCenterWorld(CurrentTilePosition);
        OccupiedTilesManager.Instance.AddOccupiedPosition(CurrentTilePosition);
    }

    public bool IsTileOccupied(Vector3Int position)
    {
        // Check for player, enemies, or any object on the tile.
        return OccupiedTilesManager.Instance.IsTileOccupied(position);
    }

    public ActionType CurrentAction
    {
        get { return currentAction; }
        set
        {
            currentAction = value;
            Debug.Log("Current Action set to: " + currentAction);
        }
    }
}