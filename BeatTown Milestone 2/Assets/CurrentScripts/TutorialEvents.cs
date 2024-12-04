using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TutorialEvents : MonoBehaviour
{
    public static UnityEvent OnPartialPause = new UnityEvent();
    public static UnityEvent OnFullPause = new UnityEvent();
    public static UnityEvent OnResume = new UnityEvent();

}


