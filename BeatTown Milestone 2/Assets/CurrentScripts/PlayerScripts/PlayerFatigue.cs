using System.Collections;
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
    public bool prone = false; // prone state (from Hole) for player. No visual cue YET. Referenced in PlayerMove* and Hole* scripts.
    public bool lockedMovement = false; // locked movement state (from Hole) for player.

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

    public void UseGetUpFatigue()
    {
        int fatigueCost = fallFatigueCost;
        // before subtracting fatigue, check if player has enough fatigue to get up
        if (currentFatigue < fatigueCost)
        {
            Debug.Log("Not enough fatigue to get up!");
            return;
        }
        else
        {
            currentFatigue -= fatigueCost;
            UpdateFatigueBar();
            prone = false; // WHERE IM WORKING RN (broken): set prone back to false since you used fatigue to get up 
            // HERE is where we need to add logic for lockedmovement
            lockedMovement = true;
            // plan: add check for lockedMovement in PlayerMove
            // if it ever becomes true, start the LockedMovement() coroutine
            // at end of LockedMovement(), set lockedMovement back to false
            // exit the coroutine

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
            prone = false; // set prone back to false since you used fatigue to get up
        }
        else
        {
            currentFatigue = maxFatigue; // Recover fatigue 
        }
       Debug.Log("Recovered fatigue. Current fatigue: " + currentFatigue);

        UpdateFatigueBar(); // Update the fatigue bar UI

    }
    private IEnumerator FadeOutFatigue(Image fatigueImage)
    {
        float fadeSpeed = 5f;

        yield return StartCoroutine(FadeToAlpha(fatigueImage, 0.4f, fadeSpeed)); // Fade to 40%
        yield return StartCoroutine(FadeToAlpha(fatigueImage, 1f, fadeSpeed));   // Fade back in
        yield return StartCoroutine(FadeToAlpha(fatigueImage, 0f, fadeSpeed));   // Fade out

        fatigueImage.enabled = false;
    }
    private IEnumerator FadeToAlpha(Image image, float targetAlpha, float speed)
    {
        Color color = image.color;
        float startAlpha = color.a;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * speed;
            float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            image.color = new Color(color.r, color.g, color.b, newAlpha);
            yield return null;
        }

        image.color = new Color(color.r, color.g, color.b, targetAlpha);
    }
    private void SetAlpha(Image image, float alpha)
    {
        Color c = image.color;
        image.color = new Color(c.r, c.g, c.b, alpha);
    }

    private void UpdateFatigueBar()
    {
        for (int i = 0; i < fatigueImages.Length; i++)
        {
            if (i < currentFatigue)
            {
                fatigueImages[i].enabled = true;
                SetAlpha(fatigueImages[i], 1f); // fully visible
            }
            else if (fatigueImages[i].enabled)
            {
                StartCoroutine(FadeOutFatigue(fatigueImages[i]));
            }
        }
    }
}