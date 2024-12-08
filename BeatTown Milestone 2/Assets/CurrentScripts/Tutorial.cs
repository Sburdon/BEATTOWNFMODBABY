using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tutorial : MonoBehaviour
{
     Animator animtut;
    public Button[] barray;

    private void Start()
    {
        animtut = GetComponent<Animator>();

        // Disable button interactions
        foreach (Button b in barray)
        {
            b.interactable = false;
        }

        // start coroutiune to delay setting Time.timescale to 0
        StartCoroutine(DelayPause());
    }

    private IEnumerator DelayPause()
    {
        // Wait for 2 seconds (or any desired duration)
        yield return new WaitForSecondsRealtime(2f); // Use WaitForSecondsRealtime to account for timeScale being 1

        // Pause the game
        Time.timeScale = 0f;
    }

    void ChangeAnimation() // to cycle through tutorial animations
    {
        animtut.SetInteger("Change", animtut.GetInteger("Change") + 1);
    }

    public void Etut() // end tutorial (called in Animator)
    {
        foreach (Button b in barray)
        {
            b.interactable = true;
        }
        Time.timeScale =1f ;
    }

    private void Update() // any key advances tutorial
    {
        if (Input.anyKeyDown)
        {
            ChangeAnimation();
        }
    }


}
