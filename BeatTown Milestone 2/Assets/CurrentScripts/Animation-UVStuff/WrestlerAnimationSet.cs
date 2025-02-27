using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "WrestlerAnimationSet", menuName = "Animation/WrestlerAnimationSet", order = 1)]
public class WrestlerAnimationSet : ScriptableObject
{
    public Texture2D idleTexture;
    public Texture2D moveTexture;
    public Texture2D punchTexture;
    public Texture2D swingTexture;
    public Texture2D swingBackTexture;
    public Texture2D pushTexture;
    public Texture2D reactTexture;
    public Texture2D jumpTexture;  // optional
    public Texture2D fallTexture;  // optional
    public Texture2D getOutTexture;  // optional

    public Texture2D sourceTexture1;
    public Texture2D sourceTexture2;
    public Texture2D sourceTexture3;
    public Texture2D sourceTexture4;
    public Texture2D sourceBackTexture;
    public Texture2D baseMask;  // common base mask if applicable for all animations
    public Texture2D backBaseMask; 
}