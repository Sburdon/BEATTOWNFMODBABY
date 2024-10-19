using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;
using System.Collections.Generic;

public class BarraAI : MonoBehaviour
{
    public Tilemap tilemap;
    public PlayerMove playerMove;
    public int moveDistance = 2;
    public float moveSpeed = 1f;


    public Vector3Int CurrentTilePosition;

    private EnemyHealth enemyHealth;
    private EnemyHealth[] allEnemies;

    public int attackDamage = 2;
    public float attackRange = 1f;

    public bool followPlayer = false; // Boolean to determine if BarraAI should follow the player
    private int followPlayerTurns = 0; // Counter for the number of turns to follow the player


    void Start()
    {
        enemyHealth = GetComponent<EnemyHealth>();
        CurrentTilePosition = tilemap.WorldToCell(transform.position);
        OccupiedTilesManager.Instance.RegisterAI(this);
    }

    public void TakeTurn()
    {
        if (enemyHealth != null && enemyHealth.IsDead)
        {
            return;
        }

        // Update the list of all enemies in the scene
        allEnemies = FindObjectsOfType<EnemyHealth>();

        if (followPlayer)
        {
            MoveTowardsPlayer();

            // Decrease the followPlayerTurns counter
            followPlayerTurns--;

            if (followPlayerTurns <= 0)
            {
                followPlayer = false; // Stop following the player after turns expire
            }
        }
        else
        {
            MoveTowardsClosestTarget();
        }
    }

    public void SetFollowPlayerTurns(int turns)
    {
        followPlayerTurns = turns;
        followPlayer = true;
    }

    private void MoveTowardsPlayer()
    {
        // Implement movement towards the player
        Vector3Int targetTilePosition = playerMove.CurrentTilePosition;

        // Calculate the path towards the player
        List<Vector3Int> path = CalculatePath(CurrentTilePosition, targetTilePosition);

        // Move along the path up to moveDistance tiles
        StartCoroutine(MoveAlongPath(path));
    }

    private void MoveTowardsClosestTarget()
    {
        Transform closestTarget = GetClosestTarget();

        if (closestTarget == null)
        {
            // No targets found; do nothing
            return;
        }

        // Implement movement towards the closest target
        Vector3Int targetTilePosition = tilemap.WorldToCell(closestTarget.position);

        // Calculate the path towards the target
        List<Vector3Int> path = CalculatePath(CurrentTilePosition, targetTilePosition);

        // Move along the path up to moveDistance tiles
        StartCoroutine(MoveAlongPath(path));
    }

    private Transform GetClosestTarget()
    {
        Transform closestTarget = null;
        float shortestDistance = Mathf.Infinity;

        // Include the player as a potential target
        List<Transform> potentialTargets = new List<Transform> { playerMove.transform };

        // Add other enemies to potential targets
        foreach (EnemyHealth enemy in allEnemies)
        {
            if (enemy != null && enemy.gameObject != this.gameObject && !enemy.IsDead)
            {
                potentialTargets.Add(enemy.transform);
            }
        }

        foreach (Transform target in potentialTargets)
        {
            float distance = Vector3.Distance(transform.position, target.position);
            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                closestTarget = target;
            }
        }

