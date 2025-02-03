using System;
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


    [Header("AI Units")]
    public GameObject PPShighlight;
    public GameObject moveMentHighlight;
    public List<AIMove> aiUnits = new List<AIMove>();
    public List<BarraMove> barraUnits = new List<BarraMove>();

    [Header("Player Components")]
    public PlayerMove playerMove;
    public PlayerFatigue playerFatigue;
    private RespawnManager respawnManager;
    private Hook hook;

    private bool isPlayerTurn = true;
    private bool isProcessingTurn = false;
    public GameObject RealMoveHighlight;
    private bool spawnBarra = true;
    private bool spawnBarra1 = true;

    public Tutorial TutorialScript; // Reference to the Tutorial script

    public All_SFX All_SFX;

    private int currentUnitIndex = -1; // To track the current unit's index for turn-based rotation
    public TurnOrderType currentTurnOrder = TurnOrderType.PlayerAIBarra;

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
        UpdateTurnOrderUI();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetAllColliders();
        }
    }
    public void ResetAllColliders()
    {
        // Find all GameObjects with the "Enemy" or "Barra" tag
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
    public void RemoveAIUnit(AIMove aiMove)
    {
        if (aiMove != null && aiUnits.Contains(aiMove))
        {
            aiUnits.Remove(aiMove);
            UpdateTurnOrderUI();
            Debug.Log($"TempTurnBase: Removed {aiMove.gameObject.name} from AI units.");
        }
    }

    public void RemoveBarraUnit(BarraMove barraMove)
    {
        if (barraMove != null && barraUnits.Contains(barraMove))
        {
            barraUnits.Remove(barraMove);
            UpdateTurnOrderUI();
            Debug.Log($"TempTurnBase: Removed {barraMove.gameObject.name} from Barra units.");
        }
    }

    public void EndPlayerTurn()
    {
        if (!isPlayerTurn || isProcessingTurn) return;

        PPShighlight.SetActive(false);
        moveMentHighlight.SetActive(false);
        RealMoveHighlight.SetActive(false);
        isPlayerTurn = false;

        StartCoroutine(AITurnRoutine());
    }

    private IEnumerator AITurnRoutine()
    {
        isProcessingTurn = true;
        Debug.Log("AI's turn has started.");

        // Start with the AI/Barra units turn in order
        List<AIMove> aiUnitsCopy = new List<AIMove>(aiUnits);
        List<BarraMove> barraUnitsCopy = new List<BarraMove>(barraUnits);

        // Go through AI Units first
        foreach (AIMove ai in aiUnitsCopy)
        {
            if (ai != null && ai.gameObject.activeInHierarchy)
            {
                currentUnitIndex = aiUnits.IndexOf(ai); // Set the current unit's index

                // Highlight current unit
                ChangeTurnOrder(TurnOrderType.AIBarraPlayer);
                UpdateTurnOrderUI();
                AIFatigue aiFatigue = ai.GetComponent<AIFatigue>();

                if (aiFatigue != null)
                {
                    Debug.Log($"AI {ai.gameObject.name} is taking its turn.");
                    yield return StartCoroutine(aiFatigue.HandleTurn());
                }
                else
                {
                    Debug.LogWarning($"AI {ai.gameObject.name} lacks AIFatigue component.");
                }

                // Reset the collider after AI unit's turn
                ResetAllColliders();
                yield return new WaitForSeconds(0.2f);
            }
        }

        // Go through Barra Units
        foreach (BarraMove barra in barraUnitsCopy)
        {
            if (barra != null && barra.gameObject.activeInHierarchy)
            {
                currentUnitIndex = aiUnits.Count + barraUnits.IndexOf(barra); // Barra index after AI units

                // Highlight current unit
                ChangeTurnOrder(TurnOrderType.BarraPlayerAI);

                UpdateTurnOrderUI();
                BarraFatigue barraFatigue = barra.GetComponent<BarraFatigue>();

                if (barraFatigue != null)
                {
                    Debug.Log($"Barra {barra.gameObject.name} is taking its turn.");
                    yield return StartCoroutine(barraFatigue.HandleTurn());
                }
                else
                {
                    Debug.LogWarning($"Barra {barra.gameObject.name} lacks BarraFatigue component.");
                }

                // Reset the collider after Barra unit's turn
                ResetAllColliders();
                yield return new WaitForSeconds(1f);
            }
        }

        Debug.Log("AI's turn has ended. Starting player's turn.");
        respawnManager.MaintainEnemyCount();
        ResetAllColliders();
        StartPlayerTurn();
        ResetAllColliders();
        isProcessingTurn = false;

        if ((hook.hookKillCount == 2 || hook.hookKillCount == 3) && spawnBarra == true)
        {

            Debug.Log("First hook kill count reached. Barra tutorial animation triggered.");
            RespawnManager.Instance.SpawnBarra();
            spawnBarra = false;

            // use TutorialManager ref to initiate barra tutorial animation
            TutorialScript.StartBarraAnimation();
        }
        if ((hook.hookKillCount == 4 || hook.hookKillCount == 5) && spawnBarra1 == true)
        {
            RespawnManager.Instance.SpawnBarra();

            spawnBarra1 = false;
            Debug.Log("Second hook kill count reached. Barra tutorial animation triggered.");

            // use TutorialManager ref to initiate barra tutorial animation
            TutorialScript.StartBarraAnimation();
        }
    }

    private void ResetCollider(GameObject unit)
    {
        Collider2D collider = unit.GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = false;
            collider.enabled = true; // Reset the collider by disabling and re-enabling
        }
    }

    public void StartPlayerTurn()
    {
        
        ChangeTurnOrder(TurnOrderType.PlayerAIBarra);
        isPlayerTurn = true;
        ResetAllColliders();
        playerFatigue.RecoverFatigue();
        playerMove.RefreshSpaceCount();
        Debug.Log("Player's turn has started. Fatigue reset to maximum.");
        currentUnitIndex = -1; // Reset the index for next rotation
        UpdateTurnOrderUI();
    }

    public void AddAIUnit(AIMove aiMove)
    {
        if (aiMove != null && !aiUnits.Contains(aiMove))
        {
            aiUnits.Add(aiMove);
            // Reset the collider of the AI unit
            ResetAllColliders();

            Debug.Log($"TempTurnBase: Added AIMove {aiMove.gameObject.name} to the turn system.");
            UpdateTurnOrderUI();
        }
        else
        {
            Debug.LogWarning("TempTurnBase: Attempted to add a null or already added AIMove.");
        }
    }

    public void AddBarraUnit(BarraMove barraMove)
    {
        if (barraMove != null && !barraUnits.Contains(barraMove))
        {
            barraUnits.Add(barraMove);
            // Reset the collider of the Barra unit
            ResetAllColliders();

            Debug.Log($"TempTurnBase: Added Barra {barraMove.gameObject.name} to the turn system.");
            UpdateTurnOrderUI();
        }
        else
        {
            Debug.LogWarning("TempTurnBase: Attempted to add a null or already added Barra.");
        }
    }
    public enum TurnOrderType
    {
        PlayerAIBarra,  // Player, AI, Barra
        AIBarraPlayer,  // AI, Barra, Player
        BarraPlayerAI   // Barra, Player, AI
    }

    private List<Transform> unitIcons = new List<Transform>();

    private void UpdateTurnOrderUI()
    {
        // Clear existing icons
        foreach (Transform child in turnOrderPanel.transform)
        {
            Destroy(child.gameObject);
        }

        List<GameObject> turnOrderIcons = new List<GameObject>();

        // Add icons to the list based on the current order type
        switch (currentTurnOrder)
        {
            case TurnOrderType.PlayerAIBarra:
                // Add player turn icon
                turnOrderIcons.Add(CreateTurnOrderIcon(playerBlueSprite, isPlayerTurn));
                // Add AI units' icons
                foreach (AIMove ai in aiUnits)
                {
                    turnOrderIcons.Add(CreateTurnOrderIcon(
                        aiUnits.IndexOf(ai) == currentUnitIndex ? enemyBlueSprite : enemyRedSprite));
                }
                // Add Barra units' icons
                foreach (BarraMove barra in barraUnits)
                {
                    turnOrderIcons.Add(CreateTurnOrderIcon(
                        (barraUnits.IndexOf(barra) + aiUnits.Count == currentUnitIndex) ? barraBlueSprite : barraRedSprite));
                }
                break;

            case TurnOrderType.AIBarraPlayer:
                // Add AI units' icons
                foreach (AIMove ai in aiUnits)
                {
                    turnOrderIcons.Add(CreateTurnOrderIcon(
                        aiUnits.IndexOf(ai) == currentUnitIndex ? enemyBlueSprite : enemyRedSprite));
                }
                // Add Barra units' icons
                foreach (BarraMove barra in barraUnits)
                {
                    turnOrderIcons.Add(CreateTurnOrderIcon(
                        (barraUnits.IndexOf(barra) + aiUnits.Count == currentUnitIndex) ? barraBlueSprite : barraRedSprite));
                }
                // Add player turn icon
                turnOrderIcons.Add(CreateTurnOrderIcon(playerBlueSprite, isPlayerTurn));
                break;

            case TurnOrderType.BarraPlayerAI:
                // Add Barra units' icons
                foreach (BarraMove barra in barraUnits)
                {
                    turnOrderIcons.Add(CreateTurnOrderIcon(
                        (barraUnits.IndexOf(barra) == currentUnitIndex) ? barraBlueSprite : barraRedSprite));
                }
                // Add player turn icon
                turnOrderIcons.Add(CreateTurnOrderIcon(playerBlueSprite, isPlayerTurn));
                // Add AI units' icons
                foreach (AIMove ai in aiUnits)
                {
                    turnOrderIcons.Add(CreateTurnOrderIcon(
                        aiUnits.IndexOf(ai) == currentUnitIndex ? enemyBlueSprite : enemyRedSprite));
                }
                break;
        }

        // Add all the icons to the panel
        foreach (GameObject icon in turnOrderIcons)
        {
            icon.transform.SetParent(turnOrderPanel.transform);
        }
    }

    private GameObject CreateTurnOrderIcon(Sprite sprite, bool isActive = false)
    {
        GameObject icon = Instantiate(turnOrderPrefab, turnOrderPanel.transform);
        UnityEngine.UI.Image image = icon.GetComponent<UnityEngine.UI.Image>();
        image.sprite = sprite;
        image.preserveAspect = true;
        // Optionally, you can add any effect when the unit is active (e.g., apply a glow effect or change alpha)
        if (isActive)
        {
            // For example, you can change the color of the icon if it’s the active turn
            image.color = new Color(1, 1, 1, 1); // Active color (white)
        }
        else
        {
            image.color = new Color(1, 1, 1, 0.5f); // Inactive color (faded)
        }
        return icon;
    }
    public void ChangeTurnOrder(TurnOrderType newOrder)
    {
        // Set the turn order to the passed in value
        currentTurnOrder = newOrder;

        // Update the UI to reflect the new turn order
        UpdateTurnOrderUI();
    }



}