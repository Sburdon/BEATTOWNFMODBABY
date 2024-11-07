using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempTurnBase : MonoBehaviour
{
    [Header("AI Units")]
    public GameObject PPShighlight;
    public GameObject moveMentHighlight;
    public List<AIMove> aiUnits = new List<AIMove>();
    public List<BarraMove> barraUnits = new List<BarraMove>();

    [Header("Player Components")]
    public PlayerMove playerMove;
    public PlayerFatigue playerFatigue;

    private bool isPlayerTurn = true;
    private bool isProcessingTurn = false;
    public GameObject RealMoveHighlight;


    void Start()
    {
        if (playerMove == null)
            playerMove = FindObjectOfType<PlayerMove>();

        if (playerFatigue == null)
            playerFatigue = FindObjectOfType<PlayerFatigue>();
    }

    void Update()
    {
        if (isPlayerTurn && !isProcessingTurn)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                EndPlayerTurn();
            }
        }
    }

    public void RemoveAIUnit(AIMove aiMove)
    {
        if (aiMove != null && aiUnits.Contains(aiMove))
        {
            aiUnits.Remove(aiMove);
            Debug.Log($"TempTurnBase: Removed {aiMove.gameObject.name} from AI units.");
        }
    }

    public void RemoveBarraUnit(BarraMove barraMove)
    {
        if (barraMove != null && barraUnits.Contains(barraMove))
        {
            barraUnits.Remove(barraMove);
            Debug.Log($"TempTurnBase: Removed {barraMove.gameObject.name} from Barra units.");
        }
    }

    /// <summary>
    /// Ends the player's turn and initiates the AI's turn.
    /// </summary>
    public void EndPlayerTurn()
    {
        if (!isPlayerTurn || isProcessingTurn) return;

        PPShighlight.SetActive(false);
        moveMentHighlight.SetActive(false);
        RealMoveHighlight.SetActive(false);
        isPlayerTurn = false;
        StartCoroutine(AITurnRoutine());
    }

    /// <summary>
    /// Coroutine that handles the AI's turn.
    /// </summary>
    private IEnumerator AITurnRoutine()
    {
        isProcessingTurn = true;
        Debug.Log("AI's turn has started.");

        // Make a copy of the list to avoid modification during iteration
        List<AIMove> aiUnitsCopy = new List<AIMove>(aiUnits);

        // Process each AIMove unit
        foreach (AIMove ai in aiUnitsCopy)
        {
            if (ai != null && ai.gameObject.activeInHierarchy)
            {
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

                yield return new WaitForSeconds(0.2f); // Delay between AI units
            }
        }

        // Make a copy of the Barra list to avoid modification during iteration
        List<BarraMove> barraUnitsCopy = new List<BarraMove>(barraUnits);

        // Process each BarraMove unit
        foreach (BarraMove barra in barraUnitsCopy)
        {
            if (barra != null && barra.gameObject.activeInHierarchy)
            {
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

                yield return new WaitForSeconds(1f); // Delay between Barra units
            }
        }

        Debug.Log("AI's turn has ended. Starting player's turn.");
        StartPlayerTurn();
        isProcessingTurn = false;
    }


    /// <summary>
    /// Initiates the player's turn.
    /// </summary>
    public void StartPlayerTurn()
    {
        isPlayerTurn = true;

        // Reset player's fatigue to maxFatigue at the start of the turn
        playerMove.RefreshSpaceCount();
        playerFatigue.RecoverFatigue();

        Debug.Log("Player's turn has started. Fatigue reset to maximum.");
    }

    public void AddAIUnit(AIMove aiMove)
    {
        if (aiMove != null && !aiUnits.Contains(aiMove))
        {
            aiUnits.Add(aiMove);
            Debug.Log($"TempTurnBase: Added AIMove {aiMove.gameObject.name} to the turn system.");
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
            Debug.Log($"TempTurnBase: Added Barra {barraMove.gameObject.name} to the turn system.");
        }
        else
        {
            Debug.LogWarning("TempTurnBase: Attempted to add a null or already added Barra.");
        }
    }
}
