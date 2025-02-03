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

        InitializeTurnOrder();
        UpdateTurnOrderUI();
    }

    private void InitializeTurnOrder()
    {
        turnUnits.Clear();
        turnUnits.Add(playerMove); // Add player first

        foreach (var ai in aiUnits)
            turnUnits.Add(ai);

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

        UpdateTurnOrderUI();
    }
    public void StartTurn()
    {
        
        if (turnUnits.Count == 0) return;

        var currentUnit = turnUnits[currentTurnIndex];

        if (currentUnit is PlayerMove)
        {
            respawnManager.MaintainEnemyCount();
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
            turnUnits.Remove(aiMove); // Remove from turn order
            UpdateTurnOrderUI();
            Debug.Log($"TempTurnBase: Removed {aiMove.gameObject.name} from AI units.");

            // Adjust the turn order index if needed
            if (currentUnitIndex >= aiUnits.Count)
            {
                currentUnitIndex = aiUnits.Count - 1; // Adjust index
            }

            UpdateTurnOrderUI(); // Re-update turn order UI after removal
        }
    }

    public void RemoveBarraUnit(BarraMove barraMove)
    {
        if (barraMove != null && barraUnits.Contains(barraMove))
        {
            barraUnits.Remove(barraMove);
            turnUnits.Remove(barraMove); // Remove from turn order
            UpdateTurnOrderUI();
            Debug.Log($"TempTurnBase: Removed {barraMove.gameObject.name} from Barra units.");

            // Adjust the turn order index if needed
            if (currentUnitIndex >= aiUnits.Count + barraUnits.Count)
            {
                currentUnitIndex = aiUnits.Count + barraUnits.Count - 1; // Adjust index
            }

            UpdateTurnOrderUI(); // Re-update turn order UI after removal
        }
    }


    public void EndPlayerTurn()
    {
        if (!isPlayerTurn || isProcessingTurn) return;

        PPShighlight.SetActive(false);
        moveMentHighlight.SetActive(false);
        RealMoveHighlight.SetActive(false);
        isPlayerTurn = false;

        // Rotate turn order after player turn ends
        RotateTurnOrder();
        StartTurn();
    }


    private IEnumerator ProcessAITurn(AIMove ai)
    {
        isProcessingTurn = true;
        AIFatigue aiFatigue = ai.GetComponent<AIFatigue>();

        if (aiFatigue != null)
        {
            Debug.Log($"AI {ai.gameObject.name} is taking its turn.");
            yield return StartCoroutine(aiFatigue.HandleTurn());
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

        RotateTurnOrder();
        StartTurn();
        isProcessingTurn = false;
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
            turnUnits.Insert(turnUnits.Count - barraUnits.Count, aiMove); // Insert before Barra units
            UpdateTurnOrderUI();
        }
    }

    public void AddBarraUnit(BarraMove barraMove)
    {
        if (barraMove != null && !barraUnits.Contains(barraMove))
        {
            barraUnits.Add(barraMove);
            turnUnits.Add(barraMove); // Barra units go to the end
            UpdateTurnOrderUI();
        }
    }


    private void UpdateTurnOrderUI()
    {
        // Clear previous turn order UI
        foreach (Transform child in turnOrderPanel.transform)
        {
            Destroy(child.gameObject);
        }

        // Iterate through all the units in the turn order
        for (int i = 0; i < turnUnits.Count; i++)
        {
            // Check if the unit is no longer active or has been removed
            if (turnUnits[i] is AIMove aiUnit && (aiUnit == null || !aiUnit.gameObject.activeInHierarchy))
            {
                // Skip adding this unit to the turn order UI
                continue;
            }

            if (turnUnits[i] is BarraMove barraUnit && (barraUnit == null || !barraUnit.gameObject.activeInHierarchy))
            {
                // Skip adding this unit to the turn order UI
                continue;
            }

            GameObject icon = Instantiate(turnOrderPrefab, turnOrderPanel.transform);
            Image unitImage = icon.GetComponent<Image>();

            // Set the correct sprite based on whether it's the current unit's turn
            if (turnUnits[i] is PlayerMove)
            {
                unitImage.sprite = (i == currentTurnIndex) ? playerBlueSprite : playerRedSprite;
            }
            else if (turnUnits[i] is AIMove)
            {
                unitImage.sprite = (i == currentTurnIndex) ? enemyBlueSprite : enemyRedSprite;
            }
            else if (turnUnits[i] is BarraMove)
            {
                unitImage.sprite = (i == currentTurnIndex) ? barraBlueSprite : barraRedSprite;
            }

            unitImage.preserveAspect = true;
        }
    }


}