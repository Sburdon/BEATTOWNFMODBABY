using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class All_SFX : MonoBehaviour
{

    FMOD.Studio.EventInstance walking;
    FMOD.Studio.EventInstance UISelect;
    FMOD.Studio.EventInstance UIDENY;
    FMOD.Studio.EventInstance CUANG;
    FMOD.Studio.EventInstance FishBattle;
    FMOD.Studio.EventInstance Shock_Value;
    FMOD.Studio.EventInstance FishSlap;
    FMOD.Studio.EventInstance Swing;
    FMOD.Studio.EventInstance Push;
    FMOD.Studio.EventInstance PlayerHurt;
    FMOD.Studio.EventInstance Caught;
    FMOD.Studio.EventInstance Panel_Fix;
    FMOD.Studio.EventInstance Panel_Added;
    FMOD.Studio.EventInstance Jump;
    FMOD.Studio.EventInstance Slip;
    FMOD.Studio.EventInstance Fall;
    FMOD.Studio.EventInstance Wood_Stock;
    FMOD.Studio.EventInstance Leaves;
    FMOD.Studio.EventInstance Trees;
    FMOD.Studio.EventInstance BEAVER;
    FMOD.Studio.EventInstance One_Step;
    FMOD.Studio.EventInstance Two_Step;
    FMOD.Studio.EventInstance Health_Drain;
    FMOD.Studio.EventInstance Endless;
    FMOD.Studio.EventInstance TITLE;
    FMOD.Studio.EventInstance Dirt;
    FMOD.Studio.EventInstance Grass;
    FMOD.Studio.EventInstance Door;
    



    int Cuda_Count = 0;



    // Start is called before the first frame update
    void Start()
    {

        UISelect = FMODUnity.RuntimeManager.CreateInstance("event:/Ring Sounds/UI/UI Click");
        UIDENY = FMODUnity.RuntimeManager.CreateInstance("event:/Ring Sounds/UI/UI Deny");
        walking = FMODUnity.RuntimeManager.CreateInstance("event:/Ring Sounds/Action/RingStep");
        CUANG = FMODUnity.RuntimeManager.CreateInstance("event:/Ring Sounds/UI/Fish_UI/CUANG");
        FishBattle = FMODUnity.RuntimeManager.CreateInstance("event:/Fish_Battle");
        Shock_Value = FMODUnity.RuntimeManager.CreateInstance("event:/Shock_Value");
        FishSlap = FMODUnity.RuntimeManager.CreateInstance("event:/Ring Sounds/Action/Fish Slap Hit");
        Swing = FMODUnity.RuntimeManager.CreateInstance("event:/Ring Sounds/Action/Carry");
        Push = FMODUnity.RuntimeManager.CreateInstance("event:/Ring Sounds/Action/Push");
        PlayerHurt = FMODUnity.RuntimeManager.CreateInstance("event:/Ring Sounds/Player Hurt");
        Caught = FMODUnity.RuntimeManager.CreateInstance("event:/Ring Sounds/UI/Fish_UI/Caught");
        Panel_Fix = FMODUnity.RuntimeManager.CreateInstance("event:/Ring Sounds/UI/Electrician_UI/Panel_Fix");
        Panel_Added = FMODUnity.RuntimeManager.CreateInstance("event:/Ring Sounds/UI/Electrician_UI/Panel_Added");
        Jump = FMODUnity.RuntimeManager.CreateInstance("event:/Ring Sounds/UI/Electrician_UI/Jump");
        Slip = FMODUnity.RuntimeManager.CreateInstance("event:/Ring Sounds/UI/Electrician_UI/Slip");
        Fall = FMODUnity.RuntimeManager.CreateInstance("event:/Ring Sounds/UI/Electrician_UI/Fall");
        Wood_Stock = FMODUnity.RuntimeManager.CreateInstance("event:/Wood_Stock");
        Leaves = FMODUnity.RuntimeManager.CreateInstance("event:/Ring Sounds/UI/Lumber_UI/Leaves");
        Trees = FMODUnity.RuntimeManager.CreateInstance("event:/Ring Sounds/UI/Lumber_UI/Tree_Fall");
        BEAVER = FMODUnity.RuntimeManager.CreateInstance("event:/Ring Sounds/UI/Lumber_UI/Beaver");
        One_Step = FMODUnity.RuntimeManager.CreateInstance("event:/Ring Sounds/UI/One_Step");
        Two_Step = FMODUnity.RuntimeManager.CreateInstance("event:/Ring Sounds/UI/Two_Step");
        Health_Drain = FMODUnity.RuntimeManager.CreateInstance("event:/Ring Sounds/UI/Health_Drain");
        Endless = FMODUnity.RuntimeManager.CreateInstance("event:/Endless");
        TITLE = FMODUnity.RuntimeManager.CreateInstance("event:/TITLE");
        Dirt = FMODUnity.RuntimeManager.CreateInstance("event:/OVERWORLD/Dirt");
        Grass = FMODUnity.RuntimeManager.CreateInstance("event:/OVERWORLD/Grass");
        Door = FMODUnity.RuntimeManager.CreateInstance("event:/OVERWORLD/Door");



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

    public void PlayShockValue()
    {
        Shock_Value.start();
    }

    public void PlayFishSlap()
    {
        FishSlap.start();
    }

    public void PlaySwing()
    {
        Swing.start();
    }

    public void PlayPush()
    {
        Push.start();
    }

    public void PlayPlayerHurt()
    {
        PlayerHurt.start();
    }


    public void PlayCaught()
    {
        Caught.start();
    }

    public void PlayUIDENY()
    {
        UIDENY.start();
    }

    public void PlayPanelFix()
    {
        Panel_Fix.start();
    }

    public void PlayPanelAdded()
    {
        Panel_Added.start();


    }

    public void PlayJump()
    {
        Jump.start();
    }

    public void PlaySlip()
    {
        Slip.start();
    }

    public void PlayFall()
    {
        Fall.start();


    }

    public void PlayOneStep()
    {
        One_Step.start();
    }

    public void PlayTwoStep()
    {
        Two_Step.start();
    }

    public void PlayHealthDrain()
    {
        Health_Drain.start();
    }

    public void StopHealthDrain()
    {
        Health_Drain.stop();
    }

    public void PlayWoodStock()
    {
        Wood_Stock.start();

    }

    public void PlayLeaves()
    {
        Leaves.start();
    }

    public void PlayTrees()
    {
        Leaves.start()
    }

    public void PlayBeaver()
    {
        BEAVER.start();
    }

    public void PlayEndless()
    {
        Endless.start();
    }

    public void PlayTitle()
    {
        TITLE.start()
    }

    public void PlayDirt()
    {
        Dirt.start();
    }

    public void PlayGrass()
    {
        Grass.start();
    }

    public void PlayDoor()
    {
        Door.start();
    }

















    // Update is called once per frame
    void Update()
    {

       



    }
}
