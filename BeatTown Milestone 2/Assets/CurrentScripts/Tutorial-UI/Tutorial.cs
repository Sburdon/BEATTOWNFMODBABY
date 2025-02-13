using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tutorial : MonoBehaviour
{
    Animator animtut;
    public Button[] barray;
    private bool isPaused = false;
    private bool isTutorialActive = true;
    private bool isBarraTutorialActive = false;

    private void Start()
    {
        animtut = GetComponent<Animator>();

        // Disable button interactions
        foreach (Button b in barray)
        {
            b.interactable = false;
        }

        // Make unwanted sprites (i.e. hook) invisible
        HideSprites();

        // start coroutiune to delay setting Time.timescale to 0
        StartCoroutine(DelayPause());
    }



    private IEnumerator DelayPause()
    {
        // Wait for 2 seconds (or any desired duration)
        yield return new WaitForFixedUpdate(); // Use WaitForSecondsRealtime to account for timeScale being 1

        // Pause the game
        Time.timeScale = 0f;
        isPaused = true;
    }

    private void HideSprites()
    {
        // Hide Hook sprite
        Hook hook = FindObjectOfType<Hook>();
        if (hook != null)
        {
            SpriteRenderer hookRenderer = hook.GetComponent<SpriteRenderer>();
            if (hookRenderer != null)
            {
                hookRenderer.enabled = false; // Make the sprite invisible
            }
        }

        // Hide Enemy health bar sprites
        EnemyHealth[] enemies = FindObjectsOfType<EnemyHealth>();
        foreach (EnemyHealth enemy in enemies)
        {
            Transform healthBar = enemy.transform.Find("HP_Square"); // Replace with actual child name
            if (healthBar != null)
            {
                SpriteRenderer healthBarRenderer = healthBar.GetComponent<SpriteRenderer>();
                if (healthBarRenderer != null)
                {
                    healthBarRenderer.enabled = false; // Make the sprite invisible
                }
            }
        }
    }


    private void ShowSprites()
    {
        // Show Hook sprite
        Hook hook = FindObjectOfType<Hook>();
        if (hook != null)
        {
            SpriteRenderer hookRenderer = hook.GetComponent<SpriteRenderer>();
            if (hookRenderer != null)
            {
                hookRenderer.enabled = true; // Make the sprite visible again
            }
        }

        // Show Enemy health bar sprites
        EnemyHealth[] enemies = FindObjectsOfType<EnemyHealth>();
        foreach (EnemyHealth enemy in enemies)
        {
            Transform healthBar = enemy.transform.Find("HealthBar"); // Replace with actual child name
            if (healthBar != null)
            {
                SpriteRenderer healthBarRenderer = healthBar.GetComponent<SpriteRenderer>();
                if (healthBarRenderer != null)
                {
                    healthBarRenderer.enabled = true; // Make the sprite visible again
                }
            }
        }
    }


    void ChangeAnimation() // to cycle through tutorial animations
    {
        animtut.SetInteger("Change", animtut.GetInteger("Change") + 1);
    }

    public void ChangeBarraAnimation() // to cycle through barra animations
    {
        Debug.Log("changebarraanimation triggered");
        animtut.SetInteger("ChangeBarra", animtut.GetInteger("ChangeBarra") + 1);
    }

    public void StartBarraAnimation()
    {
        isBarraTutorialActive = true;
        animtut.SetInteger("ChangeBarra", 1); // Ensure this value matches the transition condition in Animator
        Debug.Log("Triggered Barracuda Tutorial.");
    }

    public void Etut() // end tutorial (called in Animator)
    {
        foreach (Button b in barray)
        {
            b.interactable = true;
        }
        Time.timeScale =1f ;
        isPaused = false;
        isTutorialActive = false; // mark tutorial as finished

        // re enable sprites after tutorial
        ShowSprites();
        animtut.SetBool("MainTutOver" , true); // declare end of main tutorial

    }
    
    public void EBarraTut() // end barra tutorial (called in Animator event)
    {
        isBarraTutorialActive = false; // mark barra tutorial as finished
    }

    private void Update() // any key advances tutorial
    {
       /* if (Input.GetKeyDown(KeyCode.Space))
        {
            ChangeBarraAnimation();
        }*/ // remove once barra tutorial is implemented
        
        if (isTutorialActive)
        {
            // Check if any key is pressed to advance the tutorial (exclude ESC key to avoid conflict with pause)
            if (Input.anyKeyDown)
            {
                ChangeAnimation();
            }
        }
        else if (isBarraTutorialActive)
        {
            // Check if SpaceBar is pressed to advance the tutorial
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Debug.LogWarning("Spacebar pressed");
                ChangeBarraAnimation();
            }
        }
        else
        {
            // Check for ESC key to toggle pause state
            PauseGameOnESC();
        }
    }

    public void PauseGameOnESC()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!isTutorialActive) // only allow pausing/resuming if tutorial is completed
            {
                if (!isPaused)
                {
                    // Disable button interactions before freezing
                    foreach (Button b in barray)
                    {
                        b.interactable = false;
                    }
                    // pause the game
                    Time.timeScale = 0f;
                    isPaused = true;
                }
                else
                {
                    // enable button interactions 
                    foreach (Button b in barray)
                    {
                        b.interactable = true;
                    }
                    // resume the game
                    Time.timeScale = 1f;
                    isPaused = false;
                }
            }
        }
    }
}
