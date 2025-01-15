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

    [Header("Player Components")]
    public PlayerMove playerMove;
    public PlayerFatigue playerFatigue;
    private RespawnManager respawnManager;
    private Hook hook;

    private bool isPlayerTurn = true;
    private bool isProcessingTurn = false;
    public GameObject RealMoveHighlight;

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
        if (hook.hookKillCount == 2 || hook.hookKillCount == 4)
        {
            RespawnManager.Instance.SpawnBarra();
            All_SFX.PlayCUANG();
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

    private void UpdateTurnOrderUI()
    {
        // Clear existing icons
        foreach (Transform child in turnOrderPanel.transform)
        {
            Destroy(child.gameObject);
        }

        // Add player turn icon
        GameObject playerIcon = Instantiate(turnOrderPrefab, turnOrderPanel.transform);
        UnityEngine.UI.Image playerImage = playerIcon.GetComponent<UnityEngine.UI.Image>();
        playerImage.sprite = isPlayerTurn ? playerBlueSprite : playerRedSprite;
        playerImage.preserveAspect = true;

        // Add AI units' icons and highlight the current turn unit
        foreach (AIMove ai in aiUnits)
        {
            GameObject aiIcon = Instantiate(turnOrderPrefab, turnOrderPanel.transform);
            UnityEngine.UI.Image aiImage = aiIcon.GetComponent<UnityEngine.UI.Image>();

            // Set the color based on whether it's this unit's turn
            aiImage.sprite = (aiUnits.IndexOf(ai) == currentUnitIndex) ? enemyBlueSprite : enemyRedSprite;
            aiImage.preserveAspect = true;
        }

        // Add Barra units' icons and highlight the current turn unit
        foreach (BarraMove barra in barraUnits)
        {
            GameObject barraIcon = Instantiate(turnOrderPrefab, turnOrderPanel.transform);
            UnityEngine.UI.Image barraImage = barraIcon.GetComponent<UnityEngine.UI.Image>();

            // Set the color based on whether it's this unit's turn
            barraImage.sprite = (barraUnits.IndexOf(barra) + aiUnits.Count == currentUnitIndex) ? barraBlueSprite : barraRedSprite;
            barraImage.preserveAspect = true;
        }
    }
}