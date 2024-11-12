using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ToolTip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public string message;

    public string actionName;
    public string actionCost;
    public string actionDescription;
    public string keywords;


    //=======================================For GameObjects==================
    /*private void OnMouseEnter()
    {
        ToolTipManager._instance.SetAndShowToolTip(message);
    }

    private void OnMouseExit()
    {
        ToolTipManager._instance.HideToolTip();
    }*/
    //=======================================For Buttons=========================
    public void OnPointerEnter(PointerEventData pointerEventData)
    { // When the mouse enters the object, show the tooltip
        ToolTipManager._instance.SetAndShowToolTip(message);
    }

    public void OnPointerExit(PointerEventData pointerEventData)
    { // When the mouse exits the object, hide the tooltip
        ToolTipManager._instance.HideToolTip();
    }
}
