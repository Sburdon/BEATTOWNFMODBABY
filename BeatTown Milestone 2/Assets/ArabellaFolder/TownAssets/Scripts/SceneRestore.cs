using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneRestore : MonoBehaviour
{
    public Transform player;
    // public VectorValue playerPosition;
    public BoolValue playReturnCutscene;
    public DialogueTrigger returnCutsceneTrigger;

    public List<GameObject> destroyOnReturn;
    public GameObject returnCutscene;

    private void Start()
    {
        // Only use TownManager's saved position
        if (TownManager.Instance != null)
        {
            player.position = TownManager.Instance.savedPlayerPosition;
            TownManager.Instance.returningToScene = false;
        }

        if (playReturnCutscene != null && playReturnCutscene.Value)
            {
                foreach(var obj in destroyOnReturn){
                obj.SetActive(false);
            }
            returnCutscene.SetActive(true);
            Debug.Log("Auto-playing cutscene after scene return");
            returnCutsceneTrigger.TriggerAutomatically();
            playReturnCutscene.Value = false;
        }
    }
}
