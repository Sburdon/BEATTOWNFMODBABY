using System.Collections;
using UnityEngine;

[RequireComponent(typeof(ElectricianMove))]
public class ElectricianFatigue : MonoBehaviour
{
    public int maxFatigue = 2;
    private int currentFatigue;

    private ElectricianMove electricianMove;

    [HideInInspector]
    public bool prone = false;

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
            if (prone)
            {
                if (currentFatigue >= 1)
                {
                    currentFatigue--;
                    prone = false;
                    Debug.Log($"{name}: Stood up from hole. Remaining fatigue: {currentFatigue}");

                    yield return StartCoroutine(electricianMove.MoveUsingFatigue(1, true));
                    continue;
                }
                else
                {
                    Debug.Log($"{name}: Not enough fatigue to stand up from hole.");
                    break;
                }
            }

            if (electricianMove.currentRouteIndex >= electricianMove.routePositions.Count)
            {
                Debug.Log("Electrician: Finished all route positions.");
                break;
            }

            Vector3Int currentTile = electricianMove.CurrentTilePosition;
            Vector3Int destinationTile = electricianMove.routePositions[electricianMove.currentRouteIndex];

            if (currentTile == destinationTile)
            {
                if (currentFatigue > 0)
                {
                    Debug.Log("✅ Electrician: Fixing guaranteed panel at destination.");
                    yield return StartCoroutine(electricianMove.FixPanelAtRoutePosition());
                    UseFatigue(1);
                    continue;
                }
                else
                {
                    Debug.Log("⚠️ Not enough fatigue to fix panel.");
                    break;
                }
            }

            Debug.Log("Electrician: Attempting to move.");
            yield return StartCoroutine(electricianMove.MoveUsingFatigue(electricianMove.moveDistance));
            UseFatigue(1);

            
        }

        Debug.Log("Electrician turn ends.");
        yield return null;
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

    public int GetCurrentFatigue() => currentFatigue;
    public bool HasFatigue() => currentFatigue > 0;
}
