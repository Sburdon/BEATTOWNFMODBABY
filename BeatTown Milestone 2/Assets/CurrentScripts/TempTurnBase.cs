using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempTurnBase : MonoBehaviour
{
    public PlayerMove playerMove; // Reference to the PlayerMove script
    public List<AIMove> aiUnits;  // List of AI units in the scene
    public List<AIAttack> aiAttackUnits;  // List of AI attack units in the scene
    private bool playerTurn = true;

    void Update()
    {
        if (playerTurn)
        {
            if (Input.GetKeyDown(KeyCode.Space)) // Press Space to end player's turn
            {
                playerTurn = false;
                playerMove.RefreshSpaceCount();
                StartAITurn();
            }
        }
    }

    void StartAITurn()
    {
        StartCoroutine(AITurnRoutine());
    }

    IEnumerator AITurnRoutine()
    {
        for (int i = 0; i < aiUnits.Count; i++)
        {
            // AI Move Logic
            AIMove aiMove = aiUnits[i];
            aiMove.TakeTurn();

            // AI Attack Logic
            AIAttack aiAttack = aiAttackUnits[i];
            aiAttack.TakeTurn();

            yield return new WaitForSeconds(1f); // Delay between each AI action for clarity
        }

        EndAITurn();
    }

    void EndAITurn()
    {
        playerTurn = true;
        Debug.Log("AI turn has ended. Player's turn begins.");
    }
}
