using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Hole : MonoBehaviour

{
    [Header("ScriptRefs")]
    private ElectricianMove electricianMove;
    private GoonMove goonMove;
    private PlayerMove playerMove;
    private GameObject thingInPuddle; // Electrician, Goon, or Player

    [Header("References")]
    public Tilemap tilemap;

    private void Awake()
    {
        if (tilemap == null)
        {
            tilemap = FindObjectOfType<Tilemap>();
         //   if (tilemap == null)
        //    Debug.LogError($"GoonMove: No Tilemap found for {name}!");
        }
    }
}
