using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{
    public string sceneToLoad; 
    public Vector2 playerPosition;
    public VectorValue playerStorage; 
    public GameObject fadeInPanel;

    public void Awake(){
        if(fadeInPanel != null){
            GameObject panel = Instantiate(fadeInPanel, Vector3.zero, Quaternion.identity) as GameObject;
        }
    }

    public void OnTriggerEnter2D(Collider2D other){
        if(other.CompareTag("Player") && !other.isTrigger){
            playerStorage.initialValue = playerPosition; 
            SceneManager.LoadScene(sceneToLoad);
        }
    }
    
}
