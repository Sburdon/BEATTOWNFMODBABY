using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenuUI;
    public Button[] barray;
    private bool isPaused = false;

    private void Update()
    {
        // Check for ESC key to toggle pause state
        PauseGameOnESC();
    }

    public void PauseGameOnESC()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !isPaused)
        {
         
            foreach (Button b in barray)
            {
                b.interactable = false;
                Debug.Log("Button " + b.name + " is now unclickable");
            }

            // show the pause menu
            pauseMenuUI.SetActive(true);
            // pause the game
            Time.timeScale = 0f;
            isPaused = true;

        }

        else if (Input.GetKeyDown(KeyCode.Escape) && isPaused)
        {
            foreach (Button b in barray)
            {
        //        b.interactable = true;
            }
            // hide the pause menu
            pauseMenuUI.SetActive(false);
            // resume the game
            Time.timeScale = 1f;
            isPaused = false;
        }
    }

}
