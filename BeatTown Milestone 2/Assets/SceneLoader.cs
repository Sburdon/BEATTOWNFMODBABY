using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    private static SceneLoader instance;

    private void Awake()
    {
        // Singleton pattern to ensure only one instance exists
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LoadCreditsScene()
    {
        SceneManager.LoadScene("RollingCredits");
        StartCoroutine(WaitAndLoadMainMenu());
    }

    private IEnumerator WaitAndLoadMainMenu()
    {
        float waitTime = 18.5f;
        float elapsed = 0f;

        while (elapsed < waitTime)
        {
            elapsed += Time.deltaTime;

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ReturnToMainMenu();
                yield break; // Exit coroutine
            }

            yield return null;
        }

        ReturnToMainMenu();
    }

    private void ReturnToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
        Destroy(gameObject); // Destroy the SceneLoader after returning to the menu
    }
}
