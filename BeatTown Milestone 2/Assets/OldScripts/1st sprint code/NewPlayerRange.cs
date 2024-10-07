using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRange : MonoBehaviour
{
    public Collider2D[] rangeColliders; // Array of colliders around the player
    private List<Collider2D> enemiesInRange = new List<Collider2D>(); // List to store enemies in range

    void Start()
    {
        if (rangeColliders.Length == 0)
        {
            Debug.LogError("No range colliders assigned in PlayerRange.");
        }
    }

    void Update()
    {
        DetectEnemiesInRange();
    }

    // Detect enemies within the range colliders
    private void DetectEnemiesInRange()
    {
        enemiesInRange.Clear(); // Clear previous detections

        foreach (var collider in rangeColliders)
        {
            // Check for enemies in the current collider
            Collider2D[] hits = new Collider2D[10]; // Adjust size as needed
            int hitCount = collider.OverlapCollider(new ContactFilter2D(), hits); // Get overlapping colliders

            for (int i = 0; i < hitCount; i++)
            {
                Collider2D hit = hits[i];
                if (hit != null && hit.CompareTag("Enemy"))
                {
                    enemiesInRange.Add(hit);
                }
            }
        }
    }

    public bool IsEnemyInRange(Collider2D enemy)
    {
        return enemiesInRange.Contains(enemy); // Check if the specified enemy is in range
    }

    public List<Collider2D> GetEnemiesInRange()
    {
        return enemiesInRange; // Return the list of enemies in range
    }
}
