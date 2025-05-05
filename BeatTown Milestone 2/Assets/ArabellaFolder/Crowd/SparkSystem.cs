using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SparkSystem : MonoBehaviour
{
    public ParticleSystem sparks; 
    public float delay; 

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(PlayAnimationLoop());
    }

    private IEnumerator PlayAnimationLoop()
    {
        while (true)
        {
            delay = Random.Range(2f, 20f);
            yield return new WaitForSeconds(delay);
            sparks.Play();
            Debug.Log("sparks");

            // Wait for the animation to play (adjust this to match your animation duration)
            yield return new WaitForSeconds(0.5f); 

            sparks.Stop();
        }
    }
}
