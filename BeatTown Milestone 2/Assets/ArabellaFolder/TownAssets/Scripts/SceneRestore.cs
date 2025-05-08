using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneRestore : MonoBehaviour
{
    public Transform player;
    public VectorValue playerPosition;
    public BoolValue playReturnCutscene;
    public DialogueTrigger returnCutsceneTrigger;

    private void Start()
    {
        player.position = playerPosition.initialValue;

        if (playReturnCutscene.Value)
        {
            // returnCutsceneTrigger.SetActive(true);
            player.position = TownManager.Instance.savedPlayerPosition;
            returnCutsceneTrigger.TriggerAutomatically();
            playReturnCutscene.Value = false;
        }
    }
}
