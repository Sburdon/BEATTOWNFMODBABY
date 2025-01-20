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
   
    [Header("Electrician Sprites (Optional)")]
    public Sprite electricianBlueSprite;
    public Sprite electricianRedSprite;

    public GameObject turnOrderPrefab; // Prefab for each turn order image

    private List<Image> turnOrderImages = new List<Image>();

    [Header("AI Units")]
    public GameObject PPShighlight;
    public GameObject moveMentHighlight;
    public List<AIMove> aiUnits = new List<AIMove>();
    public List<BarraMove> barraUnits = new List<BarraMove>();

    [Header("Electrician Units")]
    public List<ElectricianMove> electricianUnits = new List<ElectricianMove>();

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

        // Hook might be null if there's no Hook in the scene
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

    // --------------------------------------------------------
    // Colliders / Removal
    // --------------------------------------------------------
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

    private void ResetCollider(GameObject unit)
    {
        Collider2D collider = unit.GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = false;
            collider.enabled = true; // Reset the collider by disabling and re-enabling
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

    // --------------------------------------------------------
    // Adding Electricians
    // --------------------------------------------------------
    public void AddElectricianUnit(ElectricianMove electrician)
    {
        if (electrician != null && !electricianUnits.Contains(electrician))
        {
            electricianUnits.Add(electrician);
            ResetAllColliders();
            Debug.Log($"TempTurnBase: Added Electrician {electrician.gameObject.name} to the turn system.");
            UpdateTurnOrderUI();
        }
        else
        {
            Debug.LogWarning("TempTurnBase: Attempted to add a null or already added Electrician.");
        }
    }

    public void RemoveElectricianUnit(ElectricianMove electrician)
    {
        if (electrician != null && electricianUnits.Contains(electrician))
        {
            electricianUnits.Remove(electrician);
            UpdateTurnOrderUI();
            Debug.Log($"TempTurnBase: Removed Electrician {electrician.gameObject.name} from the turn system.");
        }
    }

    // --------------------------------------------------------
    // Turn Logic
    // --------------------------------------------------------
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

        // Make copies so if the lists change mid-turn, we won't skip or double-run any units
        List<AIMove> aiUnitsCopy = new List<AIMove>(aiUnits);
        List<BarraMove> barraUnitsCopy = new List<BarraMove>(barraUnits);
        List<ElectricianMove> electricianUnitsCopy = new List<ElectricianMove>(electricianUnits);

        // 1) Go through AI Units
        for (int i = 0; i < aiUnitsCopy.Count; i++)
        {
            AIMove ai = aiUnitsCopy[i];
            if (ai != null && ai.gameObject.activeInHierarchy)
            {
                currentUnitIndex = aiUnits.IndexOf(ai); // set current unit
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

                ResetAllColliders();
                yield return new WaitForSeconds(0.2f);
            }
        }

        // 2) Go through Electrician Units
        for (int i = 0; i < electricianUnitsCopy.Count; i++)
        {
            ElectricianMove electrician = electricianUnitsCopy[i];
            if (electrician != null && electrician.gameObject.activeInHierarchy)
            {
                // The index for Electrician: after all AI
                currentUnitIndex = aiUnits.Count + electricianUnits.IndexOf(electrician);
                UpdateTurnOrderUI();

                Debug.Log($"Electrician {electrician.gameObject.name} is taking its turn.");
                // If you don't have a Fatigue script for Electricians, call MoveAction directly:
                yield return StartCoroutine(electrician.MoveAction());

                ResetAllColliders();
                yield return new WaitForSeconds(0.2f);
            }
        }

        // 3) Go through Barra Units
        for (int i = 0; i < barraUnitsCopy.Count; i++)
        {
            BarraMove barra = barraUnitsCopy[i];
            if (barra != null && barra.gameObject.activeInHierarchy)
            {
                // The index for Barra: after all AI and Electricians
                currentUnitIndex = aiUnits.Count + electricianUnits.Count + barraUnits.IndexOf(barra);
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

                ResetAllColliders();
                yield return new WaitForSeconds(1f);
            }
        }

        Debug.Log("AI's turn has ended. Starting player's turn.");

        // Safely check hook (in case there's no Hook in scene)
        if (hook != null && (hook.hookKillCount == 2 || hook.hookKillCount == 4))
        {
            // Example: spawn a Barra if killCount hits 2 or 4
            respawnManager.SpawnBarra();
            All_SFX.PlayCUANG();
        }

        // Maintain enemies if needed
        if (respawnManager != null)
        {
            respawnManager.MaintainEnemyCount();
        }

        ResetAllColliders();
        StartPlayerTurn();
        ResetAllColliders();

        isProcessingTurn = false;
    }

    public void StartPlayerTurn()
    {
        isPlayerTurn = true;
        ResetAllColliders();
        if (playerFatigue != null)
        {
            playerFatigue.RecoverFatigue();
        }
        if (playerMove != null)
        {
            playerMove.RefreshSpaceCount();
        }

        Debug.Log("Player's turn has started. Fatigue reset to maximum.");
        currentUnitIndex = -1; // Reset the index for next rotation
        UpdateTurnOrderUI();
    }

    // --------------------------------------------------------
    // Adding Regular AI or Barra
    // --------------------------------------------------------
    public void AddAIUnit(AIMove aiMove)
    {
        if (aiMove != null && !aiUnits.Contains(aiMove))
        {
            aiUnits.Add(aiMove);
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
            ResetAllColliders();

            Debug.Log($"TempTurnBase: Added Barra {barraMove.gameObject.name} to the turn system.");
            UpdateTurnOrderUI();
        }
        else
        {
            Debug.LogWarning("TempTurnBase: Attempted to add a null or already added Barra.");
        }
    }

    // --------------------------------------------------------
    // Turn Order UI
    // --------------------------------------------------------
    private void UpdateTurnOrderUI()
    {
        // Clear existing icons
        foreach (Transform child in turnOrderPanel.transform)
        {
            Destroy(child.gameObject);
        }

        // 1) Add player icon
        GameObject playerIcon = Instantiate(turnOrderPrefab, turnOrderPanel.transform);
        Image playerImage = playerIcon.GetComponent<Image>();
        playerImage.sprite = isPlayerTurn ? playerBlueSprite : playerRedSprite;
        playerImage.preserveAspect = true;

        // 2) Add AI icons
        for (int i = 0; i < aiUnits.Count; i++)
        {
            AIMove ai = aiUnits[i];
            GameObject aiIcon = Instantiate(turnOrderPrefab, turnOrderPanel.transform);
            Image aiImage = aiIcon.GetComponent<Image>();

            // If it's the current unit's turn, use the "blue" version
            if (i == currentUnitIndex)
                aiImage.sprite = enemyBlueSprite;
            else
                aiImage.sprite = enemyRedSprite;

            aiImage.preserveAspect = true;
        }

        // 3) Add Electrician icons
        for (int i = 0; i < electricianUnits.Count; i++)
        {
            ElectricianMove electrician = electricianUnits[i];
            GameObject elecIcon = Instantiate(turnOrderPrefab, turnOrderPanel.transform);
            Image elecImage = elecIcon.GetComponent<Image>();

            // If you assigned electricianBlueSprite/electricianRedSprite, use them.
            // Otherwise, fall back to enemy sprites
            Sprite blue = (electricianBlueSprite != null) ? electricianBlueSprite : enemyBlueSprite;
            Sprite red  = (electricianRedSprite != null)  ? electricianRedSprite  : enemyRedSprite;

            // The index for Electricians is "aiUnits.Count + i"
            if ((aiUnits.Count + i) == currentUnitIndex)
                elecImage.sprite = blue;
            else
                elecImage.sprite = red;

            elecImage.preserveAspect = true;
        }

        // 4) Add Barra icons
        for (int i = 0; i < barraUnits.Count; i++)
        {
            BarraMove barra = barraUnits[i];
            GameObject barraIcon = Instantiate(turnOrderPrefab, turnOrderPanel.transform);
            Image barraImage = barraIcon.GetComponent<Image>();

            // The index for Barra is "aiUnits.Count + electricianUnits.Count + i"
            int thisBarraIndex = aiUnits.Count + electricianUnits.Count + i;
            if (thisBarraIndex == currentUnitIndex)
                barraImage.sprite = barraBlueSprite;
            else
                barraImage.sprite = barraRedSprite;

            barraImage.preserveAspect = true;
        }
    }
}
