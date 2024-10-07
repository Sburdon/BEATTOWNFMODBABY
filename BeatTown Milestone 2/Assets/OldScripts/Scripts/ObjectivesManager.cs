using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ObjectivesManager : MonoBehaviour
{
    public static ObjectivesManager Instance { get; private set; } // Singleton pattern

    public TextMeshProUGUI punchCounterText; // Reference to the Text UI element
    private int punchCount = 0; // Counter for successful punches

    void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        UpdatePunchCounterDisplay();
    }

    // Method to increment the punch count
    public void IncrementPunchCount()
    {
        punchCount++;
        UpdatePunchCounterDisplay();
    }

    // Method to update the punch counter text UI
    private void UpdatePunchCounterDisplay()
    {
        if (punchCounterText != null)
        {
            punchCounterText.text = "Punches: " + punchCount.ToString();
        }
        else
        {
            Debug.LogError("Punch counter text is not set on ObjectivesManager.");
        }
    }
}
