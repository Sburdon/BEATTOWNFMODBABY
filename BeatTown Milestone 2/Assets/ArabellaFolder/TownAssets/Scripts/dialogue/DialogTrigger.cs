using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DialogueTrigger : MonoBehaviour
{
    public bool isCharacterDialogue = false;
    public string[] lines;

    public string speakerName;
    public Sprite speakerPortrait;

    public bool cutsceneTrigger = false;
    public GameObject cutsceneObject;
    public ScreenFader screenFader;

    public Animator fishAnimator;
    public GameObject contextClue;

    public DialogController dc; // reference to the dialogue controller
    private bool playerInRange = false;
    private bool triggered = false;

    private void Start()
    {
        contextClue.SetActive(false);
    }

    private void Update()
    {
        if (playerInRange && !dc.IsDialogueActive() && Input.GetKeyDown(KeyCode.Space))
        {
            triggered = true;
            playerInRange = false;
            contextClue.SetActive(false);

            if (isCharacterDialogue)
                dc.SetSpeakerInfo(speakerName, speakerPortrait);

            dc.StartDialogue(lines, isCharacterDialogue);

            if (cutsceneTrigger)
                StartCoroutine(WaitForDialogueThenCutscene());
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!triggered && other.CompareTag("Player"))
        {
            playerInRange = true;
            contextClue.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            contextClue.SetActive(false);
        }
    }

    IEnumerator WaitForDialogueThenCutscene()
    {
        while (dc.IsDialogueActive())
            yield return null;

        yield return screenFader.FadeOut();

        if (cutsceneObject != null)
        {
            SceneManager.LoadScene("Scenes/Brady");
        }

        yield return screenFader.FadeIn();
    }
}
