using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScreenFader : MonoBehaviour
{
    public Image fadePanel; 
    public float fadeDuration = 1f;

    public IEnumerator FadeOut(){
        Color color = fadePanel.color; 
        for(float t = 0; t < 1; t += Time.deltaTime / fadeDuration){
            color.a = t; 
            fadePanel.color = color; 
            yield return null; 
        }
        color.a = 1;
        fadePanel.color = color;
    }

    public IEnumerator FadeIn()
    {
        Color color = fadePanel.color;
        for (float t = 1; t > 0; t -= Time.deltaTime / fadeDuration)
        {
            color.a = t;
            fadePanel.color = color;
            yield return null;
        }
        color.a = 0;
        fadePanel.color = color;
    }
}
