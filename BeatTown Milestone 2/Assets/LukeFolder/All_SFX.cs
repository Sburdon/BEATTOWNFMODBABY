using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class All_SFX : MonoBehaviour
{

    FMOD.Studio.EventInstance walking;
    FMOD.Studio.EventInstance UISelect;
    FMOD.Studio.EventInstance CUANG;
    FMOD.Studio.EventInstance FishBattle;

    int Cuda_Count = 0;



    // Start is called before the first frame update
    void Start()
    {

        UISelect = FMODUnity.RuntimeManager.CreateInstance("event:/Ring Sounds/UI/UI Click");
        walking = FMODUnity.RuntimeManager.CreateInstance("event:/Ring Sounds/Action/RingStep");
        CUANG = FMODUnity.RuntimeManager.CreateInstance("event:/Ring Sounds/UI/CUANG");
        FishBattle = FMODUnity.RuntimeManager.CreateInstance("event:/Fish_Battle");

        
    }

    public void UpdateCudaCount()
    {

        Cuda_Count++ ;

        FishBattle.setParameterByName("Cuda_Count", Cuda_Count);

    }

    public void PlayWalkingSound()
    {
        walking.start();
    }

    public void OnUIButtonPressed()
    {

        UISelect.start();

    }

    public void PlayCUANG()
    {
        CUANG.start();
    }

    public void PlayFishBattle()
    {
        FishBattle.start();
    }




    // Update is called once per frame
    void Update()
    {

       



    }
}
