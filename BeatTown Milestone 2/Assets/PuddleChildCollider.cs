using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuddleChildCollider : MonoBehaviour
{
    private Puddle parentPuddle;
    private Vector3Int slideDirection;

    public void Initialize(Puddle puddle, Vector3Int direction)
    {
        parentPuddle = puddle;
        slideDirection = direction;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // debug which child is being triggered, as well as the object that triggered it

        Debug.Log("Child collider triggered: " + gameObject.name + " by " + other.name);
        if (other.CompareTag("Player") || other.CompareTag("Electrician") || other.CompareTag("Goon"))
        {
            parentPuddle.OnObjectEntered(other.transform, slideDirection);
        }
    }
}
