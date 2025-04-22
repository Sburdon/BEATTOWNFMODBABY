using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class WinLoseButtons : MonoBehaviour
{
    private void Start()
    {
        // set instance of game manager

    }
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


    public void PlayAgain()
    {
        // reload Brady scene 
        SceneManager.LoadScene("Brady");

    }
}
