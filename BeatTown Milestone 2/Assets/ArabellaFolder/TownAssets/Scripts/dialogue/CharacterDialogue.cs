using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CharacterDialogue : MonoBehaviour
{
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public TextMeshProUGUI speakerNameText;
    public Image speakerPortrait;

    public float typingSpeed = 0.04f;

    private DialogueLine[] dialogueLines;
    private int index;
    private bool isTyping;

    void Update()
    {
        if (dialoguePanel.activeInHierarchy && Input.GetKeyDown(KeyCode.Space))
        {
            if (isTyping)
            {
                StopAllCoroutines();
                dialogueText.text = dialogueLines[index].line;
                isTyping = false;
            }
            else
            {
                NextLine();
            }
        }
    }

    public void StartDialogue(DialogueLine[] newLines)
    {
        dialoguePanel.SetActive(true);
        dialogueLines = newLines;
        index = 0;
        DisplayCurrentLine();
        StartCoroutine(TypeLine());
    }

    void DisplayCurrentLine()
    {
        speakerNameText.text = dialogueLines[index].speakerName;
        speakerPortrait.sprite = dialogueLines[index].portrait;
    }

    IEnumerator TypeLine()
    {
        isTyping = true;
        dialogueText.text = "";
        foreach (char c in dialogueLines[index].line)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }
        isTyping = false;
    }

    void NextLine()
    {
        index++;
        if (index < dialogueLines.Length)
        {
            DisplayCurrentLine();
            StartCoroutine(TypeLine());
        }
        else
        {
            EndDialogue();
        }
    }

    void EndDialogue()
    {
        dialoguePanel.SetActive(false);
    }

    public bool IsActive()
    {
        return dialoguePanel.activeInHierarchy;
    }
}
