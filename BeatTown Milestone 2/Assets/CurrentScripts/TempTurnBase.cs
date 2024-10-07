using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempTurnBase : MonoBehaviour
{
    public PlayerMove playerMove; // Reference to the PlayerMove2 script

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) // For example, pressing Space to end a turn
        {
            // Call RefreshSpaceCount on playerMove to reset movement for the next turn
            playerMove.RefreshSpaceCount();
        }
    }
}