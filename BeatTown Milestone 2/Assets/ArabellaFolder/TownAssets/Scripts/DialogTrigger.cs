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

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!triggered && other.CompareTag("Player"))
        {
            triggered = true;

            DialogController dc = FindObjectOfType<DialogController>();

            if (isCharacterDialogue)
            {
                dc.SetSpeakerInfo(speakerName, speakerPortrait);
            }

            dc.StartDialogue(lines, isCharacterDialogue);

            if(cutsceneTrigger){
                StartCoroutine(WaitForDialogueThenCutscene(dc));
            }
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
