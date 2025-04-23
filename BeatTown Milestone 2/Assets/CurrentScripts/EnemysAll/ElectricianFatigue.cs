using System.Collections;
using UnityEngine;


[RequireComponent(typeof(ElectricianMove))]
public class ElectricianFatigue : MonoBehaviour
{
    public int maxFatigue = 2;
    private int currentFatigue;

    private ElectricianMove electricianMove;

    private void Awake()
    {
        electricianMove = GetComponent<ElectricianMove>();
    }

    public void ResetFatigue()
    {
        currentFatigue = maxFatigue;
    }

    public IEnumerator HandleTurn()
{
    ResetFatigue(); 
    Debug.Log($"Electrician Turn Start — Fatigue: {currentFatigue}");

    while (currentFatigue > 0)
    {
        // ◀ If we're in a hole, spend 1 fatigue to climb out (one tile)
        if (prone)
        {
            if (currentFatigue > 0)
            {
                Debug.Log("Electrician is prone: spending 1 fatigue to climb out.");
                yield return StartCoroutine(UseGetUpFatigue());
                continue;  // re-enter loop
            }
            else
            {
                Debug.Log("No fatigue to climb out. Turn ends.");
                yield break;
            }
        }

        // ◀ If standing on a broken panel, fix it
        if (electricianMove.IsOnBrokenPanel())
        {
            yield return StartCoroutine(electricianMove.FixPanel());
            UseFatigue(1);
            continue;
        }

        // ◀ Normal move: spend 1 fatigue for exactly 1 tile
        Debug.Log("Electrician attempting one‐tile move...");
        yield return StartCoroutine(electricianMove.MoveUsingFatigue(1));
        UseFatigue(1);

        // ◀ Check if we fell into a hole mid‐move
        if (prone)
        {
            Debug.Log("Fell into hole during move. Handling climb‐out next iteration.");
            continue;  // next loop will catch prone and spend get-up fatigue
        }
    }

    Debug.Log("Electrician turn ends: no fatigue left");
    yield break;
}




    public bool UseFatigue(int amount)
    {
        if (currentFatigue >= amount)
        {
            currentFatigue -= amount;
            Debug.Log($"Electrician used {amount} fatigue. Remaining: {currentFatigue}");
            return true;
        }
        Debug.Log("Electrician: Not enough fatigue!");
        return false;
    }

    [HideInInspector]
public bool prone = false;

public IEnumerator UseGetUpFatigue()
{
    if (!prone || currentFatigue < 1)
        yield break;

    currentFatigue--;
    prone = false;
    Debug.Log($"{name}: Used 1 fatigue to stand up from hole. Remaining: {currentFatigue}");

    yield return electricianMove.MoveUsingFatigue(1, limitToOneTile: true);
}


    public int GetCurrentFatigue() => currentFatigue;
    public bool HasFatigue() => currentFatigue > 0;
}
