using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomMove : MonoBehaviour
{
    //moving camera and player
    public Vector2 minCameraChange; 
    public Vector2 maxCameraChange; 
    public Vector3 playerChange;
    private CameraMovement cam; 

    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main.GetComponent<CameraMovement>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D other){
        if(other.CompareTag("Player")){
            cam.minPosition += minCameraChange; 
            cam.maxPosition += maxCameraChange; 
            other.transform.position += playerChange; 
        }
    }
}
