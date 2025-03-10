using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class HookAudioManager : MonoBehaviour
{
    private Hook hook; // Reference to Hook instance
    public StudioEventEmitter musicEmitter; // Drag the Event Emitter from Game Manager here

    private bool hasChangedParameter = false; // Prevents multiple triggers

    void OnEnable()
    {
        Hook.OnHookSpawned += AssignHook;
        Hook.OnHookKillCountChanged += CheckForMusicChange;
    }

    void OnDisable()
    {
        Hook.OnHookSpawned -= AssignHook;
        Hook.OnHookKillCountChanged -= CheckForMusicChange;
    }

    private void AssignHook(Hook spawnedHook)
    {
        hook = spawnedHook;
        Debug.Log("Hook assigned in HookAudioManager!");
    }

    private void CheckForMusicChange(int killCount)
    {
        if (killCount == 2 && !hasChangedParameter)
        {
            ChangeMusicParameter();
            hasChangedParameter = true;
        }
    }

    private void ChangeMusicParameter()
    {
        if (musicEmitter != null)
        {
            musicEmitter.SetParameter("Cuda_Count", 1f); // Change the FMOD parameter
            Debug.Log("FMOD parameter 'Cuda_Count' set to 1.");
        }
        else
        {
            Debug.LogError("FMOD Studio Event Emitter is not assigned in HookAudioManager!");
        }
    }
}
