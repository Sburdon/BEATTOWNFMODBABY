using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Hole : MonoBehaviour
{
    [Header("ScriptRefs")]
    private ElectricianMove electricianMove;
    private GoonMove goonMove;
    private PlayerMove playerMove;
    private GameObject thingInHole; // Electrician, Goon, or Player

    [Header("References")]
    public Tilemap tilemap;


    private void Awake()
    {
        if (tilemap == null)
        {
            tilemap = FindObjectOfType<Tilemap>();
            if (tilemap == null)
                Debug.LogError($"Hole: No Tilemap found for {name}!");
        }
    }


    // Called when something enters the hole('s trigger)
    // Purpose: Set the proper reference and flag depending on what type of object hit the hole
    // Use InHole flag within respective scripts to deal with hole fatigue cost
    // use while (InHole) to keep the object in the hole until Player decides to use 1 fatigue to get them out
    // ***Enemies will default to use 1 of their fatigue

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Set the proper reference and flag depending on what type of object hit the hole
        if (collision.gameObject.CompareTag("Electrician"))
        {
            electricianMove = collision.gameObject.GetComponent<ElectricianMove>();
            thingInHole = collision.gameObject;
         //   electricianMove.Prone = true; // bool for Electrician script (not used for anything yet)
        }
        else if (collision.gameObject.CompareTag("Goon"))
        {
            goonMove = collision.gameObject.GetComponent<GoonMove>();
            thingInHole = collision.gameObject;
          //  goonMove.Prone = true; // bool for Goon script (not used for anything yet)
        }
        else if (collision.gameObject.CompareTag("Player"))
        {
            // add logic to stop Player (mid turn) from moving (until they spend 1 fatigue)
            playerMove = collision.gameObject.GetComponent<PlayerMove>();
            thingInHole = collision.gameObject;
            PlayerFatigue playerFatigue = collision.GetComponent<PlayerFatigue>();
            playerFatigue.prone = true;  // bool in PlayerFatigue that requires + 1 fatigue spent
            
        }
        else
        {
            return;
        }

    }
}
