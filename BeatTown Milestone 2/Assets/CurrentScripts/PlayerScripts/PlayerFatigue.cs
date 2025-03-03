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
    public int moveFatigueCost = 1;
    public int jumpFatigueCost = 2;

    public int fallFatigueCost = 1; // for falling inside a hole (prone state) 
    public bool prone = false; // prone state (from Hole) for player. No visual cue YET

    [Header("Fatigue Bar Images")]
    public Image[] fatigueImages; // Array to hold references to the fatigue images (0/4 to 4/4)

    void Start()
    {
        currentFatigue = maxFatigue; // Initialize fatigue to the maximum at the start
        UpdateFatigueBar(); // Update fatigue bar at the start
    }

    public bool CanPerformAction(int fatigueCost) // main logic to prevent anything but movement out of hole
    {
        if (prone)
        {
            return false;
        }
        else
        {
            return currentFatigue >= fatigueCost;
        }
        
    }

    public void UseFatigue(int fatigueCost) // called in PlayerMove
    {
        /*if (prone)
        {
            currentFatigue = Mathf.Max(currentFatigue - fatigueCost - 1, 0); // Reduce fatigue by +1 for prone state
            UpdateFatigueBar(); // Update the fatigue bar UI
        }
        else*/

        currentFatigue = Mathf.Max(currentFatigue - fatigueCost, 0); // Reduce fatigue
        UpdateFatigueBar(); // Update the fatigue bar UI

        Debug.Log("Used " + fatigueCost + " fatigue. Current fatigue: " + currentFatigue);
    }

    public void RecoverFatigue() // called in TempTurnBase
    {
        if (prone) // to handle case 2/2 where player starts next turn while still in hole
        {
            currentFatigue = maxFatigue - 1; // recover less fatigue for prone state
                                             // INSERT METHOD OR REF TO BEGIN LOCKEDMOVE() (1 tile movement to get out of hole)
        }
        else
        {
            currentFatigue = maxFatigue; // Recover fatigue 
        }

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