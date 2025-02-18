using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TempTurnBase : MonoBehaviour
{
    [Header("UI Components")]
    public GameObject turnOrderPanel; // The panel holding turn order images

    public Sprite playerBlueSprite;
    public Sprite playerRedSprite;
    public Sprite enemyBlueSprite;
    public Sprite enemyRedSprite;
    public Sprite barraBlueSprite;
    public Sprite barraRedSprite;

    [Header("Goon Sprites (Optional)")]
    public Sprite goonBlueSprite;
    public Sprite goonRedSprite;

    [Header("Electrician Sprites (Optional)")]
    public Sprite electricianBlueSprite;
    public Sprite electricianRedSprite;

    public GameObject turnOrderPrefab; // Prefab for each turn order image

    private List<Image> turnOrderImages = new List<Image>();

    [Header("AI Units")]
    public GameObject PPShighlight;
    public GameObject moveMentHighlight;
    public List<AIMove> aiUnits = new List<AIMove>();

    [Header("Goon Units")]
    public List<GoonMove> goonUnits = new List<GoonMove>();

    [Header("Electrician Units")]
    public List<ElectricianMove> electricianUnits = new List<ElectricianMove>();

    [Header("Barra Units")]
    public List<BarraMove> barraUnits = new List<BarraMove>();

    [Header("Player Components")]
    public PlayerMove playerMove;
    public PlayerFatigue playerFatigue;
    private RespawnManager respawnManager;
    private Hook hook;

    public bool isPlayerTurn = true;
    private bool isProcessingTurn = false;

    public GameObject RealMoveHighlight;

    private bool spawnBarra = true;
    private bool spawnBarra1 = true;

    private List<object> turnUnits = new List<object>();
    private int currentTurnIndex = 0;

    public All_SFX All_SFX;

    void Start()
    {
        if (playerMove == null)
            playerMove = FindObjectOfType<PlayerMove>();

        if (playerFatigue == null)
            playerFatigue = FindObjectOfType<PlayerFatigue>();

        hook = Hook.Instance;
        respawnManager = RespawnManager.Instance;

        if (respawnManager == null)
        {
            Debug.LogError("RespawnManager instance not found in the scene.");
        }

        ResetAllColliders();
        InitializeTurnOrder();
        UpdateTurnOrderUI();
    }

    // Build the turn order: Player -> AI -> Goon -> Electrician -> Barra (in that sequence)
    private void InitializeTurnOrder()
    {
        turnUnits.Clear();

        // Player
        if (playerMove != null)
            turnUnits.Add(playerMove);

        // AI
        foreach (var ai in aiUnits)
            turnUnits.Add(ai);

        // Goon
        foreach (var goon in goonUnits)
            turnUnits.Add(goon);

        // Electrician
        foreach (var electrician in electricianUnits)
            turnUnits.Add(electrician);

        // Barra
        foreach (var barra in barraUnits)
            turnUnits.Add(barra);
    }

    // Rotate the turn to the next unit
    private void RotateTurnOrder()
    {
        if (turnUnits.Count == 0) return;

        var unit = turnUnits[currentTurnIndex];
        turnUnits.RemoveAt(currentTurnIndex);
        turnUnits.Add(unit);

        // Wrap the index around
        currentTurnIndex = currentTurnIndex % turnUnits.Count;

        // If you had LeanTween calls, they’d go here. But removed them:
        // e.g. AnimateTurnOrderRotation();
        UpdateTurnOrderUI();
    }

    private void updateSpacing()
    {
        int totalImages = turnOrderPanel.transform.childCount;
        HorizontalLayoutGroup layoutGroup = turnOrderPanel.GetComponent<HorizontalLayoutGroup>();

        if (totalImages == 1)
        {
            return;
        }
        else if (totalImages == 2)
        {
            layoutGroup.spacing = -442;
        }
        else if (totalImages == 3)
        {
            layoutGroup.spacing = -290;
        }
        else if (totalImages == 4)
        {
            layoutGroup.spacing = -150;
        }
        else if (totalImages == 5)
        {
            layoutGroup.spacing = -3;
        }
    }

    public void StartTurn()
    {
        if (turnUnits.Count == 0) return;

        var currentUnit = turnUnits[currentTurnIndex];

        // Player
        if (currentUnit is PlayerMove)
        {
            respawnManager.MaintainEnemyCount();

            if ((hook.hookKillCount == 2 || hook.hookKillCount == 3) && spawnBarra)
            {
                respawnManager.SpawnBarra();
                spawnBarra = false;
            }
            if ((hook.hookKillCount == 4 || hook.hookKillCount == 5) && spawnBarra1)
            {
                respawnManager.SpawnBarra();
                spawnBarra1 = false;
            }
            StartPlayerTurn();
        }
        // AI Move
        else if (currentUnit is AIMove ai)
        {
            StartCoroutine(ProcessAnyUnitTurn(ai));
        }
        // Goon
        else if (currentUnit is GoonMove goon)
        {
            StartCoroutine(ProcessAnyUnitTurn(goon));
        }
        // Electrician
        else if (currentUnit is ElectricianMove electrician)
        {
            StartCoroutine(ProcessAnyUnitTurn(electrician));
        }
        // Barra
        else if (currentUnit is BarraMove barra)
        {
            StartCoroutine(ProcessBarraTurn(barra));
        }
    }

    void Update()
    {
        updateSpacing();

        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetAllColliders();
        }
    }

    public void ResetAllColliders()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject[] barras = GameObject.FindGameObjectsWithTag("Barra");

        foreach (GameObject enemy in enemies)
            ResetCollider(enemy);

        foreach (GameObject barra in barras)
            ResetCollider(barra);
    }

    private void ResetCollider(GameObject unit)
    {
        Collider2D collider = unit.GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = false;
            collider.enabled = true;
        }
    }

    // Remove an AI unit from the rotation
    public void RemoveAIUnit(AIMove aiMove)
    {
        if (aiMove != null && aiUnits.Contains(aiMove))
        {
            aiUnits.Remove(aiMove);
            turnUnits.Remove(aiMove);
            if (currentTurnIndex >= turnUnits.Count)
            {
                currentTurnIndex = turnUnits.Count - 1;
            }
            UpdateTurnOrderUI();
            Debug.Log($"Removed AI {aiMove.gameObject.name}.");
        }
    }

    // Remove a Barra unit from the rotation
    public void RemoveBarraUnit(BarraMove barraMove)
    {
        if (barraMove != null && barraUnits.Contains(barraMove))
        {
            barraUnits.Remove(barraMove);
            turnUnits.Remove(barraMove);
            if (currentTurnIndex >= turnUnits.Count)
            {
                currentTurnIndex = turnUnits.Count - 1;
            }
            UpdateTurnOrderUI();
            Debug.Log($"Removed Barra {barraMove.gameObject.name}.");
        }
    }

    // Add an AI unit
    public void AddAIUnit(AIMove aiMove)
    {
        if (aiMove != null && !aiUnits.Contains(aiMove))
        {
            aiUnits.Add(aiMove);
            turnUnits.Insert(turnUnits.Count, aiMove);
            UpdateTurnOrderUI();
        }
    }

    // Add a Barra unit
    public void AddBarraUnit(BarraMove barraMove)
    {
        if (barraMove != null && !barraUnits.Contains(barraMove))
        {
            barraUnits.Add(barraMove);
            turnUnits.Add(barraMove);
            UpdateTurnOrderUI();
        }
    }

    // -------------------- GOON --------------------
    public void AddGoonUnit(GoonMove goon)
    {
        if (goon != null && !goonUnits.Contains(goon))
        {
            goonUnits.Add(goon);
            InitializeTurnOrder();
            UpdateTurnOrderUI();
            Debug.Log($"Added Goon {goon.gameObject.name}.");
        }
    }
    public void RemoveGoonUnit(GoonMove goon)
    {
        if (goon != null && goonUnits.Contains(goon))
        {
            goonUnits.Remove(goon);
            InitializeTurnOrder();
            UpdateTurnOrderUI();
            Debug.Log($"Removed Goon {goon.gameObject.name}.");
        }
    }

    // ----------------- ELECTRICIAN ----------------
    public void AddElectricianUnit(ElectricianMove electrician)
    {
        if (electrician != null && !electricianUnits.Contains(electrician))
        {
            electricianUnits.Add(electrician);
            InitializeTurnOrder();
            UpdateTurnOrderUI();
            Debug.Log($"Added Electrician {electrician.gameObject.name}.");
        }
    }
    public void RemoveElectricianUnit(ElectricianMove electrician)
    {
        if (electrician != null && electricianUnits.Contains(electrician))
        {
            electricianUnits.Remove(electrician);
            InitializeTurnOrder();
            UpdateTurnOrderUI();
            Debug.Log($"Removed Electrician {electrician.gameObject.name}.");
        }
    }

    // Start the player's turn
    public void StartPlayerTurn()
    {
        isPlayerTurn = true;
        ResetAllColliders();
        if (playerFatigue != null)
            playerFatigue.RecoverFatigue();

        if (playerMove != null)
            playerMove.RefreshSpaceCount();

        Debug.Log("Player turn started.");
        currentTurnIndex = 0; 
        UpdateTurnOrderUI();
    }

    // End the player's turn
    public void EndPlayerTurn()
    {
        if (!isPlayerTurn || isProcessingTurn) return;

        PPShighlight.SetActive(false);
        moveMentHighlight.SetActive(false);
        RealMoveHighlight.SetActive(false);

        isPlayerTurn = false;
        RotateTurnOrder();
        StartTurn();
    }

    /// <summary>
    /// Handle any AI-like unit’s turn (AI, Goon, Electrician) by looking for AIFatigue.
    /// </summary>
    private IEnumerator ProcessAnyUnitTurn(MonoBehaviour unit)
    {
        isProcessingTurn = true;

        var fatigue = unit.GetComponent<AIFatigue>();
        if (fatigue != null)
        {
            Debug.Log($"Unit {unit.name} is taking its turn with AIFatigue.");
            yield return StartCoroutine(fatigue.HandleTurn());
        }

        RotateTurnOrder();
        StartTurn();
        isProcessingTurn = false;
    }

    /// <summary>
    /// Handle Barra turn separately if it uses BarraFatigue.
    /// </summary>
    private IEnumerator ProcessBarraTurn(BarraMove barra)
    {
        isProcessingTurn = true;
        BarraFatigue barraFatigue = barra.GetComponent<BarraFatigue>();

        if (barraFatigue != null)
        {
            Debug.Log($"Barra {barra.gameObject.name} is taking its turn.");
            yield return StartCoroutine(barraFatigue.HandleTurn());
        }

        RotateTurnOrder();
        StartTurn();
        isProcessingTurn = false;
    }

    // Rebuild the turn-order UI
    private void UpdateTurnOrderUI()
    {
        // Remove old icons
        foreach (Transform child in turnOrderPanel.transform)
        {
            Destroy(child.gameObject);
        }

        // Re-create icons for each unit in turnUnits
        for (int i = 0; i < turnUnits.Count; i++)
        {
            var unit = turnUnits[i];
            bool isActive = (i == currentTurnIndex);

            // Create icon
            GameObject icon = Instantiate(turnOrderPrefab, turnOrderPanel.transform);

            // Tag or just figure out sprite from the type
            Image unitImage = icon.GetComponent<Image>();
            
            // Decide which sprite to assign
            if (unit is PlayerMove)
            {
                unitImage.sprite = isActive ? playerBlueSprite : playerRedSprite;
            }
            else if (unit is AIMove)
            {
                unitImage.sprite = isActive ? enemyBlueSprite : enemyRedSprite;
            }
            else if (unit is GoonMove)
            {
                unitImage.sprite = isActive ? goonBlueSprite : goonRedSprite;
            }
            else if (unit is ElectricianMove)
            {
                unitImage.sprite = isActive ? electricianBlueSprite : electricianRedSprite;
            }
            else if (unit is BarraMove)
            {
                unitImage.sprite = isActive ? barraBlueSprite : barraRedSprite;
            }
            else
            {
                // Fallback if something else is in turnUnits
                unitImage.sprite = enemyRedSprite;
            }

            // If you don’t need TurnOrderIcons, skip adding it:
            // BoxCollider2D collider = icon.AddComponent<BoxCollider2D>();
            // collider.size = new Vector2(150, 150);
        }
    }
}
