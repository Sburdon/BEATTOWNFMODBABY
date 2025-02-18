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
    public Sprite barraRedSprite; // Blue picture for the active turn

    public GameObject turnOrderPrefab; // Prefab for each turn order image

    private List<Image> turnOrderImages = new List<Image>();

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
    private List<object> turnUnits = new List<object>();
    private int currentTurnIndex = 0;

    public All_SFX All_SFX;

    private int currentUnitIndex = -1; // To track the current unit's index for turn-based rotation

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

    private void InitializeTurnOrder()
    {
        turnUnits.Clear();
        // Player first
        turnUnits.Add(playerMove);

        // Existing AI
        foreach (var ai in aiUnits)
            turnUnits.Add(ai);

        // Goons
        foreach (var goon in goonUnits)
            turnUnits.Add(goon);

        // Electricians
        foreach (var electrician in electricianUnits)
            turnUnits.Add(electrician);

        // Barras
        foreach (var barra in barraUnits)
            turnUnits.Add(barra);
    }

    private void RotateTurnOrder()
    {
        var unit = turnUnits[currentTurnIndex];
        turnUnits.RemoveAt(currentTurnIndex);
        turnUnits.Add(unit);

        // Ensure the index wraps around
        currentTurnIndex = currentTurnIndex % turnUnits.Count;

        // Animate only the current unit's turn end (optional)
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
            new Vector2(moveDistanceCur, currentUnit.GetComponent<RectTransform>().anchoredPosition.y),
            0.5f
        ).setEaseOutCubic();

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
                    otherUnit.GetComponent<RectTransform>().anchoredPosition.y
                ),
                0.5f
            ).setEaseOutCubic();
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
            new Vector2(0f, nextUnit.GetComponent<RectTransform>().anchoredPosition.y),
            0.5f
        ).setEaseInCubic();
    }

    public void StartTurn()
{
    if (turnUnits.Count == 0) return;

    var currentUnit = turnUnits[currentTurnIndex];

    if (currentUnit is PlayerMove)
    {
        respawnManager.MaintainEnemyCount();
        
        // NEW CHECK: only do killCount checks if hook exists
        if (hook != null)
        {
            if ((hook.hookKillCount == 2 || hook.hookKillCount == 3) && spawnBarra == true)
            {
                RespawnManager.Instance.SpawnBarra();
                spawnBarra = false;
            }
            if ((hook.hookKillCount == 4 || hook.hookKillCount == 5) && spawnBarra1 == true)
            {
                RespawnManager.Instance.SpawnBarra();
                spawnBarra1 = false;
            }
        }

        StartPlayerTurn();
    }
    else if (currentUnit is AIMove ai)
    {
        StartCoroutine(ProcessAITurn(ai));
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

        foreach (GameObject enemy in enemies)
        {
            ResetCollider(enemy);
        }

        foreach (GameObject barra in barras)
        {
            ResetCollider(barra);
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

    // ----------------- REMOVALS -----------------
    public void RemoveAIUnit(AIMove aiMove)
    {
        if (aiMove != null && aiUnits.Contains(aiMove))
        {
            aiUnits.Remove(aiMove);
            turnUnits.Remove(aiMove);
            UpdateTurnOrderUI();
            Debug.Log($"TempTurnBase: Removed {aiMove.gameObject.name} from AI units.");

            if (currentUnitIndex >= aiUnits.Count)
            {
                currentUnitIndex = aiUnits.Count - 1;
            }

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

            if (currentUnitIndex >= aiUnits.Count + barraUnits.Count + goonUnits.Count + electricianUnits.Count)
            {
                currentUnitIndex = aiUnits.Count + barraUnits.Count + goonUnits.Count + electricianUnits.Count - 1;
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
    // -------------------------------------------

    public void EndPlayerTurn()
{
    if (!isPlayerTurn || isProcessingTurn) return;

    // NEW: Resolve all Goon punches here
    foreach (var goon in goonUnits)
    {
        goon.ResolvePunch();
    }

    PPShighlight.SetActive(false);
    moveMentHighlight.SetActive(false);
    RealMoveHighlight.SetActive(false);
    isPlayerTurn = false;

    RotateTurnOrder();
    StartTurn();
}

    // ----------------- TURN PROCESSING -----------------
    private IEnumerator ProcessAITurn(AIMove ai)
    {
        isProcessingTurn = true;
        // If you have AI logic or AI fatigue, handle it here
        Debug.Log($"AI {ai.gameObject.name} is taking its turn.");

        yield return null;

        RotateTurnOrder();
        StartTurn();
        isProcessingTurn = false;
    }

    private IEnumerator ProcessBarraTurn(BarraMove barra)
    {
        isProcessingTurn = true;
        Debug.Log($"Barra {barra.gameObject.name} is taking its turn.");

        yield return null;

        RotateTurnOrder();
        StartTurn();
        isProcessingTurn = false;
    }

    // Here we add GoonFatigue references back in:
    private IEnumerator ProcessGoonTurn(GoonMove goon)
    {
        isProcessingTurn = true;

        // GoonFatigue reference:
        GoonFatigue goonFatigue = goon.GetComponent<GoonFatigue>();
        if (goonFatigue != null)
        {
            Debug.Log($"Goon {goon.gameObject.name} is taking its turn.");
            yield return StartCoroutine(goonFatigue.HandleTurn());
        }
        else
        {
            // If, for some reason, the goon has no GoonFatigue, just do nothing
            Debug.LogWarning($"Goon {goon.gameObject.name} has no GoonFatigue component. Skipping fatigue logic.");
            yield return null;
        }

        RotateTurnOrder();
        StartTurn();
        isProcessingTurn = false;
    }

    // Electrician has no fatigue:
    private IEnumerator ProcessElectricianTurn(ElectricianMove electrician)
{
    isProcessingTurn = true;
    Debug.Log($"Electrician {electrician.gameObject.name} is taking its turn.");

    // Call Electrician's movement routine
    yield return StartCoroutine(electrician.MoveAction());

    // Then continue as normal
    RotateTurnOrder();
    StartTurn();
    isProcessingTurn = false;
}
    // -------------------------------------------

    // ----------------- START / ADD UNITS -----------------
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

    public void AddAIUnit(AIMove aiMove)
    {
        if (aiMove != null && !aiUnits.Contains(aiMove))
        {
            aiUnits.Add(aiMove);
            turnUnits.Insert(turnUnits.Count - barraUnits.Count, aiMove);
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

    // -------------- UPDATE TURN ORDER UI --------------
    private void UpdateTurnOrderUI()
    {
        foreach (Transform child in turnOrderPanel.transform)
        {
            Destroy(child.gameObject);
        }

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
                // Goons are currently using the "Enemy" tag for the UI
                turnOrderList.Add(CreateTurnOrderIcon(unit, "Enemy")); 
            }
            else if (unit is ElectricianMove)
            {
                turnOrderList.Add(CreateTurnOrderIcon(unit, "Enemy"));
            }
        }

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
        }
    }

    private GameObject CreateTurnOrderIcon(object unit, string unitType)
    {
        GameObject icon = Instantiate(turnOrderPrefab, turnOrderPanel.transform);
        icon.tag = unitType;

        BoxCollider2D collider = icon.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(150, 150);

        TurnOrderIcons turnOrderIcons = icon.AddComponent<TurnOrderIcons>();

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
