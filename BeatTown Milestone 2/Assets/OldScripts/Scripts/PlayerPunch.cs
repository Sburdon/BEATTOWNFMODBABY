using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerPunch : MonoBehaviour
{
    public Tilemap tilemap; // Reference to the Tilemap for calculating positions
    public int health = 10; // Health of the player
    public float knockbackSpeed = 2.0f; // Speed of the knockback animation

    public void TryPunch(GameObject target)
    {
        Vector3Int attackerPosition = tilemap.WorldToCell(transform.position);
        Vector3Int targetPosition = tilemap.WorldToCell(target.transform.position);

        if (Mathf.Abs(attackerPosition.x - targetPosition.x) <= 1 && Mathf.Abs(attackerPosition.y - targetPosition.y) <= 1)
        {
            AiPunch targetAiPunch = target.GetComponent<AiPunch>();
            if (targetAiPunch != null)
            {
                // Assume AiPunch has a 'health' field you can modify
                targetAiPunch.health -= 3; // Deduct health
                Debug.Log($"Punched AI {target.name}. Remaining Health: {targetAiPunch.health}");

                // Increment the punch counter in ObjectivesManager
                ObjectivesManager.Instance.IncrementPunchCount();

                // Calculate new target position for knockback
                Vector3Int moveDirection = targetPosition - attackerPosition;
                Vector3Int newTargetPosition = targetPosition + moveDirection;
                Vector3 newWorldPosition = tilemap.GetCellCenterWorld(newTargetPosition);

                // Start the coroutine to move the target
                StartCoroutine(SmoothMove(target, newWorldPosition, newTargetPosition));

                FMOD.Studio.EventInstance Fish_Slap_Hit;
                Fish_Slap_Hit = FMODUnity.RuntimeManager.CreateInstance("event:/Ring_Sounds/Fish_Slap_Hit");
                Fish_Slap_Hit.start();
            }
            else
            {
                Debug.Log("No AiPunch component found on the target.");
            }
        }
        else
        {
            Debug.Log("Target is not adjacent. Punch failed.");
        }
    }

    private IEnumerator SmoothMove(GameObject target, Vector3 newWorldPosition, Vector3Int newCellPosition)
    {
        if (!GridManager.Instance.IsCellOccupied(newCellPosition))
        {
            Vector3 startPosition = target.transform.position;
            float startTime = Time.time;
            float journeyLength = Vector3.Distance(startPosition, newWorldPosition);

            while (Vector3.Distance(target.transform.position, newWorldPosition) > 0.01f)
            {
                float distCovered = (Time.time - startTime) * knockbackSpeed;
                float fracJourney = distCovered / journeyLength;
                target.transform.position = Vector3.Lerp(startPosition, newWorldPosition, fracJourney);
                yield return null;
            }
            target.transform.position = newWorldPosition; // Ensure target exactly reaches the new position

            // Update the grid occupancy
            GridManager.Instance.SetCellOccupied(tilemap.WorldToCell(startPosition), false);
            GridManager.Instance.SetCellOccupied(newCellPosition, true);
        }
        else
        {
            Debug.Log("Knockback failed, cell is occupied.");
        }
    }
}
