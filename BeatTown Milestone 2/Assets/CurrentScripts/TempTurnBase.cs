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
        else if(totalImages == 2){
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
        else if (totalImages == 4)
        {
            layoutGroup.spacing = -7;
        }
        else { return; }

    }

    private IEnumerator AnimateTurnOrderRotation()
    {
        // Get the current unit (who's ending their turn)
        Transform currentUnit = turnOrderPanel.transform.GetChild(currentTurnIndex);

        // Get the total number of images (children) in the panel
        int totalImages = turnOrderPanel.transform.childCount;

        // Calculate the distance to move based on the number of images (150 * number of images)
        float moveDistanceCur = 150f * totalImages - 80f;
        float moveDistanceLeft = 150f;

        // Slide out the current unit's icon to the right (far off-screen)
        LeanTween.move(currentUnit.GetComponent<RectTransform>(),
                       new Vector2(moveDistanceCur, currentUnit.GetComponent<RectTransform>().anchoredPosition.y),
                       0.5f).setEaseOutCubic(); // Slide right

        // Slide all the other icons to the left (except the current one) at the same time
        for (int i = 0; i < turnOrderPanel.transform.childCount; i++)
        {
            // Skip the current unit (who's ending its turn)
            if (i == currentTurnIndex)
                continue;

            Transform otherUnit = turnOrderPanel.transform.GetChild(i);

            // Slide the other icons to the left by the calculated move distance (150 * total images)
            LeanTween.move(otherUnit.GetComponent<RectTransform>(),
                           new Vector2(otherUnit.GetComponent<RectTransform>().anchoredPosition.x - moveDistanceLeft,
                                       otherUnit.GetComponent<RectTransform>().anchoredPosition.y),
                           0.5f).setEaseOutCubic();
        }

        // Wait for the slide-out to complete (0.5 seconds)
        yield return new WaitForSeconds(0.5f);

        // After the animation completes, we rearrange the turn order and re-update the UI
        UpdateTurnOrderUI();

        // Now, get the new active unit (the one that is next in the turn order)
        Transform nextUnit = turnOrderPanel.transform.GetChild(currentTurnIndex);  // Now points to the new active unit

        // Ensure that the other units that slid left are reset back to their original positions
        for (int i = 0; i < turnOrderPanel.transform.childCount; i++)
        {
            Transform unit = turnOrderPanel.transform.GetChild(i);

            // Reset each unit's position to its original position (assuming we want it centered at x = 0)
            unit.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, unit.GetComponent<RectTransform>().anchoredPosition.y); // Reset positions
        }

        // Set the new unit's position to off-screen to the left before it slides in
        nextUnit.GetComponent<RectTransform>().anchoredPosition = new Vector2(-moveDistanceLeft, nextUnit.GetComponent<RectTransform>().anchoredPosition.y); // Start off-screen to the left

        // Slide the new active unit into view (left to right)
        LeanTween.move(nextUnit.GetComponent<RectTransform>(),
                       new Vector2(0f, nextUnit.GetComponent<RectTransform>().anchoredPosition.y),
                       0.5f).setEaseInCubic(); // Slide in from left to right
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
        updateSpacing();
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

            UpdateTurnOrderUI();
            // Re-update turn order UI after removal
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

            UpdateTurnOrderUI();
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