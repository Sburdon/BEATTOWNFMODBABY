using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public enum TutorialState
{
    None,
    PartialPause, // Animations, audio, etc., keep playing; inputs allowed. Use unscaledDeltaTime or unscaledTime to allow for animations and sounds to continue.
    FullPause     // Everything is paused; no inputs allowed
}

// Plan for implementing tutorial boxes:
// When transitioning to the next tutorial box, fire an event.
// The event will be listened to by this script, which will then set the next tutorial state depending on the tutorial box (move on to next box or unpause/exit pause state.).
// All components that need to react to pause (e.g., character controllers, interaction scripts) can subscribe to these events and handle pausing.


// Use Unity Events for advancing the tutorial boxes
// Use a custom script to manage the timing (e.g., unscaled time) and track the current tutorial state.


public class TutorialStateManager : MonoBehaviour
{
      public static TutorialState currentTutorialState = TutorialState.None;

   //  float previousTimeScale = 1f;

    public static bool isPaused = false;

    public static void SetTutorialState(TutorialState newState)
    {
        currentTutorialState = newState;
        switch (newState)
        {
            case TutorialState.None:
                Time.timeScale = 1;
                TutorialEvents.OnResume.Invoke();
                break;
            case TutorialState.PartialPause:
                // Time is not scaled down to zero, animations and other effects continue.
                Time.timeScale = 1;
                DisablePlayerInputs(); // Allow certain inputs, but restrict gameplay inputs
                TutorialEvents.OnPartialPause.Invoke();
                break;
            case TutorialState.FullPause:
                Time.timeScale = 0;
                DisableAllInputs(); // Complete pause with no interaction.
                TutorialEvents.OnFullPause.Invoke();
                break;
        }
    }

    private static void DisablePlayerInputs()
    {
        // Logic to disable movement, attacks, etc., but keep tutorial box interactions.
    }

    private static void DisableAllInputs()
    {
        // Disable everything, including UI clicks, to ensure only tutorial box inputs are possible.
    }




    // Old way of pausing the game
    /*public void TogglePause()
    {
        if (Time.timeScale > 0)
        {
            // Pausing
            previousTimeScale = Time.timeScale; 
            Time.timeScale = 0f;
            isPaused = true;
        }
        else
        {
            // Unpausing
            Time.timeScale = previousTimeScale;
            isPaused = false;
        }
    }

    public void SetPause(bool shouldPause)
    {
        if (shouldPause)
        {
            previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            isPaused = true;
        }
        else
        {
            Time.timeScale = previousTimeScale;
            isPaused = false;
        }
    }*/


    // Example of a function that shows a tutorial that doesn't pause the game (for tutorial boxes like NPCs yelling at you)
    // "brah rah rah ah rah"
    // - Ian 2024

    /*public void ShowNonPausingTutorial()
    {
        // Show the UI box
        // Make sure Time.timeScale is unchanged
        // Allow player input to continue
    }*/


    // Example of paused tutorial (for tutorial boxes that pause the game like in beginning of game)
    // Can use CanvasGroup to block all raycast on buttons and other UI elements. 
    // Alternatively, disable the Unity EventSystem temporarily or create a UI Blocker that captures all inputs and does nothing.
    // To handle advancing texts, you can use a coroutine or event trigger to advance the text when the player clicks the 'next' button.

    /*public void ShowPausingTutorial()
    {
        PauseManager.Instance.SetPause(true);
        // Show the UI box
        // Disable all other UI buttons except the tutorial box next button
    }*/
    
}
