using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneRestore : MonoBehaviour
{
    public GameObject player;
    public VectorValue savedPosition;
    // public DialogueTrigger cutsceneTrigger; 

    void Start()
    {
        if (TownManager.Instance.returningToScene)
        {
            player.transform.position = savedPosition.initialValue;
            // player.transform.position = TownManager.Instance.savedPlayerPosition;

            TownManager.Instance.returningToScene = false;
        }
        
    }
}
