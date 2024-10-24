using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquarePlayer : MonoBehaviour
{
    public GameObject[] movementSquares; // Array to hold the four squares around the player

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Wall"))
        {
            Debug.Log("Collided with Wall: Disabling squares.");
            DisableSquares(); // Disable all squares if they collide with a wall
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Wall"))
        {
            Debug.Log("Stopped colliding with Wall: Enabling squares.");
            EnableSquares(); // Re-enable the squares when they're no longer colliding with a wall
        }
    }

    // Function to disable the squares
    private void DisableSquares()
    {
        foreach (GameObject square in movementSquares)
        {
            square.SetActive(false);
        }
    }

    // Function to enable the squares
    private void EnableSquares()
    {
        foreach (GameObject square in movementSquares)
        {
            square.SetActive(true);
        }
    }
}