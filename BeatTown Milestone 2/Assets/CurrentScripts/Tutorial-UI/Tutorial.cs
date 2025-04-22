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
    private bool isEnemyTutorialActive = false;
    private bool isHookedTutorialActive = false; 
    private bool canReceiveInput = false;
    [SerializeField]
    public GameObject[] PauseMenuItems;

    private void Start()
    {
        foreach (GameObject item in PauseMenuItems)
        {
            item.SetActive(false);
        }
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
        StartCoroutine(AllowInputAfterSplash());
    }

    private IEnumerator AllowInputAfterSplash()
    {
        // Wait at least a couple seconds after the scene starts, or match Unity splash duration
        yield return new WaitForSecondsRealtime(2f); // Adjust time to match your splash screen duration
        canReceiveInput = true;
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

    public void StartBarraAnimation() // called in TempTurnBase
    {
        isBarraTutorialActive = true;
        // only set integeer ChangeBarra to 1 ONE TIME using for loop
        for (int i = 0; i < 1; i++)
        {
            animtut.SetInteger("ChangeBarra", 1);
        }
        
        Debug.Log("Triggered Barracuda Tutorial.");
    }

    void ChangeEnemyAnimation() // to cycle through enemy animations 
    {
        animtut.SetInteger("ChangeEnemy", animtut.GetInteger("ChangeEnemy") + 1);
    }

    public void StartEnemyAnimation() // called in RespawnManager
    {
        isEnemyTutorialActive = true;
        // only set integeer ChangeEnemy to 1 ONE TIME using for loop
        for (int i = 0; i < 1; i++)
        {
            animtut.SetInteger("ChangeEnemy", 1);
        }
        Debug.Log("Triggered Enemy Tutorial.");
    }
    

    void ChangeHookedAnimation()
    {
        animtut.SetInteger("ChangeHooked", animtut.GetInteger("ChangeHooked") + 1);
    }


    public void StartHookedAnimation()
    {
        isHookedTutorialActive = true;
        for (int i = 0; i < 1; i++)
        {
            animtut.SetInteger("ChangeHooked", 1);
        }
        Debug.Log("Triggered Hooked Tutorial.");
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
        if (!canReceiveInput)
            return; // Ignore input until allowed

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
        else if (isEnemyTutorialActive)
        {
            // Check if SpaceBar is pressed to advance the tutorial
            if (Input.GetKeyDown(KeyCode.Space))
            {
                ChangeEnemyAnimation();
            }
        }
        else if (isHookedTutorialActive)
        {
            // Check if SpaceBar is pressed to advance the tutorial
            if (Input.GetKeyDown(KeyCode.Space))
            {
                ChangeHookedAnimation();
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
                    foreach (GameObject item in PauseMenuItems)
                    {
                        item.SetActive(true);
                    }

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
                    foreach (GameObject item in PauseMenuItems)
                    {
                        item.SetActive(false);
                    }
                }
            }
        }
    }
}
