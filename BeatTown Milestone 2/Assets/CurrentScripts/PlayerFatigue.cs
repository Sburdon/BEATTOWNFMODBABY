using UnityEngine;
using UnityEngine.UI;

public class PlayerFatigue : MonoBehaviour
{
    [Header("Fatigue Settings")]
    public int maxFatigue = 4; // Maximum amount of fatigue (4/4)
    public int currentFatigue; // Current fatigue level

    public int swingFatigueCost = 2;
    public int punchFatigueCost = 1;
    public int pushFatigueCost = 1;

    [Header("Fatigue Bar Images")]
    public Image[] fatigueImages; // Array to hold references to the fatigue images (0/4 to 4/4)

    void Start()
    {
        currentFatigue = maxFatigue; // Initialize fatigue to the maximum at the start
        UpdateFatigueBar(); // Update fatigue bar at the start
    }

    public bool CanPerformAction(int fatigueCost)
    {
        return currentFatigue >= fatigueCost;
    }

    public void UseFatigue(int fatigueCost)
    {
        currentFatigue = Mathf.Max(currentFatigue - fatigueCost, 0); // Reduce fatigue
        UpdateFatigueBar(); // Update the fatigue bar UI
        Debug.Log("Used " + fatigueCost + " fatigue. Current fatigue: " + currentFatigue);
    }

    public void RecoverFatigue()
    {
        currentFatigue = maxFatigue; // Recover fatigue
        UpdateFatigueBar(); // Update the fatigue bar UI
       
    }

    private void UpdateFatigueBar()
    {
        // Loop through the images and update them based on current fatigue
        for (int i = 0; i < fatigueImages.Length; i++)
        {
            // Enable the correct image based on current fatigue level
            fatigueImages[i].enabled = (i == currentFatigue);
        }
    }
}