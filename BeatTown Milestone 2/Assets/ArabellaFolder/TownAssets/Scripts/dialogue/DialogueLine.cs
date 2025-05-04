using UnityEngine;

[System.Serializable]
public struct DialogueLine
{
    public string speakerName;
    [TextArea(2, 5)]
    public string line;
    public Sprite portrait;
}
