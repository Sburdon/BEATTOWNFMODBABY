using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class TempTurnBase : MonoBehaviour
{
    [Header("UI Components")] public GameObject turnOrderPanel; // The panel holding turn order images


    public Sprite playerBlueSprite;
    public Sprite playerRedSprite;
    public Sprite enemyBlueSprite;
    public Sprite enemyRedSprite;
    public Sprite barraBlueSprite;
    public Sprite barraRedSprite; // Blue picture for the active turn

    public Sprite goonRedSprite;
    public Sprite goonBlueSprite;
    public Sprite electBlueSprite;
    public Sprite electRedSprite;


    [Header("Enemy Sprites for Scene 2")]
    public Sprite[] enemySpritesScene2;

    public GameObject turnOrderPrefab; // Prefab for each turn order image


    [Header("AI Units")]
    public GameObject PPShighlight;
    public GameObject moveMentHighlight;
    public List<AIMove> aiUnits = new List<AIMove>();
    public List<BarraMove> barraUnits = new List<BarraMove>();

    // ---------------------------------------------
    // Goon and Electrician
    public List<GoonMove> goonUnits = new List<GoonMove>();
    public List<ElectricianMove> electricianUnits = new List<ElectricianMove>();
    // ---------------------------------------------

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
    private bool spawnBarra2 = true;
    private bool spawnBarra3 = true;
    private bool spawnBarra4 = true;
    private List<object> turnUnits = new List<object>();
    private int currentTurnIndex = 0;//was 0
    private bool hasPlayerTurnBeenSkipped = false;

    public All_SFX All_SFX;

    public Tutorial TutorialScript;

    private int currentUnitIndex = -1; // To track the current unit's index for turn-based rotation

    void Start()
    {
        hook = Hook.Instance;
        respawnManager = RespawnManager.Instance;


        if (playerMove == null)
            playerMove = FindObjectOfType<PlayerMove>();

        if (playerFatigue == null)
            playerFatigue = FindObjectOfType<PlayerFatigue>();

        if (respawnManager == null)
        {
            Debug.LogError("RespawnManager instance not found in the scene.");
        }


        ResetAllColliders();
        InitializeTurnOrder();
        UpdateTurnOrderUI();
        StartTurn();
    }

    /// <summary>
    /// Builds the initial turn order (Player, AI, Goon, Electrician, Barra, etc.).
    /// </summary>
    private void InitializeTurnOrder()
    {
        turnUnits.Clear();

        // Check the active scene
        if (SceneManager.GetActiveScene().name == "ElecScenario") // Replace "Scene2" with your actual scene name
        {
            // Scene 2 order: Electrician, Goon, Player
            foreach (var electrician in electricianUnits)
                turnUnits.Add(electrician);

            foreach (var goon in goonUnits)
                turnUnits.Add(goon);

            // Add player last
            turnUnits.Add(playerMove);
        }
        else
        {
            // Default order: Player, AI, Goon, Electrician, Barra
            turnUnits.Add(playerMove);

            foreach (var ai in aiUnits)
                turnUnits.Add(ai);

            foreach (var barra in barraUnits)
                turnUnits.Add(barra);
        }
    }

    /// <summary>
    /// Moves the current unit (turnUnits[currentTurnIndex]) to the back of the list,
    /// then triggers the rotation animation.
    /// </summary>
    private void RotateTurnOrder()
    {
        var unit = turnUnits[currentTurnIndex];
        turnUnits.RemoveAt(currentTurnIndex);
        turnUnits.Add(unit);

        // Ensure the index wraps around
        currentTurnIndex = currentTurnIndex % turnUnits.Count;

        StartCoroutine(AnimateTurnOrderRotation());
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
        else
        {
            return;
        }
    }

    private IEnumerator AnimateTurnOrderRotation()
    {
        // Get the current unit (who's ending their turn)
        Transform currentUnit = turnOrderPanel.transform.GetChild(currentTurnIndex);

        // Get the total number of images (children) in the panel
        int totalImages = turnOrderPanel.transform.childCount;

        // Calculate the distance to move based on the number of images
        float moveDistanceCur = 150f * totalImages - 80f;
        float moveDistanceLeft = 150f;

        // Slide out the current unit's icon to the right
        LeanTween.move(
            currentUnit.GetComponent<RectTransform>(),
            new Vector2(moveDistanceCur, currentUnit.GetComponent<RectTransform>().anchoredPosition.y), 0.5f).setEaseOutCubic();

        // Slide the other icons left
        for (int i = 0; i < turnOrderPanel.transform.childCount; i++)
        {
            if (i == currentTurnIndex)
                continue;

            Transform otherUnit = turnOrderPanel.transform.GetChild(i);
            LeanTween.move(
                otherUnit.GetComponent<RectTransform>(),
                new Vector2(
                    otherUnit.GetComponent<RectTransform>().anchoredPosition.x - moveDistanceLeft,
                    otherUnit.GetComponent<RectTransform>().anchoredPosition.y), 0.5f).setEaseOutCubic();
        }

        // Wait for the slide-out to complete
        yield return new WaitForSeconds(0.5f);

        // Rearrange the turn order and re-update the UI
        UpdateTurnOrderUI();

        // New active unit
        Transform nextUnit = turnOrderPanel.transform.GetChild(currentTurnIndex);

        // Reset positions
        for (int i = 0; i < turnOrderPanel.transform.childCount; i++)
        {
            Transform unit = turnOrderPanel.transform.GetChild(i);
            unit.GetComponent<RectTransform>().anchoredPosition =
                new Vector2(0f, unit.GetComponent<RectTransform>().anchoredPosition.y);
        }

        // Slide next unit into view
        nextUnit.GetComponent<RectTransform>().anchoredPosition =
            new Vector2(-moveDistanceLeft, nextUnit.GetComponent<RectTransform>().anchoredPosition.y);

        LeanTween.move(
            nextUnit.GetComponent<RectTransform>(),
            new Vector2(0f, nextUnit.GetComponent<RectTransform>().anchoredPosition.y), 0.5f).setEaseInCubic();
    }

    /// <summary>
    /// Called to start the turn for whichever unit is at currentTurnIndex.
    /// </summary>
    /// 
    public int holderOfHookkills = 0;
    public void StartTurn()
    {
        if (turnUnits.Count == 0) return;

        var currentUnit = turnUnits[currentTurnIndex];

        // Immediately skip dead AI units
        if (currentUnit is AIMove ai && ai.isDead)
        {
            RotateTurnOrder();
            StartTurn();
            return;
        }

        if (currentUnit is PlayerMove)
        {
            // Respawn management only during player's turn
            if (respawnManager != null)
            {
                respawnManager.MaintainEnemyCount();
            }
            else
            {
                Debug.LogError("RespawnManager is null in StartTurn()");
            }

            // Hook and Barra spawning logic
            if (hook != null && SceneManager.GetActiveScene().name == "Brady")
            {
                if ((hook.hookKillCount == 2 || hook.hookKillCount == 3) && spawnBarra)
                {
                    RespawnManager.Instance.SpawnBarra();
                    spawnBarra = false;
                    TutorialScript.StartBarraAnimation();
                }
                if ((hook.hookKillCount == 4 || hook.hookKillCount == 5) && spawnBarra1)
                {
                    RespawnManager.Instance.SpawnBarra();
                    spawnBarra1 = false;
                }
            }


            if (hook != null && SceneManager.GetActiveScene().name == "Endless")
            {
                int currentKills = hook.hookKillCount;

                // Handle fixed spawn milestones
                if ((currentKills == 2 || currentKills == 3) && spawnBarra)
                {
                    RespawnManager.Instance.SpawnBarra();
                    spawnBarra = false;
                    TutorialScript.StartBarraAnimation();
                    holderOfHookkills = currentKills;
                    StartPlayerTurn();
                    return;
                }
                if ((currentKills == 4 || currentKills == 5) && spawnBarra1)
                {
                    RespawnManager.Instance.SpawnBarra();
                    spawnBarra1 = false;
                    holderOfHookkills = currentKills;
                    StartPlayerTurn();
                    return;
                }
                if ((currentKills == 6 || currentKills == 7) && spawnBarra2)
                {
                    RespawnManager.Instance.SpawnBarra();
                    spawnBarra2 = false;
                    holderOfHookkills = currentKills;
                    StartPlayerTurn();
                    return;
                }
                if ((currentKills == 8 || currentKills == 9) && spawnBarra3)
                {
                    RespawnManager.Instance.SpawnBarra();
                    spawnBarra3 = false;
                    holderOfHookkills = currentKills;
                    StartPlayerTurn();
                    return;
                }
                if (currentKills >= 10 && spawnBarra4)
                {
                    RespawnManager.Instance.SpawnBarra();
                    spawnBarra4 = false;
                    holderOfHookkills = currentKills;
                    StartPlayerTurn();
                    return;
                }

                // After 11 kills, spawn a barra per kill
                if (currentKills > 10 && !spawnBarra4)
                {
                    int killsToProcess = currentKills - holderOfHookkills;

                    for (int i = 0; i < killsToProcess; i++)
                    {
                        RespawnManager.Instance.SpawnBarra();
                        holderOfHookkills++;
                    }

                    StartPlayerTurn();
                    return;
                }
            }

            StartPlayerTurn();
        }
        else if (currentUnit is AIMove aiUnit)
        {
            StartCoroutine(ProcessAITurn(aiUnit));
        }
        else if (currentUnit is BarraMove barra)
        {
            StartCoroutine(ProcessBarraTurn(barra));
        }
        else if (currentUnit is GoonMove goon)
        {
            StartCoroutine(ProcessGoonTurn(goon));
        }
        else if (currentUnit is ElectricianMove electrician)
        {
            StartCoroutine(ProcessElectricianTurn(electrician));
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
        GameObject[] goons = GameObject.FindGameObjectsWithTag("Goon");
        GameObject[] elects = GameObject.FindGameObjectsWithTag("Electrician");

        foreach (GameObject enemy in enemies)
        {
            ResetCollider(enemy);
        }
        foreach (GameObject barra in barras)
        {
            ResetCollider(barra);
        }
        foreach (GameObject elect in elects)
        {
            ResetCollider(elect);
        }
        foreach (GameObject goon in goons)
        {
            ResetCollider(goon);
        }
    }

    private void ResetCollider(GameObject unit)
    {
        Collider2D collider = unit.GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = false;
            collider.enabled = true; // Reset the collider
        }
    }

    // ─────────────────────────────────────
    // Removal Methods (AI, Barra, Goon, Electrician)
    // ─────────────────────────────────────
    public void RemoveAIUnit(AIMove aiMove)
    {
        if (aiMove != null && aiUnits.Contains(aiMove))
        {
            // Check if the AI died due to player or Barra
            if (aiMove.isDead)
            {
                // If it was a player or Barra that caused the death, set the respawn delay
                aiMove.turnsUntilRespawn = 1; // Delay for one turn
            }
            else
            {
                // Remove the AI unit immediately if it died from a hook
                aiUnits.Remove(aiMove);
                turnUnits.Remove(aiMove);
                UpdateTurnOrderUI();
                Debug.Log($"TempTurnBase: Removed {aiMove.gameObject.name} from AI units.");
            }

            // Update the turn order UI
            UpdateTurnOrderUI();
        }
    }

    public void RemoveBarraUnit(BarraMove barraMove)
    {
        if (barraMove != null && barraUnits.Contains(barraMove))
        {
            barraUnits.Remove(barraMove);
            turnUnits.Remove(barraMove);
            UpdateTurnOrderUI();
            Debug.Log($"TempTurnBase: Removed {barraMove.gameObject.name} from Barra units.");

            if (currentUnitIndex >= aiUnits.Count + barraUnits.Count)
            {
                currentUnitIndex = aiUnits.Count + barraUnits.Count - 1;
            }

            UpdateTurnOrderUI();
        }
    }

    public void RemoveGoonUnit(GoonMove goonMove)
    {
        if (goonMove != null && goonUnits.Contains(goonMove))
        {
            goonUnits.Remove(goonMove);
            turnUnits.Remove(goonMove);
            UpdateTurnOrderUI();
            Debug.Log($"TempTurnBase: Removed {goonMove.gameObject.name} from Goon units.");

            if (currentUnitIndex >=
                aiUnits.Count + barraUnits.Count + goonUnits.Count + electricianUnits.Count)
            {
                currentUnitIndex =
                    aiUnits.Count + barraUnits.Count + goonUnits.Count + electricianUnits.Count - 1;
            }

            UpdateTurnOrderUI();
        }
    }

    public void RemoveElectricianUnit(ElectricianMove electricianMove)
    {
        if (electricianMove != null && electricianUnits.Contains(electricianMove))
        {
            electricianUnits.Remove(electricianMove);
            turnUnits.Remove(electricianMove);
            UpdateTurnOrderUI();
            Debug.Log($"TempTurnBase: Removed {electricianMove.gameObject.name} from Electrician units.");

            if (currentUnitIndex >=
                aiUnits.Count + barraUnits.Count + goonUnits.Count + electricianUnits.Count)
            {
                currentUnitIndex =
                    aiUnits.Count + barraUnits.Count + goonUnits.Count + electricianUnits.Count - 1;
            }

            UpdateTurnOrderUI();
        }
    }

    // ─────────────────────────────────────
    // End Player Turn
    // ─────────────────────────────────────
    public void EndPlayerTurn() // called by button press
    {
        if (!isPlayerTurn || isProcessingTurn) return;

        // If you want to handle some Goon-specific logic at the end of the player's turn:
        foreach (var goon in goonUnits)
        {
            goon.ResolvePunch();
        }
        if (playerMove.pendingMovePurchase)
        {
            playerMove.ResetPendingMove();
        }

        // Reset moves and anything else for next turn
        playerMove.remainingMoves = 0;
        playerMove.UpdateMoveImages();

        PPShighlight.SetActive(false);
        moveMentHighlight.SetActive(false);
        RealMoveHighlight.SetActive(false);
        isPlayerTurn = false;

        RotateTurnOrder();
        StartTurn();
    }

    // ─────────────────────────────────────
    // AI, Barra, Goon, Electrician coroutines
    // ─────────────────────────────────────
    private IEnumerator ProcessAITurn(AIMove ai)
    {
        isProcessingTurn = true;

        // If we have an AI fatigue system:
        AIFatigue aiFatigue = ai.GetComponent<AIFatigue>();
        if (aiFatigue != null)
        {
            Debug.Log($"AI {ai.gameObject.name} is taking its turn.");
            yield return StartCoroutine(aiFatigue.HandleTurn());

            // ⭐ Added: Handle taunt countdown for fish enemies
            ai.DecrementFollowTurns();
            if (!ai.IsFollowingPlayer())
            {
                Debug.Log($"{ai.gameObject.name} is no longer taunted.");
            }
        }
        else
        {
            // Otherwise, do nothing or your own AI logic
            Debug.LogWarning($"No AIFatigue found on {ai.gameObject.name}. The AI won't move.");
            yield return null;
        }

        RotateTurnOrder();
        StartTurn();
        isProcessingTurn = false;
    }


    private IEnumerator ProcessBarraTurn(BarraMove barra)
    {
        isProcessingTurn = true;

        BarraFatigue barraFatigue = barra.GetComponent<BarraFatigue>();
        if (barraFatigue != null)
        {
            Debug.Log($"Barra {barra.gameObject.name} is taking its turn.");
            yield return StartCoroutine(barraFatigue.HandleTurn());
        }
        else
        {
            Debug.LogWarning($"No BarraFatigue found on {barra.gameObject.name}. The Barra won't move.");
            yield return null;
        }

        RotateTurnOrder();
        StartTurn();
        isProcessingTurn = false;

        if (barra.tauntTurnsRemaining > 0)
        {
            barra.tauntTurnsRemaining--;

            if (barra.tauntTurnsRemaining == 0)
            {
                barra.isTaunted = false;
                Debug.Log($"{barra.name}'s taunt has worn off.");
            }
        }

    }

    private IEnumerator ProcessGoonTurn(GoonMove goon)
    {
        GoonMove.GloballyReservedTiles.Clear();

        isProcessingTurn = true;

        GoonFatigue goonFatigue = goon.GetComponent<GoonFatigue>();
        if (goonFatigue != null)
        {
            Debug.Log($"Goon {goon.gameObject.name} is taking its turn.");
            yield return StartCoroutine(goonFatigue.HandleTurn());
            
        }
        else
        {
            Debug.LogWarning($"Goon {goon.gameObject.name} has no GoonFatigue component. Skipping fatigue logic.");
            yield return null;
        }

        RotateTurnOrder();
        StartTurn();
        isProcessingTurn = false;
    }

    private IEnumerator ProcessElectricianTurn(ElectricianMove electrician)
{
    isProcessingTurn = true;
    Debug.Log($"Electrician {electrician.gameObject.name} is taking its turn.");

    ElectricianFatigue elecFatigue = electrician.GetComponent<ElectricianFatigue>();
    if (elecFatigue != null)
    {
        yield return StartCoroutine(elecFatigue.HandleTurn());
    }
    else
    {
        Debug.LogWarning("Electrician is missing ElectricianFatigue script!");
        yield return null;
    }

    RotateTurnOrder();
    StartTurn();
    isProcessingTurn = false;
}


    // ─────────────────────────────────────
    // Start Player Turn
    // ─────────────────────────────────────
    public void StartPlayerTurn()
    {
        isPlayerTurn = true;
        ResetAllColliders();

        // Player fatigue logic
        playerFatigue.RecoverFatigue();
        playerMove.RefreshSpaceCount();
        Debug.Log("Player's turn has started. Fatigue reset to maximum.");

        currentUnitIndex = -1;
        UpdateTurnOrderUI();
    }

    // ─────────────────────────────────────
    // Add Methods (AI, Barra, Goon, Electrician)
    // ─────────────────────────────────────
    public void AddAIUnit(AIMove aiMove)
    {
        if (aiMove != null && !aiUnits.Contains(aiMove))
        {
            aiUnits.Add(aiMove);
            // Insert AI units before goons/electricians/barra if you like
            turnUnits.Insert(turnUnits.Count - (barraUnits.Count + goonUnits.Count + electricianUnits.Count), aiMove);
            UpdateTurnOrderUI();
        }
    }

    public void AddBarraUnit(BarraMove barraMove)
    {
        if (barraMove != null && !barraUnits.Contains(barraMove))
        {
            barraUnits.Add(barraMove);
            turnUnits.Add(barraMove);
            UpdateTurnOrderUI();
        }
    }

    public void AddGoonUnit(GoonMove goonMove)
    {
        if (goonMove != null && !goonUnits.Contains(goonMove))
        {
            goonUnits.Add(goonMove);
            turnUnits.Add(goonMove);
            UpdateTurnOrderUI();
        }
    }

    public void AddElectricianUnit(ElectricianMove electricianMove)
    {
        if (electricianMove != null && !electricianUnits.Contains(electricianMove))
        {
            electricianUnits.Add(electricianMove);
            turnUnits.Add(electricianMove);
            UpdateTurnOrderUI();
        }
    }

    /// <summary>
    /// Rebuilds the turn order UI icons based on turnUnits
    /// and highlights the currentTurnIndex with the blue sprite.
    /// </summary>
    private void UpdateTurnOrderUI()
    {
        // Clear previous turn order UI
        foreach (Transform child in turnOrderPanel.transform)
        {
            Destroy(child.gameObject);
        }

        // Build the list for UI icons
        List<GameObject> turnOrderList = new List<GameObject>();

        foreach (var unit in turnUnits)
        {
            if (unit is PlayerMove)
            {
                turnOrderList.Add(CreateTurnOrderIcon(unit, "Player"));
            }
            else if (unit is AIMove)
            {
                turnOrderList.Add(CreateTurnOrderIcon(unit, "Enemy"));
            }
            else if (unit is BarraMove)
            {
                turnOrderList.Add(CreateTurnOrderIcon(unit, "Barra"));
            }
            else if (unit is GoonMove)
            {
                turnOrderList.Add(CreateTurnOrderIcon(unit, "Goon"));
            }
            else if (unit is ElectricianMove)
            {
                turnOrderList.Add(CreateTurnOrderIcon(unit, "Electrician"));
            }
        }

        // Now set the correct sprite for each icon
        for (int i = 0; i < turnOrderList.Count; i++)
        {
            GameObject icon = turnOrderList[i];
            Image unitImage = icon.GetComponent<Image>();

            if (icon.CompareTag("Player"))
            {
                unitImage.sprite = (i == currentTurnIndex) ? playerBlueSprite : playerRedSprite;
            }
            else if (icon.CompareTag("Enemy"))
            {
                unitImage.sprite = (i == currentTurnIndex) ? enemyBlueSprite : enemyRedSprite;
            }
            else if (icon.CompareTag("Barra"))
            {
                unitImage.sprite = (i == currentTurnIndex) ? barraBlueSprite : barraRedSprite;
            }
            else if (icon.CompareTag("Electrician"))
            {
                unitImage.sprite = (i == currentTurnIndex) ? electBlueSprite : electRedSprite;
            }
            else if (icon.CompareTag("Goon"))
            {
                unitImage.sprite = (i == currentTurnIndex) ? goonBlueSprite : goonRedSprite;
            }
        }
    }

    private GameObject CreateTurnOrderIcon(object unit, string unitType)
    {
        GameObject icon = Instantiate(turnOrderPrefab, turnOrderPanel.transform);
        icon.tag = unitType;

        BoxCollider2D collider = icon.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(150, 150);

        TurnOrderIcons turnOrderIcons = icon.AddComponent<TurnOrderIcons>();

        // Link the enemy if it's not a player
        if (unit is AIMove || unit is BarraMove || unit is GoonMove || unit is ElectricianMove)
        {
            GameObject enemyObject = ((MonoBehaviour)unit).gameObject;
            turnOrderIcons.linkedEnemy = enemyObject;
        }
        else
        {
            turnOrderIcons.linkedEnemy = null;
        }

        return icon;
    }
}