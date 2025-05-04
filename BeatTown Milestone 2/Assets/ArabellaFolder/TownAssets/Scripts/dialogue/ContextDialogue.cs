using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ContextDialogue : MonoBehaviour
{
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;

    public float typingSpeed = 0.04f;

    private string[] lines;
    private int index;
    private bool isTyping;

    private void Update()
    {
        if (dialoguePanel.activeInHierarchy && Input.GetKeyDown(KeyCode.Space))
        {
            if (isTyping)
            {
                StopAllCoroutines();
                dialogueText.text = lines[index];
                isTyping = false;
            }
            else
            {
                NextLine();
            }
        }
    }

    public void StartDialogue(string[] newLines)
    {
        dialoguePanel.SetActive(true);
        lines = newLines;
        index = 0;
        StartCoroutine(TypeLine());
    }

    IEnumerator TypeLine()
    {
        isTyping = true;
        dialogueText.text = "";
        foreach (char c in lines[index])
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }
        isTyping = false;
    }

    void NextLine()
    {
        index++;
        if (index < lines.Length)
        {
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
