using UnityEngine;

[CreateAssetMenu]
public class BoolValue : ScriptableObject
{
    public bool Value;

    private void OnEnable()
    {
    #if UNITY_EDITOR
        Value = false;
    #endif
    }
}