        return closestTarget;
    }

    private List<Vector3Int> CalculatePath(Vector3Int start, Vector3Int end)
    {
        // Modify the path calculation to stop before the target if necessary
        List<Vector3Int> path = new List<Vector3Int>();

        int dx = end.x - start.x;
        int dy = end.y - start.y;

        int stepX = dx > 0 ? 1 : -1;
        int stepY = dy > 0 ? 1 : -1;

        int absDx = Mathf.Abs(dx);
        int absDy = Mathf.Abs(dy);

        int maxSteps = moveDistance;

        // Calculate the number of steps needed to reach the target
        int totalSteps = absDx + absDy;

        // If the target is within attack range, we need to stop one tile before
        bool targetWithinAttackRange = totalSteps <= moveDistance;

        // Adjust maxSteps to stop before the target if necessary
        if (targetWithinAttackRange)
        {
            maxSteps = totalSteps - 1; // Stop one tile before the target
        }

        // Move along x-axis
        int stepsTaken = 0;
        for (int i = 0; i < absDx && stepsTaken < maxSteps; i++)
        {
            Vector3Int nextPosition = new Vector3Int(start.x + stepX * (i + 1), start.y, start.z);
            if (IsMoveValid(nextPosition))
            {
                path.Add(nextPosition);
                stepsTaken++;
            }
            else
            {
                break; // Stop if movement is blocked
            }
        }

        // Update current x position
        int currentX = start.x + stepX * stepsTaken;

        // Move along y-axis
        for (int i = 0; i < absDy && stepsTaken < maxSteps; i++)
        {
            Vector3Int nextPosition = new Vector3Int(currentX, start.y + stepY * (i + 1), start.z);
            if (IsMoveValid(nextPosition))
            {
                path.Add(nextPosition);
                stepsTaken++;
            }
            else
            {
                break;
            }
        }

        return path;
    }

    private IEnumerator MoveAlongPath(List<Vector3Int> path)
    {
        int steps = Mathf.Min(moveDistance, path.Count);

        for (int i = 0; i < steps; i++)
        {
            Vector3Int targetPosition = path[i];

            // Remove current position from occupied positions
            OccupiedTilesManager.Instance.RemoveOccupiedPosition(CurrentTilePosition);

            // Move to the target tile
            yield return MoveToTile(targetPosition);

            // Update current tile position
            CurrentTilePosition = targetPosition;
            OccupiedTilesManager.Instance.AddOccupiedPosition(CurrentTilePosition);
        }

        // After moving, attempt to attack if in range
        AttackIfInRange();
    }

    private IEnumerator MoveToTile(Vector3Int targetTilePosition)
    {
        Vector3 targetWorldPosition = tilemap.GetCellCenterWorld(targetTilePosition);
        float elapsedTime = 0f;
        float travelTime = 1f / moveSpeed;

        Vector3 startPosition = transform.position;

        while (elapsedTime < travelTime)
        {
            transform.position = Vector3.Lerp(startPosition, targetWorldPosition, elapsedTime / travelTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = targetWorldPosition;
    }

    private bool IsMoveValid(Vector3Int targetTilePosition)
    {
        // Check if the tile is valid, not occupied by another unit, and within bounds
        if (!tilemap.HasTile(targetTilePosition)
            || OccupiedTilesManager.Instance.IsTileOccupied(targetTilePosition)
            || IsPlayerOrEnemyAtPosition(targetTilePosition))
        {
            return false;
        }
        return true;
    }

    private bool IsPlayerOrEnemyAtPosition(Vector3Int position)
    {
        // Check if the player or an enemy is at the given position
        Vector3 worldPosition = tilemap.GetCellCenterWorld(position);
        Collider2D[] colliders = Physics2D.OverlapPointAll(worldPosition);

        foreach (var collider in colliders)
        {
            if (collider.CompareTag("Player") || (collider.CompareTag("Enemy") && collider.gameObject != this.gameObject))
            {
                return true;
            }
        }
        return false;
    }

    private void AttackIfInRange()
    {
        // Check for targets within attack range
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, attackRange);

        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Player"))
            {
                PlayerHealth playerHealth = hitCollider.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(attackDamage);
                    Debug.Log($"{gameObject.name} attacked the player for {attackDamage} damage.");
                }
            }
            else if (hitCollider.CompareTag("Enemy"))
            {
                EnemyHealth otherEnemyHealth = hitCollider.GetComponent<EnemyHealth>();
                if (otherEnemyHealth != null && otherEnemyHealth.gameObject != this.gameObject)
                {
                    otherEnemyHealth.TakeDamage(attackDamage);
                    Debug.Log($"{gameObject.name} attacked {otherEnemyHealth.gameObject.name} for {attackDamage} damage.");
                }
            }
        }
    }

    public void ResetAI()
    {
        StopAllCoroutines(); // Stop any active coroutines
        // Reset other state variables if necessary
    }
}
