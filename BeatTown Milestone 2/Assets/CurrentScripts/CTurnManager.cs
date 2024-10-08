using System.Collections;
using UnityEngine;

public class CTurnManager : MonoBehaviour
{
    public PlayerMove playerMove; // Reference to the PlayerMove script

    private bool isPlayerTurn = true; // Keep track of whose turn it is

    void Start()
    {
        BeginPlayerTurn(); // Start the game with the player's turn
    }

    void Update()
    {
        if (!isPlayerTurn)
        {
            // AI's turn (placeholder for now)
            StartCoroutine(AITurn());
        }
    }

    // This function should be called when the player presses the UI button to end their turn
    public void OnEndTurnButtonPressed()
    {
        EndPlayerTurn();
    }

    private void BeginPlayerTurn()
    {
        isPlayerTurn = true;
        playerMove.RefreshSpaceCount(); // Reset movement for the player
        Debug.Log("Player's turn started.");
    }

    private void EndPlayerTurn()
    {
        isPlayerTurn = false;
        Debug.Log("Player's turn ended. AI's turn begins.");
    }

    private IEnumerator AITurn()
    {
        Debug.Log("AI Turn successful.");
        yield return new WaitForSeconds(1f); // Simulate AI taking time to act
        BeginPlayerTurn(); // After AI turn, return to player's turn
    }
}
