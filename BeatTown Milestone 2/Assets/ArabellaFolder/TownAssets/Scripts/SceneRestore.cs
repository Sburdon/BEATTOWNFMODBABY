using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneRestore : MonoBehaviour
{
    public Transform player;
    // public VectorValue playerPosition;
    public BoolValue playReturnCutscene;
    public DialogueTrigger returnCutsceneTrigger;

    public List<GameObject> destroyOnReturn, keepActiveOnReturn;
    public GameObject returnCutscene;

    private void Start()
    {
        if (TownManager.Instance != null)
        {
            player.position = TownManager.Instance.savedPlayerPosition;
            TownManager.Instance.returningToScene = false;
        }

        if (playReturnCutscene != null && playReturnCutscene.Value)
        {
            foreach (var obj in destroyOnReturn)
            {
                obj.SetActive(false);
            }

            foreach(var obj in keepActiveOnReturn){
                obj.SetActive(true);
            }

            returnCutscene.SetActive(true);
            StartCoroutine(DelayedCutsceneTrigger());
            playReturnCutscene.Value = false;
        }
    }

    IEnumerator DelayedCutsceneTrigger()
    {
        yield return null; 

        if (returnCutsceneTrigger != null)
        {
            returnCutsceneTrigger.ResetTrigger();
            Debug.Log("Auto-playing cutscene after scene return (delayed)");
            returnCutsceneTrigger.TriggerAutomatically();
        }
    }


}
