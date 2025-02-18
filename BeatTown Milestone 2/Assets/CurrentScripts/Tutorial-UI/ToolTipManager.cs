using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ToolTipManager : MonoBehaviour
{

    public static ToolTipManager _instance; // Singleton instance of the tooltip manager

    public TextMeshProUGUI textComponent; // The text component of the tooltip (drag in text box for relevant tooltip)

    private void Awake()
    {
        if (_instance != null && _instance != this)
        { // If there is already an instance of the tooltip manager, destroy this one
            Destroy(this.gameObject);
        }
        else
        { // Otherwise, set this as the instance
            _instance = this;
        }
    }

    private void Start()
    {
        Cursor.visible = true; // Make the cursor visible
        gameObject.SetActive(false); // Hide the tooltip
    }

    private void Update()
    {
        transform.position = Input.mousePosition; // Update the position of the tooltip to the mouse position
    }

    public void SetAndShowToolTip(string messsage) // Set the tooltip message and show the tooltip
    {
        gameObject.SetActive(true);
        textComponent.text = messsage;
    }

    public void HideToolTip() // Hide the tooltip
    {
        gameObject.SetActive(false);
        textComponent.text = string.Empty;
    }
}
