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
    public VectorValue playerPosition;
    public Transform player;
    public BoolValue playReturnCutscene;

    // public string sceneToLoad;
    public Vector2 returnPosition;
    public TownManager townManager; 

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
        // playerPosition.initialValue = returnPosition;
        townManager.savedPlayerPosition = player.transform.position;
        Debug.Log(townManager.savedPlayerPosition);
        playReturnCutscene.Value = true;
        UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneName);
    }
}
