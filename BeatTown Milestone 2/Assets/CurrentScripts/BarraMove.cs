using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BarraMove : MonoBehaviour
{
    [Header("Barra Move Settings")]
    public float moveSpeed = 1f;
    public int moveDistance = 2;
    public Tilemap tilemap;
    public PlayerMove playerMove;

    public Vector3Int CurrentTilePosition { get; set; }

    void Start()
    {
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
        GameObject target = FindClosestTarget();
        if (target == null) yield break;

        Vector3Int targetTilePosition = tilemap.WorldToCell(target.transform.position);
        List<Vector3Int> path = CalculatePath(CurrentTilePosition, targetTilePosition);

        for (int i = 0; i < moveDistance && i < path.Count; i++)
        {
            Vector3Int nextTile = path[i];

            // Ensure that the next tile is not occupied before moving
            if (!OccupiedTilesManager.Instance.IsTileOccupied(nextTile))
            {
                OccupiedTilesManager.Instance.RemoveOccupiedPosition(CurrentTilePosition);

                yield return StartCoroutine(MoveToTile(nextTile));
                CurrentTilePosition = nextTile;

                OccupiedTilesManager.Instance.AddOccupiedPosition(CurrentTilePosition);
            }
            else
            {
                Debug.Log($"Tile {nextTile} is occupied. Skipping move.");
                break;
            }
        }
    }

    private GameObject FindClosestTarget()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        GameObject closestTarget = null;
        float closestDistance = Mathf.Infinity;

        foreach (GameObject target in players)
        {
            float distance = Vector3.Distance(transform.position, target.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestTarget = target;
            }
        }

        foreach (GameObject target in enemies)
        {
            float distance = Vector3.Distance(transform.position, target.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestTarget = target;
            }
        }

        return closestTarget;
    }

    private IEnumerator MoveToTile(Vector3Int targetTile)
    {
        Vector3 targetWorldPosition = tilemap.GetCellCenterWorld(targetTile);
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

    private List<Vector3Int> CalculatePath(Vector3Int start, Vector3Int target)
    {
        List<Vector3Int> path = new List<Vector3Int>();

        int dx = target.x - start.x;
        int dy = target.y - start.y;

        for (int i = 0; i < Mathf.Abs(dx); i++)
        {
            Vector3Int nextTile = new Vector3Int(start.x + (dx > 0 ? 1 : -1), start.y, start.z);
            if (!OccupiedTilesManager.Instance.IsTileOccupied(nextTile))
            {
                path.Add(nextTile);
                start = nextTile;
            }
            else
            {
                break;
            }
        }
        for (int i = 0; i < Mathf.Abs(dy); i++)
        {
            Vector3Int nextTile = new Vector3Int(start.x, start.y + (dy > 0 ? 1 : -1), start.z);
            if (!OccupiedTilesManager.Instance.IsTileOccupied(nextTile))
            {
                path.Add(nextTile);
                start = nextTile;
            }
            else
            {
                break;
            }
        }

        return path;
    }
}
