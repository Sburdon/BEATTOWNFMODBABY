using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenuUI;
    [SerializeField] private GameObject pauseOptionsUI;

    public Button[] barray;
    public bool isPaused = false;

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
                b.interactable = true;
            }
            // hide the pause menu
            pauseMenuUI.SetActive(false);
            // resume the game
            Time.timeScale = 1f;
            isPaused = false;
        }
    }


    // resume method for button
    public void Resume()
    {
        foreach (Button b in barray)
        {
            b.interactable = true;
        }
        // hide the pause menu
        pauseMenuUI.SetActive(false);
        // resume the game
        Time.timeScale = 1f;
        isPaused = false;
    }

    // main menu method for button 

    public void MainMenu()
    {
        Time.timeScale = 1f; // Resume the game before loading the main menu
        // swap scene by name "MainMenu" 
        SceneManager.LoadScene("MainMenu");
    }

    // quit method for button

    public void Quit()
    {
        // quit the game
        Application.Quit();
    }


    // options method for button 

    public void Options()
    {
        // hide the pause menu
        pauseMenuUI.SetActive(false);

        // show the options menu
        pauseOptionsUI.SetActive(true);

    }

}
