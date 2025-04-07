using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIButtons : MonoBehaviour
{    // This function will load the scene named "New Beat Town"
    public void LoadNextScene()
    {
        // Get the current scene's index
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        // Load the next scene (make sure there is a next scene in the build settings)
        SceneManager.LoadScene(currentSceneIndex + 1);
    }

    // This function will quit the application
    public void QuitGame()
    {
        Application.Quit();
    }
    public void ResetToSceneZero()
    {
        SceneManager.LoadScene(0);  // Loads scene with build index 0
    }

    // button method to call the credits scene
    // Use IEnumerator to stay in RollingCredits scene for 18 seconds (duration of credits) and then revert back to main menu

    public void LoadCreditsScene()
    {
        // Load the credits scene
        SceneManager.LoadScene("RollingCredits");
        // Start the coroutine to wait for 18 seconds and then load the main menu
        StartCoroutine(WaitAndLoadMainMenu());
    }


    private IEnumerator WaitAndLoadMainMenu()
    {
        // Wait for 18 seconds
        yield return new WaitForSeconds(18);
        // Load the main menu scene (assuming it's at index 0)
        // if ESC is pressed, load the main menu immediately 

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SceneManager.LoadScene("MainMenu");
        }

        SceneManager.LoadScene("MainMenu");
    }

}
