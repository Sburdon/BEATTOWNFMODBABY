using System.Collections;
using UnityEngine;

public class ElectricianFatigue : MonoBehaviour
{
    [Tooltip("Maximum fatigue per turn")]
    public int maxFatigue = 2; // Each fatigue allows either 2 tiles of movement or 1 panel fix
    private int currentFatigue;

    private ElectricianMove electricianMove;

    private void Awake()
    {
        electricianMove = GetComponent<ElectricianMove>();
    }

    /// <summary>
    /// Resets the Electrician's fatigue at the start of each turn.
    /// </summary>
    public void ResetFatigue()
    {
        currentFatigue = maxFatigue;
        Debug.Log($"Electrician: Fatigue reset to {currentFatigue}");
    }

    /// <summary>
    /// Tries to spend fatigue. Returns true if successful, false otherwise.
    /// </summary>
    public bool UseFatigue(int amount)
    {
        if (currentFatigue >= amount)
        {
            currentFatigue -= amount;
            Debug.Log($"Electrician used {amount} fatigue. Remaining: {currentFatigue}");
            return true;
        }
        Debug.Log("Electrician: Not enough fatigue!");
        return false;
    }

    /// <summary>
    /// Checks if the Electrician has enough fatigue left to perform an action.
    /// </summary>
    public bool HasFatigue(int amount)
    {
        return currentFatigue >= amount;
    }

    [HideInInspector]
public bool prone = false;

/// <summary>
/// Spend 1 fatigue to get up if prone.
/// </summary>
public void UseGetUpFatigue()
{
    if (prone && UseFatigue(1))
    {
        prone = false;
        Debug.Log("Electrician used 1 fatigue to get up from hole.");
    }
}

}
