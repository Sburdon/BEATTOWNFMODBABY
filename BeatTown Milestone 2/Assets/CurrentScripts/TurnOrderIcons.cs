using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TurnOrderIcons : MonoBehaviour
{
    public GameObject linkedEnemy;  // Reference to the corresponding enemy
    public Texture2D chosenTexture;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (linkedEnemy != null)
        {
            EnemyHealth enemyHealth = linkedEnemy.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.ShowHealthSlider(true);
            }
            else
            {
                Debug.LogError("EnemyHealth component not found on " + linkedEnemy.name);
            }
        }
        else
        {
            Debug.LogError("LinkedEnemy is null.");
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (linkedEnemy != null)
        {
            linkedEnemy.GetComponent<EnemyHealth>().ShowHealthSlider(false);
        }
    }
}