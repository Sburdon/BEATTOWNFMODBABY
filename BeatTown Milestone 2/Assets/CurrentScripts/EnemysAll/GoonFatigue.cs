using System.Collections;
using UnityEngine;

[RequireComponent(typeof(GoonMove))]
public class GoonFatigue : MonoBehaviour
{
    [Tooltip("Maximum fatigue (actions) the Goon can take each turn.")]
    public int maxFatigue = 2;

    private int currentFatigue;
    private GoonMove goonMove;

    private void Awake()
    {
        goonMove = GetComponent<GoonMove>();
    }

    /// <summary>
    /// Resets current fatigue to the maximum at the start of each turn.
    /// </summary>
    public void ResetFatigue()
    {
        currentFatigue = maxFatigue;
    }

    /// <summary>
    /// Called by your turn manager or AI controller each time
    /// the Goon takes a turn. Each fatigue point can be used for:
    ///   - Move (up to 2 tiles), OR
    ///   - Punch if adjacent.
    /// </summary>
    public IEnumerator HandleTurn()
    {
        // If the Goon or references are missing, just stop
        if (goonMove == null)
        {
            Debug.LogWarning("GoonFatigue: Missing GoonMove reference!");
            yield break;
        }

        // 1) Refresh fatigue to maximum
        ResetFatigue();
        Debug.Log($"GoonFatigue: Starting turn with {currentFatigue} fatigue.");

        // 2) While we still have fatigue, perform actions
        while (currentFatigue > 0)
        {
            // If Goon is dead (optional check), stop
            if (goonMove.enemyHealth != null && goonMove.enemyHealth.IsDead)
            {
                Debug.Log("GoonFatigue: Goon is dead, ending turn.");
                yield break;
            }

            // Check adjacency
            if (goonMove.IsAdjacentToElectrician())
            {
                // If already adjacent, punch
                goonMove.SetupPunch();
                Debug.Log($"GoonFatigue: Used 1 fatigue to Punch. Remaining: {currentFatigue - 1}");
                currentFatigue--;
            }
            else
            {
                // Otherwise, spend 1 fatigue to move UP TO 2 tiles
                // MoveUsingFatigue(1) => BFS sees "1" and allows up to 2 steps
                Debug.Log($"GoonFatigue: Attempting to move up to 2 tiles. Fatigue before move: {currentFatigue}");
                yield return StartCoroutine(goonMove.MoveUsingFatigue(1));
                currentFatigue--;
                Debug.Log($"GoonFatigue: Used 1 fatigue to Move. Remaining: {currentFatigue}");
            }
        }

        Debug.Log("GoonFatigue: Turn complete. No more fatigue left.");
        yield return null;
    }
}
