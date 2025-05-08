using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Playables;

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
            PlayableDirector director = cutsceneObject.GetComponent<PlayableDirector>();
            if (director != null)
            {
                director.Play();
                yield return screenFader.FadeIn();
            }
        }
    }

    public void TriggerAutomatically()
    {
        triggered = true;

        if (isCharacterDialogue)
        {
            characterDialogue.StartDialogue(characterLines);

            if (cutsceneTrigger)
                StartCoroutine(WaitForDialogueThenCutscene());
        }
        else
        {
            contextualDialogue.StartDialogue(contextualLines);
        }
    }

}
