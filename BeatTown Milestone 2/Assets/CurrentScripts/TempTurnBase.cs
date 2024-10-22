using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TempTurnBase : MonoBehaviour
{
    public List<AIMove> aiUnits;
    public List<BarraAI> barraUnits; // List for BarraAI units

    public PlayerMove playerMove;
    public PlayerFatigue playerFatigue;

    private bool isPlayerTurn = true;

    void Start()
    {
        // Initialization...
    }

    void Update()
    {
        if (isPlayerTurn)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                EndPlayerTurn();
            }
        }
    }

    public void EndPlayerTurn()
    {
        isPlayerTurn = false;
        StartCoroutine(AITurnRoutine());
    }

    private IEnumerator AITurnRoutine()
    {
        // AI units' turns
        foreach (AIMove ai in aiUnits)
        {
            if (ai != null)
            {
                ai.TakeTurn();
                yield return new WaitForSeconds(0.5f);

                // Check if the AI has an attack component
                AIAttack aiAttack = ai.GetComponent<AIAttack>();
                if (aiAttack != null)
                {
                    aiAttack.AttackIfInRange();
                    yield return new WaitForSeconds(0.5f);
                }
            }
        }

        // BarraAI units' turns
        foreach (BarraAI barra in barraUnits)
        {
            if (barra != null)
            {
                barra.TakeTurn();
                yield return new WaitForSeconds(0.5f);
            }
        }

        // Start player's turn
        StartPlayerTurn();
    }

    public void StartPlayerTurn()
    {
        isPlayerTurn = true;
        // Reset player's fatigue to maxFatigue at the start of the turn
        playerMove.RefreshSpaceCount();
        playerMove.ResetMoveCount();
        Debug.Log("Player's turn has started. Fatigue reset to maximum.");
    }

    // Method to add BarraAI units to the list
    public void AddBarraUnit(BarraAI barraAI)
    {
        if (!barraUnits.Contains(barraAI))
        {
            barraUnits.Add(barraAI);
        }
    }
}
