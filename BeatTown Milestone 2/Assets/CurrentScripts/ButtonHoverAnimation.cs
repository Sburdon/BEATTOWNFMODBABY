using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonHoverAnimation : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Animator childAnimator; // Assign the child's Animator in the Inspector
    public Button myButton;
    public Button swingButton;
    public Button moveButton;

    private TempTurnBase tempTurnBase;
    private PlayerFatigue playerFatigue;
    private PlayerMove playerMove;
    private Swing swing;


    private void Start()
    {
        tempTurnBase = FindObjectOfType<TempTurnBase>();
        playerFatigue = FindObjectOfType<PlayerFatigue>();
        swing = FindObjectOfType<Swing>();
        playerMove = FindObjectOfType<PlayerMove>();
    }

    private void Update()
    {
        if (tempTurnBase.isPlayerTurn == true && playerFatigue.currentFatigue > 0)
        {
            EnableButton();
        }
        if (swing.disableSwing == false)
        {
            EnableSwingButton();
        }
        if(playerFatigue.currentFatigue == 0)
        {
            DisableButton();
        }
        else
        {
            if(playerMove.remainingMoves == 0 && playerFatigue.currentFatigue == 0)
            {
                disableMove();//for some reason this does not work as intended
            }
            if (tempTurnBase.isPlayerTurn == false)
            {
                DisableButton();
                disableSwing();
            }
            if(swing.disableSwing == true)
            {
                disableSwing();
            }
        }
    }

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
    public void DisableButton()
    {
        myButton.interactable = false; // Disable the button and show the disabled sprite
    }

    public void EnableButton()
    {
        myButton.interactable = true; // Enable the button and show the normal sprite
        moveButton.interactable = true;
    }
    public void EnableSwingButton()
    {
        swingButton.interactable = true;
    }
    public void disableSwing()
    {
        swingButton.interactable = false;
    }
    public void disableMove()
    {
        moveButton.interactable = false;
    }

}