using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using static StateMachine;

public class BarraMove : MonoBehaviour
{
    [Header("Barra Move Settings")]
    public float moveSpeed = 1f;
    public int moveDistance = 2;
    public Tilemap tilemap;
    public PlayerMove playerMove;
    private StateMachine stateMachine;
    private bool facingRight = true;

    public Vector3Int CurrentTilePosition { get; set; }

    public bool isTaunted = false;
    public int tauntTurnsRemaining = 0;

    void Start()
    {
        stateMachine = GetComponent<StateMachine>();

        if (tilemap == null)
            tilemap = FindObjectOfType<Tilemap>();
        if (playerMove == null)
            playerMove = FindObjectOfType<PlayerMove>();

        CurrentTilePosition = tilemap.WorldToCell(transform.position);
    }

    public IEnumerator PerformMove()
    {
        GameObject target = FindClosestTargetWithPriority();
        if (target == null) yield break;

        Vector3Int targetTile = tilemap.WorldToCell(target.transform.position);
        List<Vector3Int> adjacentTiles = GetWalkableAdjacentTiles(targetTile);

        List<Vector3Int> shortestPath = null;

        foreach (var adjTile in adjacentTiles)
        {
            List<Vector3Int> path = CalculatePath(CurrentTilePosition, adjTile);
            if (path.Count > 0 && (shortestPath == null || path.Count < shortestPath.Count))
            {
                shortestPath = path;
            }
        }

        if (shortestPath == null)
        {
            Debug.Log($"{gameObject.name}: No valid path to target.");
            yield break;
        }

        int tilesToMove = Mathf.Min(moveDistance, shortestPath.Count);
        for (int i = 0; i < tilesToMove; i++)
        {
            Vector3Int nextTile = shortestPath[i];

            if (!OccupiedTilesManager.Instance.IsTileOccupied(nextTile))
            {
                OccupiedTilesManager.Instance.RemoveOccupiedPosition(CurrentTilePosition);
                yield return StartCoroutine(MoveToTile(nextTile));
                CurrentTilePosition = nextTile;
                OccupiedTilesManager.Instance.AddOccupiedPosition(CurrentTilePosition);
            }
            else
            {
                Debug.Log($"Tile {nextTile} is occupied. Stopping move.");
                break;
            }
        }
    }

    private GameObject FindClosestTargetWithPriority()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        GameObject closestPlayer = null;
        GameObject closestEnemy = null;
        float closestPlayerDist = Mathf.Infinity;
        float closestEnemyDist = Mathf.Infinity;

        foreach (GameObject target in players)
        {
            float dist = Vector3.Distance(transform.position, target.transform.position);
            if (dist < closestPlayerDist)
            {
                closestPlayerDist = dist;
                closestPlayer = target;
            }
        }

        foreach (GameObject target in enemies)
        {
            if (target == this.gameObject) continue;
            float dist = Vector3.Distance(transform.position, target.transform.position);
            if (dist < closestEnemyDist)
            {
                closestEnemyDist = dist;
                closestEnemy = target;
            }
        }

        if (isTaunted && closestPlayer != null)
            return closestPlayer;

        if (Mathf.Approximately(closestEnemyDist, closestPlayerDist))
            return closestEnemy;

        return closestEnemyDist < closestPlayerDist ? closestEnemy : closestPlayer;
    }

    private IEnumerator MoveToTile(Vector3Int targetTile)
    {
        Vector3 targetWorldPosition = tilemap.GetCellCenterWorld(targetTile);
        float elapsedTime = 0f;
        float travelTime = 1f / moveSpeed;
        Vector3 startPosition = transform.position;

        stateMachine.ChangeState(WrestlerState.Move);

        if (targetWorldPosition.x < startPosition.x && facingRight)
        {
            Flip();
        }
        else if (targetWorldPosition.x > startPosition.x && !facingRight)
        {
            Flip();
        }

        while (elapsedTime < travelTime)
        {
            transform.position = Vector3.Lerp(startPosition, targetWorldPosition, elapsedTime / travelTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = targetWorldPosition;
    }

    private void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    private List<Vector3Int> CalculatePath(Vector3Int start, Vector3Int goal)
    {
        Queue<Vector3Int> frontier = new Queue<Vector3Int>();
        Dictionary<Vector3Int, Vector3Int> cameFrom = new Dictionary<Vector3Int, Vector3Int>();
        frontier.Enqueue(start);
        cameFrom[start] = start;

        Vector3Int[] directions = new Vector3Int[]
        {
            Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right
        };

        while (frontier.Count > 0)
        {
            Vector3Int current = frontier.Dequeue();

            if (current == goal)
                break;

            foreach (Vector3Int dir in directions)
            {
                Vector3Int next = current + dir;

                if (!cameFrom.ContainsKey(next) && tilemap.HasTile(next) && !OccupiedTilesManager.Instance.IsTileOccupied(next))
                {
                    frontier.Enqueue(next);
                    cameFrom[next] = current;
                }
            }
        }

        List<Vector3Int> path = new List<Vector3Int>();
        if (!cameFrom.ContainsKey(goal))
        {
            Debug.Log("No path found to target.");
            return path;
        }

        Vector3Int step = goal;
        while (step != start)
        {
            path.Insert(0, step);
            step = cameFrom[step];
        }

        return path;
    }

    private List<Vector3Int> GetWalkableAdjacentTiles(Vector3Int center)
    {
        List<Vector3Int> result = new List<Vector3Int>();
        Vector3Int[] directions = new Vector3Int[]
        {
            Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right
        };

        foreach (var dir in directions)
        {
            Vector3Int check = center + dir;
            if (tilemap.HasTile(check) && !OccupiedTilesManager.Instance.IsTileOccupied(check))
            {
                result.Add(check);
            }
        }

        return result;
    }
}
