using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;

public class AiPunch : MonoBehaviour
{
    public Tilemap tilemap;
    public int health = 10;  // Health should probably not be in the AiPunch script.
    public float knockbackSpeed = 3.0f;

    public void TryPunchIfInRange(GameObject target)
    {
        if (target == null)
        {
            Debug.LogError("Target is null in TryPunchIfInRange.");
            return;
        }

        Vector3Int aiPosition = tilemap.WorldToCell(transform.position);
        Vector3Int targetPosition = tilemap.WorldToCell(target.transform.position);

        if (Mathf.Abs(aiPosition.x - targetPosition.x) <= 1 && Mathf.Abs(aiPosition.y - targetPosition.y) <= 1)
        {
            var playerPunch = target.GetComponent<PlayerPunch>();
            if (playerPunch != null)
            {
                //playerPunch.health -= 3;
                //Debug.Log($"AI punched {target.name}. Remaining Health: {playerPunch.health}");
                TriggerSound("event:/Ring_Sounds/Player_Hurt");

                Vector3Int moveDirection = targetPosition - aiPosition;
                Vector3Int newTargetPosition = targetPosition + moveDirection;
                Vector3 newWorldPosition = tilemap.GetCellCenterWorld(newTargetPosition);
                StartCoroutine(SmoothMove(target, newWorldPosition, newTargetPosition));
            }
            else
            {
                Debug.LogError("PlayerPunch component is missing on target.");
            }
        }
        else
        {
            Debug.Log("Target is not adjacent. Punch failed.");
        }
    }

    private void TriggerSound(string path)
    {
        FMOD.Studio.EventInstance soundEvent = FMODUnity.RuntimeManager.CreateInstance(path);
        soundEvent.start();
        soundEvent.release(); // Release the instance after it has played.
    }

    private IEnumerator SmoothMove(GameObject target, Vector3 newWorldPosition, Vector3Int newCellPosition)
    {
        if (!GridManager.Instance.IsCellOccupied(newCellPosition))
        {
            Vector3 startPosition = target.transform.position;
            float startTime = Time.time;
            float journeyLength = Vector3.Distance(startPosition, newWorldPosition);
            float fracJourney;

            while (Vector3.Distance(target.transform.position, newWorldPosition) > 0.01f)
            {
                float distCovered = (Time.time - startTime) * knockbackSpeed;
                fracJourney = distCovered / journeyLength;
                target.transform.position = Vector3.Lerp(startPosition, newWorldPosition, fracJourney);
                yield return null;
            }
            target.transform.position = newWorldPosition;
            GridManager.Instance.SetCellOccupied(tilemap.WorldToCell(startPosition), false);
            GridManager.Instance.SetCellOccupied(newCellPosition, true);
        }
        else
        {
            Debug.Log("Knockback failed, cell is occupied.");
        }
    }
}
