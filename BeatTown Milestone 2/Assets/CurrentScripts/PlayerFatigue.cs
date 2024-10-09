using UnityEngine;

public class PlayerFatigue : MonoBehaviour
{
    [Header("Fatigue Settings")]
    public int maxFatigue = 5; // Maximum amount of fatigue the player can have
    public int currentFatigue; // Current fatigue level

    public int swingFatigueCost = 2; // Fatigue cost for swinging
    public int punchFatigueCost = 1; // Fatigue cost for punching

    void Start()
    {
        currentFatigue = maxFatigue; // Initialize fatigue to the maximum at the start
    }

    public bool CanPerformAction(int fatigueCost)
    {
        // Check if the player has enough fatigue to perform an action
        return currentFatigue >= fatigueCost;
    }

    public void UseFatigue(int fatigueCost)
    {
        // Reduce the player's fatigue by the specified cost
        currentFatigue = Mathf.Max(currentFatigue - fatigueCost, 0); // Ensure fatigue doesn't go below 0
        Debug.Log("Used " + fatigueCost + " fatigue. Current fatigue: " + currentFatigue);
    }

    public void RecoverFatigue(int amount)
    {
        // Recover fatigue by the specified amount
        currentFatigue = Mathf.Min(currentFatigue + amount, maxFatigue); // Ensure fatigue doesn't exceed the maximum
        Debug.Log("Recovered " + amount + " fatigue. Current fatigue: " + currentFatigue);
    }
}
