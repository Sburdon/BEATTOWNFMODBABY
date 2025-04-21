using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TypewriterEffect : MonoBehaviour
{
    public TextMeshProUGUI dialogueText;
    public float typingSpeed = 0.05f;
    public string[] sentences;
    private int index = 0;
    private Coroutine typingCoroutine;
    private bool isTyping = false;
    // public bool contextTriggered;

    void Start()
    {
        StartDialogue();
        // this.contextTriggered = true;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isTyping)
            {
                // Skip to full sentence
                StopCoroutine(typingCoroutine);
                dialogueText.text = sentences[index];
                isTyping = false;
            }
            else
            {
                NextSentence();
            }
        }
    }

    public void StartDialogue()
    {
        index = 0;
        ShowSentence();
    }

    void ShowSentence()
    {
        typingCoroutine = StartCoroutine(TypeSentence(sentences[index]));
    }

    IEnumerator TypeSentence(string sentence)
    {
        isTyping = true;
        dialogueText.text = "";
        foreach (char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
        isTyping = false;
    }

    void NextSentence()
    {
        if (index < sentences.Length - 1)
        {
            index++;
            ShowSentence();
        }
        else
        {
            // Dialogue is over
            gameObject.SetActive(false); // Or hide panel, trigger event, etc.
        }
    }
}
