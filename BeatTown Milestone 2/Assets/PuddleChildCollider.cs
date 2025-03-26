using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuddleChildCollider : MonoBehaviour
{
    private Puddle parentPuddle;
    private Vector3Int slideDirection;


    private void Start()
    {
        Debug.Log($"Child collider initialized: {name}", this);
        if (parentPuddle == null) Debug.LogError("Parent Puddle not assigned!", this);
    }
    public void Initialize(Puddle puddle, Vector3Int direction)
    {
        // set puddle equal to parent of this child collider
        parentPuddle = puddle;
        slideDirection = direction;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // debug which child is being triggered, as well as the object that triggered it

        Debug.Log("Child collider triggered: " + gameObject.name + " by " + other.name);

        if (other.transform.root.CompareTag("Player") || other.CompareTag("Electrician") || other.CompareTag("Goon")) // 'root' to compare tag of parent object
        {
            parentPuddle.OnObjectEntered(other.transform, slideDirection);
        }
    }
}
