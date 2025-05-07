using UnityEngine;

public class TownManager : MonoBehaviour
{
    public static TownManager Instance;

    public Vector3 savedPlayerPosition;
    public bool returningToScene = false;

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist between scenes
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
