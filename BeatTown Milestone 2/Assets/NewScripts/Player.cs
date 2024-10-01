using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private bool isInRange = false;
    private HashSet<Collider2D> collidersInRange = new HashSet<Collider2D>(); // store all colliders in range

    // check if Player is in range to punch/act
    public bool IsInRange()
    {
        return collidersInRange.Count > 0; // if there are colliders in range, return true
    }


    // function for (punch) button
    public void Punch()
    {
        if (IsInRange())
        {
            Debug.Log("Punch can be thrown");
        }
        else
        {
            Debug.Log("Out of range to punch! Choose something else, bud.");
        }
    }

    // Call from range collider children
    public void NotifyRangeEntered(Collider2D enemy)
    {
        if (enemy.CompareTag("Enemy"))
        {
            collidersInRange.Add(enemy);
            Debug.Log("Player is in range of enemy");
        }
    }

    // call this when exiting the range 
    public void NotifyRangeExit(Collider2D enemy)
    {
        if (enemy.CompareTag("Enemy"))
        {
            collidersInRange.Remove(enemy);
            Debug.Log("Player is out of range of enemy");
        }
    }
}
