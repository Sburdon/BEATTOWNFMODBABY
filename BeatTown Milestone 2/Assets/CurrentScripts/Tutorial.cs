using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tutorial : MonoBehaviour
{
     Animator animtut;

    private void Start()
    {
        animtut = GetComponent<Animator>();


    }

    void ChangeAnimation()
    {
        animtut.SetInteger("Change", animtut.GetInteger("Change") + 1);
    }


    private void Update()
    {
        if (Input.anyKeyDown)
        {
            ChangeAnimation();
        }
    }
}
