using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class HoverOver : MonoBehaviour
{
    [SerializeField] private GameObject uiElementA; // UI element for Enemy hover
    [SerializeField] private GameObject uiElementB; // UI element for Barra hover

    private void Update()
    {
        // Create a PointerEventData object and set the mouse position
        PointerEventData pointerData = new PointerEventData(EventSystem.current);
        pointerData.position = Input.mousePosition;

        // Raycast to check if the mouse is hovering over any unit icon
        RaycastHit2D[] hits = Physics2D.RaycastAll(Camera.main.ScreenToWorldPoint(pointerData.position), Vector2.zero);

        bool hoverOverEnemy = false;
        bool hoverOverBarra = false;

        // Check for Enemy or Barra hover and show health slider if applicable
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider != null)
            {
                if (hit.collider.CompareTag("Enemy"))
                {
                    hoverOverEnemy = true;
                    // Show health slider if the icon has a linked enemy
                    TurnOrderIcons iconScript = hit.collider.GetComponent<TurnOrderIcons>();
                    if (iconScript != null && iconScript.linkedEnemy != null)
                    {
                        iconScript.linkedEnemy.GetComponent<EnemyHealth>().ShowHealthSlider(true);
                    }
                }
                else if (hit.collider.CompareTag("Barra"))
                {
                    hoverOverBarra = true;
                    // Similar handling for Barra if needed
                    TurnOrderIcons iconScript = hit.collider.GetComponent<TurnOrderIcons>();
                    if (iconScript != null && iconScript.linkedEnemy != null)
                    {
                        iconScript.linkedEnemy.GetComponent<EnemyHealth>().ShowHealthSlider(true);
                    }
                }
            }
        }

        // Turn off health sliders if no longer hovering
        if (!hoverOverEnemy)
        {
            HideAllSlidersWithTag("Enemy");
        }
        if (!hoverOverBarra)
        {
            HideAllSlidersWithTag("Barra");
        }

        // Update UI elements visibility based on hover
        uiElementA.SetActive(hoverOverEnemy);
        uiElementB.SetActive(hoverOverBarra);
    }

    private void HideAllSlidersWithTag(string tag)
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(tag);
        foreach (GameObject enemy in enemies)
        {
            EnemyHealth health = enemy.GetComponent<EnemyHealth>();
            if (health != null)
            {
                health.ShowHealthSlider(false);
            }
        }
    }
}