using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResetButtonScene : MonoBehaviour
{
    public Button pauseButton;   // Drag your button here in the Inspector
    private bool isPaused = false;

    void Start()
    {
        // Ensure the button is hidden at the start of the game
        pauseButton.gameObject.SetActive(false);
    }

    void Update()
    {
        // Check if the Esc key is pressed
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    // This method resumes the game
    public void ResumeGame()
    {
        pauseButton.gameObject.SetActive(false);  // Hide the button
        Time.timeScale = 1f;                      // Resume game time
        isPaused = false;
    }

    // This method pauses the game
    public void PauseGame()
    {
        pauseButton.gameObject.SetActive(true);   // Show the button
        Time.timeScale = 0f;                      // Freeze game time
        isPaused = true;
    }
}