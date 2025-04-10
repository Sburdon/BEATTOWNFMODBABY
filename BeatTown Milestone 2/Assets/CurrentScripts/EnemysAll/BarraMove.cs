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

    void Start()
    {
        stateMachine = GetComponent<StateMachine>();

        if (tilemap == null)
        {
            tilemap = FindObjectOfType<Tilemap>();
        }
        if (playerMove == null)
        {
            playerMove = FindObjectOfType<PlayerMove>();
        }

        CurrentTilePosition = tilemap.WorldToCell(transform.position);
    }

    public IEnumerator PerformMove()
    {
        GameObject target = FindClosestTargetWithPriority();
        if (target == null) yield break;

        Vector3Int targetTilePosition = tilemap.WorldToCell(target.transform.position);
        List<Vector3Int> path = CalculatePath(CurrentTilePosition, targetTilePosition);

        int tilesToMove = Mathf.Min(moveDistance, path.Count);
        for (int i = 0; i < tilesToMove; i++)
        {
            Vector3Int nextTile = path[i];

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
            if (target == this.gameObject) continue; // Skip self
            float dist = Vector3.Distance(transform.position, target.transform.position);
            if (dist < closestEnemyDist)
            {
                closestEnemyDist = dist;
                closestEnemy = target;
            }
        }

        // If distances are equal, prefer enemy unless taunted
        if (Mathf.Approximately(closestEnemyDist, closestPlayerDist))
        {
            return isTaunted ? closestPlayer : closestEnemy;
        }

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

    private List<Vector3Int> CalculatePath(Vector3Int start, Vector3Int target)
    {
        List<Vector3Int> path = new List<Vector3Int>();

        int dx = target.x - start.x;
        int dy = target.y - start.y;

        Vector3Int current = start;

        while (current != target)
        {
            Vector3Int nextStep;

            if (Mathf.Abs(dx) > Mathf.Abs(dy))
            {
                nextStep = new Vector3Int(current.x + (dx > 0 ? 1 : -1), current.y, current.z);
                dx += (dx > 0 ? -1 : 1);
            }
            else
            {
                nextStep = new Vector3Int(current.x, current.y + (dy > 0 ? 1 : -1), current.z);
                dy += (dy > 0 ? -1 : 1);
            }

            if (!OccupiedTilesManager.Instance.IsTileOccupied(nextStep))
            {
                path.Add(nextStep);
                current = nextStep;
            }
            else
            {
                break;
            }
        }

        return path;
    }
}
