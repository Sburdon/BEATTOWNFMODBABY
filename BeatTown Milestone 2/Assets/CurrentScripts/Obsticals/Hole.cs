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
    private GameObject thingInHole; // Electrician, Goon, or Player. Unused for now
    private Vector3Int holePosition; // Snap thingInHole to the hole's position in the tilemap

    [Header("References")]
    public Tilemap tilemap;
  //  public TempTurnBase tempTurnBase;
    

    private void Awake()
    {
        if (tilemap == null)
        {
            tilemap = FindObjectOfType<Tilemap>();
            if (tilemap == null)
                Debug.LogError($"Hole: No Tilemap found for {name}!");
        }
    }

    private void Start()
    {
        // create holePosition to snap thingInHole to the hole's position in the tilemap
        holePosition = tilemap.WorldToCell(transform.position);
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
        Debug.LogWarning("Electrician has fallen into a hole!");

        electricianMove = collision.gameObject.GetComponent<ElectricianMove>();
        thingInHole = collision.gameObject;

        // Snap Electrician to the center of the hole
        thingInHole.transform.position = tilemap.GetCellCenterWorld(holePosition);

        // Prevent movement (if needed)
        // electricianMove.remainingMoves = 0; // Add this if you use movement locking via variable

        // Apply fatigue effect
        ElectricianFatigue electricianFatigue = collision.GetComponent<ElectricianFatigue>();
        if (electricianFatigue != null)
        {
            electricianFatigue.prone = true;
            electricianFatigue.UseGetUpFatigue(); // Spend 1 fatigue to get out
        }
        }
        else if (collision.gameObject.CompareTag("Goon"))
        {
            goonMove = collision.gameObject.GetComponent<GoonMove>();
            thingInHole = collision.gameObject;
            //  goonMove.Prone = true; // bool for Goon script (not used for anything yet)
        }
        else if (collision.gameObject.CompareTag("Player")) // where Russell is currently working (Player) (see PlayerFatigue UseGetUpFatigue())
        {
            Debug.LogWarning("Player has fallen into a hole!");
            // add logic to stop Player (mid turn) from moving (until they spend 1 fatigue)
            playerMove = collision.gameObject.GetComponent<PlayerMove>();
            thingInHole = collision.gameObject;
            // Snap thingInHole to the CENTER OF hole's position in the tilemap (for visual cue
            thingInHole.transform.position = tilemap.GetCellCenterWorld(holePosition);

            playerMove.remainingMoves = 0; // Player can't move while in hole

            PlayerFatigue playerFatigue = collision.GetComponent<PlayerFatigue>();
            playerFatigue.prone = true;  // bool in PlayerFatigue that requires + 1 fatigue spent

            playerFatigue.UseGetUpFatigue(); // Player spends 1 fatigue to get out of hole

        }
        else
        {
            return;
        }

    }
}
