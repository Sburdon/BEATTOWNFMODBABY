using System.Collections;
using UnityEngine;

public class BarraFatigue : MonoBehaviour
{
    [Header("Fatigue Settings")]
    public int maxFatigue = 2;  // Number of actions per turn
    private int currentFatigue;

    private BarraMove barraMove;
    private BarraAttack barraAttack;

    void Start()
    {
        barraMove = GetComponent<BarraMove>();
        barraAttack = GetComponent<BarraAttack>();

        if (barraMove == null || barraAttack == null)
        {
            Debug.LogError("BarraFatigue requires both BarraMove and BarraAttack components.");
        }
    }

    /// <summary>
    /// Handles Barra's turn based on available fatigue.
    /// </summary>
    public IEnumerator HandleTurn()
    {
        Debug.Log($"{gameObject.name} is starting its turn.");

        currentFatigue = maxFatigue; // Reset fatigue at the start of each turn

        while (currentFatigue > 0)
        {
            bool actionTaken = false;

            // Attempt to attack if possible
            if (barraAttack != null && barraAttack.CanAttack())
            {
                yield return StartCoroutine(barraAttack.PerformAttack());
                currentFatigue--;
                actionTaken = true;

                yield return new WaitForSeconds(1f); // Brief delay after each action
            }

            // If no attack was possible, attempt to move
            if (!actionTaken && barraMove != null)
            {
                yield return StartCoroutine(barraMove.PerformMove());
                currentFatigue--;
                actionTaken = true;

                yield return new WaitForSeconds(1f); // Brief delay after each action
            }

            // If no actions were taken (e.g., blocked), break the loop to end the turn
            if (!actionTaken) break;
        }

        Debug.Log($"{gameObject.name} has ended its turn.");
        yield return null;
    }
}
