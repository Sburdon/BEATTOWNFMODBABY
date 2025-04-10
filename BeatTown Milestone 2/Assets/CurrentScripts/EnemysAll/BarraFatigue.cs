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

            // Check if we can attack from current position
            if (barraAttack != null && barraAttack.CanAttack())
            {
                yield return StartCoroutine(barraAttack.PerformAttack());
                currentFatigue--;
                actionTaken = true;

                yield return new WaitForSeconds(1f); // Brief delay after each action
            }
            // If not in range, try moving toward closest target
            else if (barraMove != null)
            {
                yield return StartCoroutine(barraMove.PerformMove());
                currentFatigue--;
                actionTaken = true;

                yield return new WaitForSeconds(1f); // Brief delay after each action

                // After moving, try attacking again
                if (currentFatigue > 0 && barraAttack.CanAttack())
                {
                    yield return StartCoroutine(barraAttack.PerformAttack());
                    currentFatigue--;
                    yield return new WaitForSeconds(1f);
                }
            }

            // If no actions were taken (e.g., blocked), break to end turn early
            if (!actionTaken) break;
        }

        Debug.Log($"{gameObject.name} has ended its turn.");
        yield return null;
    }
}
