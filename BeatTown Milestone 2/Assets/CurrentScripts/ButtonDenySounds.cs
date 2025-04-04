using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonDenySounds : MonoBehaviour
{
    private PointerEventData pointerEventData;
    private EventSystem eventSystem;
    private All_SFX all_SFX;

    void Start()
    {
        eventSystem = EventSystem.current;
        all_SFX = FindObjectOfType<All_SFX>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            pointerEventData = new PointerEventData(eventSystem)
            {
                position = Input.mousePosition
            };

            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerEventData, results);

            foreach (var result in results)
            {
                Button button = result.gameObject.GetComponent<Button>();
                if (button != null && !button.interactable)
                {
                    // Play the deny sound if the clicked button is not interactable
                    if (all_SFX != null)
                        all_SFX.PlayUIDENY();
                    break;
                }
            }
        }
    }
}