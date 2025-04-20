using System.Collections;
using UnityEngine;

public class BubbleAnimation : MonoBehaviour
{
    public Animator animator;
    public float delay;

    void Start()
    {
        animator = GetComponent<Animator>();
        StartCoroutine(PlayAnimationLoop());
    }

    private IEnumerator PlayAnimationLoop()
    {
        while (true)
        {
            delay = Random.Range(4f, 10f);
            yield return new WaitForSeconds(delay);
            animator.SetBool("canPlay", true);

            // Wait for the animation to play (adjust this to match your animation duration)
            yield return new WaitForSeconds(2f); // replace 1f with the length of your animation

            animator.SetBool("canPlay", false);
        }
    }
}
