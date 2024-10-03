using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

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
    public Button pushButton;     // Button for pushing
    public Button endTurnButton;  // Button to end the turn

    private bool isPlayerTurn = true;
    private bool canMove = true;
    private bool canPush = true;

    public GameObject player;  // Reference to the player GameObject
    public PlayerMove2 playerMoveScript;
    public PlayerFatigue playerFatigueScript;
    public Push pushScript;

    // AI variables
    public GameObject aiPrefab;  // AI prefab for respawning
    public List<GameObject> activeAIList = new List<GameObject>(); // Track active AI

    // Trap-related variables
    private int trapKills = 0;
    public TextMeshProUGUI trapKillCountText; // TextMeshPro for trap kills

    // UI variables
    public TextMeshProUGUI playerHealthText;
    public TextMeshProUGUI enemyHealthText;
    public TextMeshProUGUI playerFatigueText;

    // Boss-related variables
    public GameObject bossPrefab; // Assign boss prefab in the Inspector
    public GameObject bossInstance; // Store the spawned boss instance
    public Vector2Int bossSpawnGridPos = new Vector2Int(3, 3); // Grid position where the boss will spawn
    private bool bossSpawned = false; // Track if the boss has already spawned
    private bool isBossTurn = false;  // Track if it's the boss's turn

    void Start()
    {
        gridSquares = new Dictionary<Vector2Int, GameObject>();
        InitializeGrid();

        playerMoveScript = FindObjectOfType<PlayerMove2>();
        playerFatigueScript = FindObjectOfType<PlayerFatigue>();
        pushScript = FindObjectOfType<Push>();

        moveButton.onClick.AddListener(EnableMove);
        pushButton.onClick.AddListener(EnablePush);
        endTurnButton.onClick.AddListener(EndTurn);

        StartPlayerTurn();
        UpdateUI();
    }

    // Initialize the grid and populate the dictionary
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
        if (isPlayerTurn && canMove)
        {
            playerMoveScript.EnableMovement(true);  // Enable movement in the PlayerMove2 script
            canMove = false;  // Disable further movement until reset
        }
    }

    // Called when the player clicks the push button
    void EnablePush()
    {
        if (isPlayerTurn && canPush)
        {
            Debug.Log("Push mode activated. Select an enemy to push.");
            canPush = false;

            if (pushScript != null)
            {
                pushScript.EnterPushMode();  // Enter push mode and await player click
            }
            else
            {
                Debug.LogError("Push script not found!");
            }
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

    // Starts the AI turn
    public void StartAITurn()
    {
        Debug.Log("AI turn started.");
        isPlayerTurn = false;

        foreach (var ai in activeAIList)
        {
            AIMove aiMove = ai.GetComponent<AIMove>();
            if (aiMove != null)
            {
                aiMove.MoveAI();
            }
        }

        // After AI turns, handle boss movement (if the boss is spawned)
        if (bossSpawned && bossInstance != null)
        {
            isBossTurn = true;  // Now it's the boss's turn
            StartBossTurn();    // Call the method to move the boss
        }
        else
        {
            StartPlayerTurn();  // After AI and boss turns, return control to the player
        }
    }

    // Starts the boss's turn
    private void StartBossTurn()
    {
        BossMovement bossMove = bossInstance.GetComponent<BossMovement>();
        if (bossMove != null)
        {
            bossMove.MoveTowardsTarget();  // Move the boss only during its turn
        }

        isBossTurn = false;  // End boss's turn
        StartPlayerTurn();   // After boss turn, go back to player
    }

    // Start the player's turn
    public void StartPlayerTurn()
    {
        Debug.Log("Player's turn started.");
        isPlayerTurn = true;

        canMove = true;
        canPush = true;

        playerFatigueScript.RecoverFatigue(playerFatigueScript.maxFatigue);

        UpdateUI();
    }

    // Respawn AI at a random grid position after it dies
    public void RespawnAI(GameObject ai)
    {
        Vector2Int randomPos = GetRandomGridPosition();
        ai.GetComponent<AIMove>().InitializeAIPosition(randomPos);

        SpriteRenderer spriteRenderer = ai.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }

        Collider2D collider = ai.GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = true;
        }

        ai.SetActive(true);

        EnemyHealth enemyHealth = ai.GetComponent<EnemyHealth>();
        if (enemyHealth != null)
        {
            enemyHealth.ResetHealth();
        }

        AIMove aiMove = ai.GetComponent<AIMove>();
        if (aiMove != null)
        {
            aiMove.isMoving = false;
            aiMove.MoveAI();
        }

        Debug.Log("AI respawned at position: " + randomPos);
    }

    // Helper method to get a random grid position
    private Vector2Int GetRandomGridPosition()
    {
        int x = Random.Range(1, 5);
        int y = Random.Range(1, 5);
        return new Vector2Int(x, y);
    }

    // Track trap kills (called by Trap script when enemy dies)
    public void TrapKill()
    {
        trapKills += 1;
        UpdateTrapKillUI();

        if (trapKills >= 2 && !bossSpawned)
        {
            SpawnBoss();
        }
    }

    // Spawn the boss at the grid coordinates
    private void SpawnBoss()
    {
        if (bossPrefab != null && gridSquares.ContainsKey(bossSpawnGridPos))
        {
            Vector3 bossWorldPos = gridSquares[bossSpawnGridPos].transform.position;
            bossInstance = Instantiate(bossPrefab, bossWorldPos, Quaternion.identity);  // Store the boss instance
            bossSpawned = true;
            Debug.Log("Boss spawned at: " + bossSpawnGridPos);
        }
        else
        {
            Debug.LogError("Boss prefab or grid position for spawn is missing.");
        }
    }

    // Update the trap kill count in the UI
    private void UpdateTrapKillUI()
    {
        trapKillCountText.text = "Trap Kills: " + trapKills.ToString();
    }

    // Update the player's health, enemy health, fatigue, and trap kills in the UI
    private void UpdateUI()
    {
        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealthText.text = "Player Health: " + playerHealth.currentHealth + "/" + playerHealth.maxHealth;
        }

        if (activeAIList.Count > 0)
        {
            EnemyHealth enemyHealth = activeAIList[0].GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealthText.text = "Enemy Health: " + enemyHealth.currentHealth + "/" + enemyHealth.maxHealth;
            }
        }

        playerFatigueText.text = "Fatigue: " + playerFatigueScript.currentFatigue + "/" + playerFatigueScript.maxFatigue;
    }

    // Check if the player can move within 2 squares and not diagonally
    public bool IsValidMove(Vector2Int currentPos, Vector2Int targetPos)
    {
        int xDistance = Mathf.Abs(currentPos.x - targetPos.x);
        int yDistance = Mathf.Abs(currentPos.y - targetPos.y);

        Debug.Log("xDistance: " + xDistance + ", yDistance: " + yDistance);

        if ((xDistance == 0 && yDistance > 0 && yDistance <= 2) ||
            (yDistance == 0 && xDistance > 0 && xDistance <= 2))
        {
            return true;
        }
        return false;
    }
}
