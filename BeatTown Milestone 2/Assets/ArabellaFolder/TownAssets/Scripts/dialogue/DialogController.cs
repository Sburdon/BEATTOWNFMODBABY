using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DialogController : MonoBehaviour
{
    [Header("UI")]
    public GameObject characterBox, contextBox;

    [Header("Contextual Dialogue")]
    public TextMeshProUGUI descriptiveText;

    [Header("Character Dialogue")]
    public TextMeshProUGUI speakerNameText;
    public TextMeshProUGUI dialogueText;
    public Image portraitImage;

    [Header("Portraits")]
    public Sprite johnSprite, fishermanSprite;

    [Header("Typing Settings")]
    public float typingSpeed = 0.02f;

    [Header("Player Script")]
    public JohnStateMachine playerScript;

    private Queue<string> dialogueLines = new Queue<string>();
    private bool isTyping = false;
    private string currentLine;
    private bool isCharacterDialogue = false;

    private string overrideSpeakerName = "";
    private Sprite overridePortrait = null;

    void Start()
    {
        characterBox.SetActive(false);
        contextBox.SetActive(false);
    }

    void Update()
    {
        if ((characterBox.activeInHierarchy || contextBox.activeInHierarchy) && Input.GetKeyDown(KeyCode.Space))
        {
            if (isTyping)
            {
                StopAllCoroutines();
                if (isCharacterDialogue)
                    dialogueText.text = currentLine;
                else
                    descriptiveText.text = currentLine;

                isTyping = false;
            }
            else
            {
                DisplayNextLine();
            }
        }
    }

    public void StartDialogue(string[] lines, bool characterMode)
    {
        if (playerScript != null)
        {
            playerScript.enabled = false;
            playerScript.animator.SetBool("moving", false);
        }

        dialogueLines.Clear();
        foreach (string line in lines)
        {
            dialogueLines.Enqueue(line);
        }

        isCharacterDialogue = characterMode;

        characterBox.SetActive(characterMode);
        contextBox.SetActive(!characterMode);

        DisplayNextLine();
    }

    public void SetSpeakerInfo(string name, Sprite portrait)
    {
        overrideSpeakerName = name;
        overridePortrait = portrait;
    }

    void DisplayNextLine()
    {
        if (dialogueLines.Count == 0)
        {
            EndDialogue();
            return;
        }

        string fullLine = dialogueLines.Dequeue();
        currentLine = fullLine;

        if (isCharacterDialogue)
        {
            string speaker = overrideSpeakerName;
            string lineText = fullLine;

            // Check if line starts with "Speaker: text"
            if (fullLine.Contains(":"))
            {
                var split = fullLine.Split(new char[] { ':' }, 2);
                speaker = split[0].Trim();
                lineText = split[1].Trim();
            }

            speakerNameText.text = speaker;
            UpdatePortrait(speaker);
            currentLine = lineText;

            StartCoroutine(TypeLine(lineText, true));
        }
        else
        {
            StartCoroutine(TypeLine(fullLine, false));
        }
    }

    IEnumerator TypeLine(string line, bool isCharacter)
    {
        isTyping = true;

        if (isCharacter)
            dialogueText.text = "";
        else
            descriptiveText.text = "";

        foreach (char letter in line)
        {
            if (isCharacter)
                dialogueText.text += letter;
            else
                descriptiveText.text += letter;

            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }

    void UpdatePortrait(string speaker)
    {
        switch (speaker)
        {
            case "John":
                portraitImage.sprite = johnSprite;
                break;
            case "Fisherman":
                portraitImage.sprite = fishermanSprite;
                break;
            default:
                if (overridePortrait != null)
                    portraitImage.sprite = overridePortrait;
                else
                    portraitImage.sprite = null;
                break;
        }
    }

    void EndDialogue()
    {
        characterBox.SetActive(false);
        contextBox.SetActive(false);

        if (playerScript != null)
        {
            playerScript.enabled = true;
        }

        overrideSpeakerName = "";
        overridePortrait = null;
    }

    public bool IsDialogueActive()
    {
        return characterBox.activeInHierarchy || contextBox.activeInHierarchy || isTyping;
    }
}
