using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DialogueTrigger : MonoBehaviour
{
    public bool isCharacterDialogue = false;
    public DialogueLine[] characterLines;
    public string[] contextualLines;

    public bool cutsceneTrigger = false;
    public GameObject cutsceneObject;
    public ScreenFader screenFader;

    public Animator fishAnimator;
    public GameObject contextClue;

    private bool playerInRange = false;
    private bool triggered = false;

    public CharacterDialogue characterDialogue;
    public ContextDialogue contextualDialogue;

    private void Start()
    {
        contextClue.SetActive(false);
    }

    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.Space))
        {
            if (isCharacterDialogue)
            {
                if (!characterDialogue.IsActive() && !triggered)
                {
                    triggered = true;
                    playerInRange = false;
                    contextClue.SetActive(false);

                    characterDialogue.StartDialogue(characterLines);

                    if (cutsceneTrigger)
                        StartCoroutine(WaitForDialogueThenCutscene());
                }
            }
            else
            {
                if (!contextualDialogue.IsActive())
                {
                    playerInRange = false;
                    contextClue.SetActive(false);

                    contextualDialogue.StartDialogue(contextualLines);
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (!triggered || !isCharacterDialogue)
            {
                playerInRange = true;
                contextClue.SetActive(true);
            }
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
        while (characterDialogue.IsActive())
            yield return null;

        yield return screenFader.FadeOut();

        if (cutsceneObject != null)
        {
            SceneManager.LoadScene("Scenes/Brady");
        }

        // // yield return screenFader.FadeIn();
        // yield return new WaitUntil(() => !characterDialogue.IsActive());

        // // Start the fade
        // yield return fadeController.FadeOut(); // assumes this returns IEnumerator

        // // Then start the cutscene
        // cutsceneManager.PlayCutscene(); // or trigger Timeline, animation, etc.

        // // After cutscene finishes, load next scene
        // yield return new WaitForSeconds(cutsceneManager.cutsceneLength);
        // SceneManager.LoadScene("Scenes/Brady"); // Replace with your actual scene
    }
}
