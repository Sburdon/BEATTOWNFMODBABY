using UnityEngine;
using UnityEngine.SceneManagement; // Required for scene management

public class LevelSwitcher : MonoBehaviour
{
    // Function to load a scene by name
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
