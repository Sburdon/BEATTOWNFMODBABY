/*
using UnityEngine.Tilemaps;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarraAI : MonoBehaviour
{
    [Header("References")]
    public Tilemap tilemap;
    public PlayerMove playerMove;
    public int moveDistance = 2;
    public float moveSpeed = 1f;

    [Header("Attack Settings")]
    public int attackDamage = 2;
    public float attackRange = 1f;

    [Header("Behavior Settings")]
    public bool followPlayer = false; // Determines if BarraAI should follow the player
    private int followPlayerTurns = 1; // Number of turns to follow the player

    [HideInInspector]
    public Vector3Int CurrentTilePosition { get; set; }

    private EnemyHealth enemyHealth;
    private Hook hook; // Reference to the hook object for position checks

    void Start()
    {
        enemyHealth = GetComponent<EnemyHealth>();
        CurrentTilePosition = tilemap.WorldToCell(transform.position);
        OccupiedTilesManager.Instance.RegisterBarraAI(this); // Corrected method call
        hook = Hook.Instance; // Get reference to the hook

        // Ensure AIFatigue component is present
        AIFatigue aiFatigue = GetComponent<AIFatigue>();
        if (aiFatigue == null)
        {
            aiFatigue = gameObject.AddComponent<AIFatigue>();
            aiFatigue.maxFatigue = 2; // Set as needed
        }

        // Ensure AIAttack component is present
        AIAttack aiAttack = GetComponent<AIAttack>();
        if (aiAttack == null)
        {
            aiAttack = gameObject.AddComponent<AIAttack>();
            aiAttack.playerHealth = playerMove.GetComponent<PlayerHealth>();
            aiAttack.attackDamage = attackDamage;
            aiAttack.attackRange = attackRange;
        }
    }

    /// <summary>
    /// Determines if the player or any other enemy is nearby within attack range.
    /// </summary>
    /// <returns>True if a target is nearby; otherwise, false.</returns>
    private bool IsEnemyOrPlayerNearby()
    {
        // Check if the AI is next to the player or an enemy
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, attackRange);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Player"))
            {
                PlayerHealth playerHealth = hitCollider.GetComponent<PlayerHealth>();
                if (playerHealth != null && !playerHealth.IsDead)
                {
                    return true;
                }
            }
            else if (hitCollider.CompareTag("Enemy"))
            {
                EnemyHealth enemyHealth = hitCollider.GetComponent<EnemyHealth>();
                if (enemyHealth != null && !enemyHealth.IsDead && enemyHealth.gameObject != this.gameObject)
                {
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// Sets the number of turns the BarraAI will follow the player.
    /// </summary>
    /// <param name="turns">Number of turns to follow the player.</param>
    public void SetFollowPlayerTurns(int turns)
    {
        followPlayerTurns = turns;
        followPlayer = true;
    }

    /// <summary>
    /// Determines the closest target (player or enemy) for movement.
    /// </summary>
    /// <returns>The Transform of the closest target.</returns>
    private Transform GetClosestTarget()
    {
        Transform closestTarget = null;
        float shortestDistance = Mathf.Infinity;

        // Include the player as a potential target
        List<Transform> potentialTargets = new List<Transform> { playerMove.transform };

        // Add other enemies to potential targets
        EnemyHealth[] allEnemies = FindObjectsOfType<EnemyHealth>();
        foreach (EnemyHealth enemy in allEnemies)
        {
            if (enemy != null && enemy.gameObject != this.gameObject && !enemy.IsDead)
            {
                potentialTargets.Add(enemy.transform);
            }
        }

        // Find the closest target
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

    /// <summary>
    /// Calculates a simple path towards the target while avoiding the hook.
    /// </summary>
    /// <param name="start">Starting tile position.</param>
    /// <param name="end">Target tile position.</param>
    /// <returns>A list of tile positions representing the path.</returns>
    private List<Vector3Int> CalculatePathAvoidingHook(Vector3Int start, Vector3Int end)
    {
        // Get the hook position to avoid
        Vector3Int hookPosition = hook.GetHookPosition();

        List<Vector3Int> path = new List<Vector3Int>();

        int dx = end.x - start.x;
        int dy = end.y - start.y;

        int stepX = dx > 0 ? 1 : -1;
        int stepY = dy > 0 ? 1 : -1;

        int absDx = Mathf.Abs(dx);
        int absDy = Mathf.Abs(dy);

        int maxSteps = moveDistance;

        int totalSteps = absDx + absDy;

        bool targetWithinAttackRange = totalSteps <= moveDistance;

        if (targetWithinAttackRange)
        {
            maxSteps = totalSteps - 1; // Stop one tile before the target
        }

        // Move along x-axis, avoiding the hook
        int stepsTaken = 0;
        for (int i = 0; i < absDx && stepsTaken < maxSteps; i++)
        {
            Vector3Int nextPosition = new Vector3Int(start.x + stepX * (i + 1), start.y, start.z);
            if (IsMoveValid(nextPosition) && nextPosition != hookPosition)
            {
                path.Add(nextPosition);
                stepsTaken++;
            }
            else
            {
                break; // Stop if movement is blocked or hook is nearby
            }
        }

        // Update current x position
        int currentX = start.x + stepX * stepsTaken;

        // Move along y-axis, avoiding the hook
        for (int i = 0; i < absDy && stepsTaken < maxSteps; i++)
        {
            Vector3Int nextPosition = new Vector3Int(currentX, start.y + stepY * (i + 1), start.z);
            if (IsMoveValid(nextPosition) && nextPosition != hookPosition)
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

    /// <summary>
    /// Moves the BarraAI along the specified path.
    /// </summary>
    /// <param name="path">List of tile positions to move through.</param>
    /// <returns>Coroutine.</returns>
    private IEnumerator MoveAlongPath(List<Vector3Int> path)
    {
        int steps = Mathf.Min(moveDistance, path.Count);

        for (int i = 0; i < steps; i++)
        {
            Vector3Int targetPosition = path[i];

            // Remove current position from occupied positions
            OccupiedTilesManager.Instance.RemoveOccupiedPosition(CurrentTilePosition);

            // Move to the target tile
            yield return StartCoroutine(MoveToTile(targetPosition));

            // Update current tile position
            CurrentTilePosition = targetPosition;
            OccupiedTilesManager.Instance.AddOccupiedPosition(CurrentTilePosition);
        }

        // After moving, attempt to attack if in range
        AttackIfInRange();
    }

    /// <summary>
    /// Moves the BarraAI to a specific tile position over time.
    /// </summary>
    /// <param name="targetTilePosition">Target tile position.</param>
    /// <returns>Coroutine.</returns>
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

    /// <summary>
    /// Checks if moving to the target tile is valid.
    /// </summary>
    /// <param name="targetTilePosition">Target tile position.</param>
    /// <returns>True if move is valid; otherwise, false.</returns>
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

    /// <summary>
    /// Checks if the player or any other enemy is at the given tile position.
    /// </summary>
    /// <param name="position">Tile position to check.</param>
    /// <returns>True if player or enemy is present; otherwise, false.</returns>
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

    /// <summary>
    /// Attacks all targets within attack range.
    /// </summary>
    private void AttackIfInRange()
    {
        // Check for targets within attack range
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, attackRange);

        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Player"))
            {
                PlayerHealth playerHealth = hitCollider.GetComponent<PlayerHealth>();
                if (playerHealth != null && !playerHealth.IsDead)
                {
                    playerHealth.TakeDamage(attackDamage); // Deal damage to the player
                    Debug.Log($"{gameObject.name} attacked the player for {attackDamage} damage.");
                }
            }
            else if (hitCollider.CompareTag("Enemy"))
            {
                EnemyHealth otherEnemyHealth = hitCollider.GetComponent<EnemyHealth>();
                if (otherEnemyHealth != null && !otherEnemyHealth.IsDead && otherEnemyHealth.gameObject != this.gameObject)
                {
                    otherEnemyHealth.TakeDamage(attackDamage);
                    Debug.Log($"{gameObject.name} attacked {otherEnemyHealth.gameObject.name} for {attackDamage} damage.");
                }
            }
        }
    }

    /// <summary>
    /// Resets the BarraAI by stopping all coroutines.
    /// </summary>
    public void ResetAI()
    {
        StopAllCoroutines(); // Stop any active coroutines
    }

    /// <summary>
    /// Example method to perform BarraAI's turn logic.
    /// </summary>
    public void PerformTurn()
    {
        if (enemyHealth != null && enemyHealth.CurrentHealth > 0)
        {
            // Example: Move towards the player
            Vector3Int targetTile = tilemap.WorldToCell(playerMove.transform.position);
            List<Vector3Int> path = CalculatePathAvoidingHook(CurrentTilePosition, targetTile);
            StartCoroutine(MoveAlongPath(path));
        }
    }
}
*/