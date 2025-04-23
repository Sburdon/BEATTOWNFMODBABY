using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class OutlineRenderer : MonoBehaviour
{
    public Material outlineMaterial; // Must support same UV mapping
    // public Material baseMaterial;

    void Start()
    {
        // var sr = GetComponent<SpriteRenderer>();
        // if (sr != null && outlineMaterial != null && baseMaterial != null)
        // {
        //     sr.materials = new Material[] { outlineMaterial, baseMaterial };
        // }
        ApplyOutline();
    }

    void ApplyOutline(){
        SpriteRenderer sr = GetComponent<SpriteRenderer>();

        if (sr != null && outlineMaterial != null)
        {
            Material[] materials = { outlineMaterial, sr.sharedMaterial }; // Outline first
            sr.materials = materials;
        }
    }
}
