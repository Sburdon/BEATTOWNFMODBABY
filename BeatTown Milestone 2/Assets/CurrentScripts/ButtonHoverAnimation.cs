using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static TMPro.Examples.ObjectSpin;

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
    private bool isHoveredOrSelected = false;
    private Coroutine blinkingCoroutine;
    public ActionType actionType;
    //public All_SFX all_SFX;


    private void Start()
    {
        tempTurnBase = FindObjectOfType<TempTurnBase>();
        playerFatigue = FindObjectOfType<PlayerFatigue>();
        swing = FindObjectOfType<Swing>();
        playerMove = FindObjectOfType<PlayerMove>();
    }

    private void Update()
    {
        if (tempTurnBase.isPlayerTurn && playerFatigue.currentFatigue > 0)
        {
            EnableButton();
        }

        if (!swing.disableSwing)
        {
            EnableSwingButton();
        }

        if (playerFatigue.currentFatigue == 0)
        {
            DisableButton();

            // Only disable move button when no fatigue AND no moves
            if (playerMove.remainingMoves == 0)
            {
                disableMove();
            }
        }

        if (!tempTurnBase.isPlayerTurn)
        {
            DisableButton();
            disableSwing();
            disableMove();
        }

        if (swing.disableSwing)
        {
            disableSwing();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (childAnimator != null)
            childAnimator.SetTrigger("StartAnim");

        // Check if the hovered button is interactable
        if (eventData.pointerEnter.TryGetComponent(out Button hoveredButton) && !hoveredButton.interactable)
            return;

        // Now handle blinking only for interactable buttons
        if (eventData.pointerEnter == moveButton.gameObject)
        {
            playerFatigue.BlinkFatigueSlots(playerFatigue.moveFatigueCost);
        }
        else if (eventData.pointerEnter == swingButton.gameObject)
        {
            playerFatigue.BlinkFatigueSlots(playerFatigue.swingFatigueCost);
        }
        else if (eventData.pointerEnter == myButton.gameObject)
        {
            playerFatigue.BlinkFatigueSlots(playerFatigue.punchFatigueCost);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (childAnimator != null)
            childAnimator.SetTrigger("StopAnim");

        playerFatigue.StopBlinking();
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