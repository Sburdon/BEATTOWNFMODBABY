using UnityEngine;
using System.Collections;

public class AIFatigue : MonoBehaviour
{
    [Header("Fatigue Settings")]
    public int maxFatigue = 2; // Total fatigue per turn

    private int currentFatigue;
    private AIMove aiMove;
    private AIAttack aiAttack;

    void Start()
    {
        aiMove = GetComponent<AIMove>();
        aiAttack = GetComponent<AIAttack>();

        if (aiMove == null)
        {
            Debug.LogError("AIFatigue requires an AIMove component on the same GameObject.");
        }

        if (aiAttack == null)
        {
            Debug.LogError("AIFatigue requires an AIAttack component on the same GameObject.");
        }
    }

    /// <summary>
    /// Handles the AI's turn based on its current fatigue.
    /// </summary>
    public IEnumerator HandleTurn()
    {
        currentFatigue = maxFatigue;
        Debug.Log($"AI {gameObject.name} starting turn with {currentFatigue} fatigue.");

        while (currentFatigue > 0)
        {
            bool actionTaken = false;

            // Attempt to attack if possible
            if (aiAttack != null && aiAttack.attackDamage > 0 && IsPlayerInRange())
            {
                aiAttack.AttackIfInRange();
                currentFatigue -= 1;
                actionTaken = true;
                Debug.Log($"AI {gameObject.name} attacked the player. Remaining fatigue: {currentFatigue}");

                // Wait for 1 second between actions
                yield return new WaitForSeconds(0.1f);
            }

            // If no attack was made, attempt to move
            if (!actionTaken && aiMove != null)
            {
                yield return StartCoroutine(aiMove.MoveAction());

                currentFatigue -= 1;
                Debug.Log($"AI {gameObject.name} moved. Remaining fatigue: {currentFatigue}");

                // Wait for 1 second between actions
                yield return new WaitForSeconds(0.1f);
            }

            // If AI cannot perform any actions, break to prevent infinite loop
            if (!actionTaken && aiMove == null)
            {
                Debug.LogWarning($"AI {gameObject.name} cannot perform any actions.");
                break;
            }

            // If attackDamage is 0, ensure all fatigue is spent on moving
            if (aiAttack != null && aiAttack.attackDamage == 0 && aiMove != null && currentFatigue > 0)
            {
                yield return StartCoroutine(aiMove.MoveAction());
                currentFatigue -= 1;
                Debug.Log($"AI {gameObject.name} moved (attackDamage=0). Remaining fatigue: {currentFatigue}");

                // Wait for 1 second between actions
                yield return new WaitForSeconds(0.1f);
            }
        }

        Debug.Log($"AI {gameObject.name} has completed its turn.");
        yield return null;
    }

    /// <summary>
    /// Determines if the player is within attack range.
    /// </summary>
    /// <returns>True if the player is in range; otherwise, false.</returns>
    private bool IsPlayerInRange()
    {
        if (aiAttack == null || aiAttack.playerHealth == null)
            return false;

        float distanceToPlayer = Vector3.Distance(transform.position, aiAttack.playerHealth.transform.position);
        return distanceToPlayer <= aiAttack.attackRange;
    }
}
