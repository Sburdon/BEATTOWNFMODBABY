using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; 

public class DialogueTrigger : MonoBehaviour
{
    public bool isCharacterDialogue = false;
    /*[TextArea(3, 10)]*/ public string[] lines;

    public string speakerName;
    public Sprite speakerPortrait;

    private bool triggered = false;

    public bool cutsceneTrigger = false; 
    public GameObject cutsceneObject; 
    public ScreenFader screenFader; 

    public Animator fishAnimator; 

    public GameObject contextClue; 

    public DialogController dc; // cache reference
    private bool playerInRange = false;

    private void Start(){
        contextClue.SetActive(false);
    }

    private void Update(){
        if (playerInRange && !dc.IsDialogueActive() && Input.GetKeyDown(KeyCode.Space))
        {
            triggered = true;
            playerInRange = false;

            if (isCharacterDialogue)
            {
                dc.SetSpeakerInfo(speakerName, speakerPortrait);
            }

            dc.StartDialogue(lines, isCharacterDialogue);

            if (cutsceneTrigger)
            {
                StartCoroutine(WaitForDialogueThenCutscene(dc));
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!triggered && other.CompareTag("Player"))
        {
            playerInRange = true;
            contextClue.SetActive(true); // Show context clue
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (playerInRange && other.CompareTag("Player"))
        {
            playerInRange = false;
            
        }
        if(other.CompareTag("Player")){
            contextClue.SetActive(false); // Hide context clue
        }
    }

    IEnumerator WaitForDialogueThenCutscene(DialogController dc)
    {
        while (dc.IsDialogueActive()) // <-- You'll need to expose this!
            yield return null;

        yield return screenFader.FadeOut();

        // Trigger cutscene
        if (cutsceneObject != null)
        {
            // fishAnimator.SetTrigger("cutsceneStarted");
            // var director = cutsceneObject.GetComponent<UnityEngine.Playables.PlayableDirector>();
            // if (director != null)
            //     director.Play();
            SceneManager.LoadScene("Scenes/Brady");

        }

        // Optionally Fade In afterward
        yield return screenFader.FadeIn();
    }
}
