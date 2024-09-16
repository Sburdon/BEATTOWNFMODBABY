using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Combo : MonoBehaviour
{
    public Button[] actionButtons;  // Array to hold your buttons
    private Button firstButton = null;

    // Possible combos: (Button1, Button2) => Some combo action
    private Dictionary<(Button, Button), string> comboActions = new Dictionary<(Button, Button), string>();

    void Start()
    {
        // Assign button listeners
        foreach (Button button in actionButtons)
        {
            button.onClick.AddListener(() => OnButtonPressed(button));
        }

        // Example combo setup
        comboActions.Add((actionButtons[0], actionButtons[1]), "Combo Attack 1");
        comboActions.Add((actionButtons[1], actionButtons[2]), "Combo Attack 2");
    }

    // This method is called when any button is pressed
    public void OnButtonPressed(Button pressedButton)
    {
        if (firstButton == null)
        {
            // This is the first button pressed
            firstButton = pressedButton;
            HighlightValidCombos(pressedButton);
        }
        else
        {
            // This is the second button pressed, check if it forms a combo
            if (comboActions.TryGetValue((firstButton, pressedButton), out string comboAction))
            {
                PerformCombo(comboAction);
            }
            else if (comboActions.TryGetValue((pressedButton, firstButton), out comboAction)) // Try reverse order
            {
                PerformCombo(comboAction);
            }
            else
            {
                Debug.Log("No valid combo with these buttons");
            }

            // Reset for the next combo attempt
            ResetCombo();
        }
    }

    private void HighlightValidCombos(Button selectedButton)
    {
        // Iterate through the combo dictionary and highlight valid second buttons
        foreach (var combo in comboActions)
        {
            if (combo.Key.Item1 == selectedButton || combo.Key.Item2 == selectedButton)
            {
                Button secondButton = combo.Key.Item1 == selectedButton ? combo.Key.Item2 : combo.Key.Item1;
                HighlightButton(secondButton);
            }
        }
    }

    private void HighlightButton(Button button)
    {
        // You can change button appearance to highlight it
        button.GetComponent<Image>().color = Color.yellow;
    }

    private void ResetHighlight()
    {
        // Reset button highlight (for all buttons)
        foreach (Button button in actionButtons)
        {
            button.GetComponent<Image>().color = Color.white;
        }
    }

    private void PerformCombo(string comboAction)
    {
        Debug.Log($"Performing {comboAction}");
        // Trigger the corresponding combo attack here
    }

    private void ResetCombo()
    {
        firstButton = null;
        ResetHighlight();
    }
}
