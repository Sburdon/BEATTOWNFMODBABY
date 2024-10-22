//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.SceneManagement;

//public class UIButtons : MonoBehaviour
//{
//    // This function will load the scene named "New Beat Town"
//    public void LoadNextScene()
//    {
//        // Get the current scene's index
//        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
//        // Load the next scene (make sure there is a next scene in the build settings)
//        SceneManager.LoadScene(currentSceneIndex + 1);
//    }

//    // This function will quit the application
//    public void QuitGame()
//    {
//        UnityEditor.EditorApplication.isPlaying = false;

//        Application.Quit();
//    }
//    public void ResetToSceneZero()
//    {
//        SceneManager.LoadScene(0);  // Loads scene with build index 0
//    }
//}