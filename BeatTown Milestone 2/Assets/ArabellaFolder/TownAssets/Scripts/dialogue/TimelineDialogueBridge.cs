using UnityEngine;
using UnityEngine.Playables;
using System.Collections;

public class TimelineDialogueBridge : MonoBehaviour
{
    public CharacterDialogue characterDialogue;
    public DialogueLine[] characterLines;

    public PlayableDirector director;

    public ScreenFader screenFader; 
    public string nextSceneName;    

    private bool waitingForDialogue;

    //saving position and scene
    public GameObject player;
    public VectorValue savedPosition;

    public void Awake(){
        StartCoroutine(FadeAndBeginScene());
    }

    IEnumerator FadeAndBeginScene(){
        yield return screenFader.FadeIn(); 
    }

    void Update()
    {
        if (waitingForDialogue && !characterDialogue.IsActive())
        {
            waitingForDialogue = false;

            director.Stop(); // fully stop the Timeline

            if (screenFader != null && !string.IsNullOrEmpty(nextSceneName))
            {
                StartCoroutine(FadeAndLoadScene());
            }
            else
            {
                Debug.LogWarning("Missing ScreenFader or Scene Name!");
            }
        }
    }

    public void TriggerDialogue()
    {
        if (!characterDialogue.IsActive())
        {
            characterDialogue.StartDialogue(characterLines);
            waitingForDialogue = true;
            director.Pause(); // pause timeline during dialogue
        }
    }

    IEnumerator FadeAndLoadScene()
    {
        yield return screenFader.FadeOut();
        savedPosition.initialValue = player.transform.position;
        TownManager.Instance.savedPlayerPosition = player.transform.position;
        TownManager.Instance.returningToScene = true;
        UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneName);
    }
}
