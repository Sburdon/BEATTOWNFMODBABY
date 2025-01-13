using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

[System.Serializable]
public class CutsceneElement
{
    [TextArea(3, 5)]
    public string lineText;
    public Sprite lineImage;
}

public class IntroCutscene : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text dialogueText;            // Reference to the UI Text in the scene
    public Image dialogueImage;          // Reference to the UI Image in the scene

    [Header("Cutscene Data")]
    public CutsceneElement[] cutsceneElements;  // Array of dialogue lines & images

    [Header("Typewriter Settings")]
    public float typingSpeed = 0.05f;    // Time delay between letters

    private int currentIndex = 0;        // Which line we're currently on
    private bool isTyping = false;       // Are we currently in the middle of typing text?
    private Coroutine typingCoroutine;   // So we can keep track of the active typewriter routine

    void Start()
    {
        // Start the first line automatically
        ShowNextLine();
    }

    void Update()
    {
        // Check if the user clicks or presses a key
        if (Input.GetMouseButtonDown(0)) // or Input.GetKeyDown(KeyCode.Space), etc.
        {
            // If text is currently typing, skip to the end immediately.
            if (isTyping)
            {
                // End the typing coroutine and show the full line at once
                StopCoroutine(typingCoroutine);
                dialogueText.text = cutsceneElements[currentIndex].lineText;
                isTyping = false;
            }
            else
            {
                // Move on to the next line
                ShowNextLine();
            }
        }
    }

    private void ShowNextLine()
    {
        // If we've reached the end of our cutsceneElements, we can end the cutscene here
        if (currentIndex >= cutsceneElements.Length)
        {
            Debug.Log("Cutscene ended.");
            // Optionally load a new scene or do something else
            // UnityEngine.SceneManagement.SceneManager.LoadScene("NextScene");
            return;
        }

        // Clear text for new line
        dialogueText.text = "";

        // Change image to match this line
        dialogueImage.sprite = cutsceneElements[currentIndex].lineImage;

        // Start typing the text
        typingCoroutine = StartCoroutine(TypeLine(cutsceneElements[currentIndex].lineText));
    }

    private IEnumerator TypeLine(string line)
    {
        isTyping = true;

        // Type each character one by one
        foreach (char c in line)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
        currentIndex++;
    }
}
