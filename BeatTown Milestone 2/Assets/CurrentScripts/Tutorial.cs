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

        foreach(Button b in barray)
        {
            b.interactable = false;
        }
        Time.timeScale = 0f;
    }

    void ChangeAnimation()
    {
        animtut.SetInteger("Change", animtut.GetInteger("Change") + 1);
    }

    public void Etut()
    {
        foreach (Button b in barray)
        {
            b.interactable = true;
        }
        Time.timeScale =1f ;
    }

    private void Update()
    {
        if (Input.anyKeyDown)
        {
            ChangeAnimation();
        }
    }


}
