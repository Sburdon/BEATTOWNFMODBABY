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

    [Header("Goon Sprites (Optional)")]
    public Sprite goonBlueSprite;
    public Sprite goonRedSprite;

    public GameObject turnOrderPrefab; // Prefab for each turn order image

    private List<Image> turnOrderImages = new List<Image>();

    [Header("AI Units")]
    public GameObject PPShighlight;
    public GameObject moveMentHighlight;
    public List<AIMove> aiUnits = new List<AIMove>();
    public List<BarraMove> barraUnits = new List<BarraMove>();

    [Header("Electrician Units")]
    public List<ElectricianMove> electricianUnits = new List<ElectricianMove>();

    [Header("Goon Units")]
    public List<GoonMove> goonUnits = new List<GoonMove>();

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

    // --------------------------------------------------------
    // Colliders / Removal
    // --------------------------------------------------------
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
            collider.enabled = true;
        }
    }

    // Regular AI
    public void RemoveAIUnit(AIMove aiMove)
    {
        if (aiMove != null && aiUnits.Contains(aiMove))
        {
            aiUnits.Remove(aiMove);
            UpdateTurnOrderUI();
            Debug.Log($"TempTurnBase: Removed {aiMove.gameObject.name} from AI units.");
        }
    }

    // Barra
    public void RemoveBarraUnit(BarraMove barraMove)
    {
        if (barraMove != null && barraUnits.Contains(barraMove))
        {
            barraUnits.Remove(barraMove);
            UpdateTurnOrderUI();
            Debug.Log($"TempTurnBase: Removed {barraMove.gameObject.name} from Barra units.");
        }
    }

    // Electrician
    public void AddElectricianUnit(ElectricianMove electrician)
    {
        if (electrician != null && !electricianUnits.Contains(electrician))
        {
            electricianUnits.Add(electrician);
            ResetAllColliders();
            Debug.Log($"TempTurnBase: Added Electrician {electrician.gameObject.name} to turn system.");
            UpdateTurnOrderUI();
        }
    }

    public void RemoveElectricianUnit(ElectricianMove electrician)
    {
        if (electrician != null && electricianUnits.Contains(electrician))
        {
            electricianUnits.Remove(electrician);
            UpdateTurnOrderUI();
            Debug.Log($"TempTurnBase: Removed Electrician {electrician.gameObject.name} from turn system.");
        }
    }

    // Goon
    public void AddGoonUnit(GoonMove goon)
    {
        if (goon != null && !goonUnits.Contains(goon))
        {
            goonUnits.Add(goon);
            ResetAllColliders();
            Debug.Log($"TempTurnBase: Added Goon {goon.gameObject.name} to turn system.");
            UpdateTurnOrderUI();
        }
    }

    public void RemoveGoonUnit(GoonMove goon)
    {
        if (goon != null && goonUnits.Contains(goon))
        {
            goonUnits.Remove(goon);
            UpdateTurnOrderUI();
            Debug.Log($"TempTurnBase: Removed Goon {goon.gameObject.name} from turn system.");
        }
    }

    // --------------------------------------------------------
    // Turn Flow
    // --------------------------------------------------------
    public void EndPlayerTurn()
    {
        if (!isPlayerTurn || isProcessingTurn) return;

        // Resolve any Goon punches that were "charged"
        ResolveAllGoonPunches();

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

        List<AIMove> aiUnitsCopy = new List<AIMove>(aiUnits);
        List<GoonMove> goonUnitsCopy = new List<GoonMove>(goonUnits);
        List<ElectricianMove> electricianUnitsCopy = new List<ElectricianMove>(electricianUnits);
        List<BarraMove> barraUnitsCopy = new List<BarraMove>(barraUnits);

        // 1) Regular AI
        for (int i = 0; i < aiUnitsCopy.Count; i++)
        {
            AIMove ai = aiUnitsCopy[i];
            if (ai != null && ai.gameObject.activeInHierarchy)
            {
                currentUnitIndex = aiUnits.IndexOf(ai);
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

        // 2) Goon Units
        for (int i = 0; i < goonUnitsCopy.Count; i++)
        {
            GoonMove goon = goonUnitsCopy[i];
            if (goon != null && goon.gameObject.activeInHierarchy)
            {
                currentUnitIndex = aiUnits.Count + goonUnits.IndexOf(goon);
                UpdateTurnOrderUI();

                GoonFatigue gf = goon.GetComponent<GoonFatigue>();
                if (gf != null)
                {
                    Debug.Log($"Goon {goon.gameObject.name} is taking its turn via GoonFatigue.");
                    gf.ResetFatigue();
                    yield return StartCoroutine(gf.HandleTurn());
                }
                else
                {
                    // No GoonFatigue present, so we skip the turn rather than calling DecideAndAct (which no longer exists).
                    Debug.LogWarning($"Goon {goon.gameObject.name} lacks GoonFatigue component. Skipping Goon turn.");
                }

                ResetAllColliders();
                yield return new WaitForSeconds(0.2f);
            }
        }

        // 3) Electricians
        for (int i = 0; i < electricianUnitsCopy.Count; i++)
        {
            ElectricianMove electrician = electricianUnitsCopy[i];
            if (electrician != null && electrician.gameObject.activeInHierarchy)
            {
                currentUnitIndex = aiUnits.Count + goonUnits.Count + electricianUnits.IndexOf(electrician);
                UpdateTurnOrderUI();

                // Electrician typically doesn't have a Fatigue script, so just call MoveAction
                Debug.Log($"Electrician {electrician.gameObject.name} is taking its turn.");
                yield return StartCoroutine(electrician.MoveAction());

                ResetAllColliders();
                yield return new WaitForSeconds(0.2f);
            }
        }

        // 4) Barra
        for (int i = 0; i < barraUnitsCopy.Count; i++)
        {
            BarraMove barra = barraUnitsCopy[i];
            if (barra != null && barra.gameObject.activeInHierarchy)
            {
                currentUnitIndex = aiUnits.Count + goonUnits.Count + electricianUnits.Count + barraUnits.IndexOf(barra);
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

        // Optional hooking logic
        if (hook != null && (hook.hookKillCount == 2 || hook.hookKillCount == 4))
        {
            respawnManager.SpawnBarra();
            All_SFX.PlayCUANG();
        }

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
        currentUnitIndex = -1;
        UpdateTurnOrderUI();
    }

    // Resolve any Goon punches that are "charged" at the END of the player's turn
    private void ResolveAllGoonPunches()
    {
        for (int i = 0; i < goonUnits.Count; i++)
        {
            GoonMove goon = goonUnits[i];
            if (goon != null && goon.gameObject.activeInHierarchy)
            {
                goon.ResolvePunch();
            }
        }
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

        // 1) Player Icon
        GameObject playerIcon = Instantiate(turnOrderPrefab, turnOrderPanel.transform);
        Image playerImage = playerIcon.GetComponent<Image>();
        playerImage.sprite = isPlayerTurn ? playerBlueSprite : playerRedSprite;
        playerImage.preserveAspect = true;

        // 2) AI
        for (int i = 0; i < aiUnits.Count; i++)
        {
            AIMove ai = aiUnits[i];
            GameObject aiIcon = Instantiate(turnOrderPrefab, turnOrderPanel.transform);
            Image aiImage = aiIcon.GetComponent<Image>();

            if (i == currentUnitIndex)
                aiImage.sprite = enemyBlueSprite;
            else
                aiImage.sprite = enemyRedSprite;

            aiImage.preserveAspect = true;
        }

        // 3) Goon
        for (int i = 0; i < goonUnits.Count; i++)
        {
            GoonMove goon = goonUnits[i];
            GameObject goonIcon = Instantiate(turnOrderPrefab, turnOrderPanel.transform);
            Image goonImage = goonIcon.GetComponent<Image>();

            Sprite blue = goonBlueSprite != null ? goonBlueSprite : enemyBlueSprite;
            Sprite red  = goonRedSprite != null ? goonRedSprite : enemyRedSprite;

            // Index for Goon is: aiUnits.Count + i
            if ((aiUnits.Count + i) == currentUnitIndex)
                goonImage.sprite = blue;
            else
                goonImage.sprite = red;

            goonImage.preserveAspect = true;
        }

        // 4) Electrician
        for (int i = 0; i < electricianUnits.Count; i++)
        {
            ElectricianMove electrician = electricianUnits[i];
            GameObject elecIcon = Instantiate(turnOrderPrefab, turnOrderPanel.transform);
            Image elecImage = elecIcon.GetComponent<Image>();

            Sprite blueElec = (electricianBlueSprite != null) ? electricianBlueSprite : enemyBlueSprite;
            Sprite redElec  = (electricianRedSprite != null) ? electricianRedSprite : enemyRedSprite;

            if ((aiUnits.Count + goonUnits.Count + i) == currentUnitIndex)
                elecImage.sprite = blueElec;
            else
                elecImage.sprite = redElec;

            elecImage.preserveAspect = true;
        }

        // 5) Barra
        for (int i = 0; i < barraUnits.Count; i++)
        {
            BarraMove barra = barraUnits[i];
            GameObject barraIcon = Instantiate(turnOrderPrefab, turnOrderPanel.transform);
            Image barraImage = barraIcon.GetComponent<Image>();

            int indexBarra = aiUnits.Count + goonUnits.Count + electricianUnits.Count + i;
            if (indexBarra == currentUnitIndex)
                barraImage.sprite = barraBlueSprite;
            else
                barraImage.sprite = barraRedSprite;

            barraImage.preserveAspect = true;
        }
    }
}
