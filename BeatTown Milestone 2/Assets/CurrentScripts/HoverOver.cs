using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class HoverOver : MonoBehaviour
{
    [SerializeField] private GameObject uiElementA; // UI element for Enemy hover
    [SerializeField] private GameObject uiElementB; // UI element for Barra hover

    private GameObject lastHoveredEnemy;
    private GameObject lastHoveredBarra;

    private void Update()
    {
        PointerEventData pointerData = new PointerEventData(EventSystem.current);
        pointerData.position = Input.mousePosition;

        // Use both Physics2D raycast (for world units) and EventSystem raycast (for UI)
        RaycastHit2D[] worldHits = Physics2D.RaycastAll(Camera.main.ScreenToWorldPoint(pointerData.position), Vector2.zero);
        List<RaycastResult> uiHits = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, uiHits);

        bool hoverOverEnemy = false;
        bool hoverOverBarra = false;

        // Check for world units (actual enemies and Barra in the game world)
        foreach (RaycastHit2D hit in worldHits)
        {
            if (hit.collider != null)
            {
                if (hit.collider.CompareTag("Enemy"))
                {
                    HandleHover(hit.collider.gameObject, "Enemy", ref hoverOverEnemy, ref lastHoveredEnemy);
                }
                else if (hit.collider.CompareTag("Barra"))
                {
                    HandleHover(hit.collider.gameObject, "Barra", ref hoverOverBarra, ref lastHoveredBarra);
                }
            }
        }

        // Check for UI elements (turn order icons)
        foreach (RaycastResult result in uiHits)
        {
            if (result.gameObject.CompareTag("Enemy"))
            {
                TurnOrderIcons iconScript = result.gameObject.GetComponent<TurnOrderIcons>();
                if (iconScript != null && iconScript.linkedEnemy != null)
                {
                    HandleHover(iconScript.linkedEnemy, "Enemy", ref hoverOverEnemy, ref lastHoveredEnemy);
                }
            }
            else if (result.gameObject.CompareTag("Barra"))
            {
                TurnOrderIcons iconScript = result.gameObject.GetComponent<TurnOrderIcons>();
                if (iconScript != null && iconScript.linkedEnemy != null)
                {
                    HandleHover(iconScript.linkedEnemy, "Barra", ref hoverOverBarra, ref lastHoveredBarra);
                }
            }
        }

        // Hide health sliders if not hovering over anything
        if (!hoverOverEnemy && lastHoveredEnemy != null)
        {
            HideAllSlidersWithTag("Enemy");
            lastHoveredEnemy = null;
        }

        if (!hoverOverBarra && lastHoveredBarra != null)
        {
            HideAllSlidersWithTag("Barra");
            lastHoveredBarra = null;
        }

        // Update UI elements visibility
        uiElementA.SetActive(hoverOverEnemy);
        uiElementB.SetActive(hoverOverBarra);
    }

    private void HandleHover(GameObject unit, string tag, ref bool hoverFlag, ref GameObject lastHovered)
    {
        hoverFlag = true;

        // Only update if hovering over a different unit
        if (lastHovered != unit)
        {
            HideAllSlidersWithTag(tag);
            EnemyHealth healthScript = unit.GetComponent<EnemyHealth>();
            if (healthScript != null)
            {
                healthScript.ShowHealthSlider(true);
            }
            lastHovered = unit;
        }
    }

    private void HideAllSlidersWithTag(string tag)
    {
        GameObject[] units = GameObject.FindGameObjectsWithTag(tag);
        foreach (GameObject unit in units)
        {
            EnemyHealth health = unit.GetComponent<EnemyHealth>();
            if (health != null)
            {
                health.ShowHealthSlider(false);
            }
        }
    }
}
