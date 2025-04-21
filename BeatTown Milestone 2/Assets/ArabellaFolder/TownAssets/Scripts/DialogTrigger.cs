using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
            // Option A: Timeline
            var director = cutsceneObject.GetComponent<UnityEngine.Playables.PlayableDirector>();
            if (director != null)
                director.Play();

            // Option B: Animator
            // var animator = cutsceneObject.GetComponent<Animator>();
            // if (animator != null)
            //     animator.SetTrigger("StartCutscene");
        }

        // Optionally Fade In afterward
        yield return screenFader.FadeIn();
    }
}
