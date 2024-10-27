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
    public Texture2D pushTexture;
    public Texture2D reactTexture;

    public Texture2D sourceTexture;
    public Texture2D baseMask;  // common base mask if applicable for all animations
}