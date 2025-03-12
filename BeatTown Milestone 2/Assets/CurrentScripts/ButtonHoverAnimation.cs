using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonHoverAnimation : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Animator childAnimator; // Assign the child's Animator in the Inspector

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (childAnimator != null)
        {
            childAnimator.SetTrigger("StartAnim"); // Start animation
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (childAnimator != null)
        {
            childAnimator.SetTrigger("StopAnim"); // Stop animation
        }
    }
}