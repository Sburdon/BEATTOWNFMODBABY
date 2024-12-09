using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tutorial : MonoBehaviour
{
     Animator animtut;
    public Button[] barray;
    private bool isPaused = false;

    private void Start()
    {
        animtut = GetComponent<Animator>();

        // Disable button interactions
        foreach (Button b in barray)
        {
            b.interactable = false;
        }

        // start coroutiune to delay setting Time.timescale to 0
        StartCoroutine(DelayPause());
    }

    private IEnumerator DelayPause()
    {
        // Wait for 2 seconds (or any desired duration)
        yield return new WaitForSecondsRealtime(2f); // Use WaitForSecondsRealtime to account for timeScale being 1

        // Pause the game
        Time.timeScale = 0f;
        isPaused = true;
    }

    void ChangeAnimation() // to cycle through tutorial animations
    {
        animtut.SetInteger("Change", animtut.GetInteger("Change") + 1);
    }

    public void Etut() // end tutorial (called in Animator)
    {
        foreach (Button b in barray)
        {
            b.interactable = true;
        }
        Time.timeScale =1f ;
        isPaused = false;
    }

    private void Update() // any key advances tutorial
    {
        // Check if any key is pressed to advance the tutorial (exclude ESC key to avoid conflict with pause)
        if (Input.anyKeyDown && !Input.GetKeyDown(KeyCode.Escape))
        {
            ChangeAnimation();
        }

        // Check for ESC key to toggle pause state
        PauseGameOnESC();
    }

    public void PauseGameOnESC()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!isPaused)
            {
                // Disable button interactions before freezing
                foreach (Button b in barray)
                {
                    b.interactable = false;
                }
                // pause the game
                Time.timeScale = 0f;
                isPaused = true;
            }
            else
            {
                // enable button interactions 
                foreach (Button b in barray)
                {
                    b.interactable = true;
                }
                // resume the game
                Time.timeScale = 1f;
                isPaused = false;
            }
        }
    }
}
