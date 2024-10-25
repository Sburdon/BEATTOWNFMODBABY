using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Tilemaps;

public class BarraMove : MonoBehaviour
{
    [Header("Barra Move Settings")]
    public float moveSpeed = 1f;          // Speed at which Barra moves
    public int moveDistance = 2;          // Number of tiles Barra can move
    public Tilemap tilemap;               // Reference to the tilemap
    public PlayerMove playerMove;         // Reference to the PlayerMove script

    public Vector3Int CurrentTilePosition { get; set; }

    void Start()
    {
        // Auto-assign Tilemap and PlayerMove if not assigned
        if (tilemap == null)
        {
            tilemap = FindObjectOfType<Tilemap>();
        }
        if (playerMove == null)
        {
            playerMove = FindObjectOfType<PlayerMove>();
        }

        // Set initial position
        CurrentTilePosition = tilemap.WorldToCell(transform.position);
    }

    /// <summary>
    /// Move the Barra towards the closest non-Barra target (player or enemy).
    /// </summary>
    public IEnumerator PerformMove()
    {
        GameObject target = FindClosestTarget();
        if (target == null) yield break;

        Vector3Int targetTilePosition = tilemap.WorldToCell(target.transform.position);
        List<Vector3Int> path = CalculatePath(CurrentTilePosition, targetTilePosition);

        // Move up to moveDistance tiles along the path
        for (int i = 0; i < moveDistance && i < path.Count; i++)
        {
            Vector3Int nextTile = path[i];
            yield return StartCoroutine(MoveToTile(nextTile));
            CurrentTilePosition = nextTile;
        }
    }

    /// <summary>
    /// Finds the closest target (either Player or Enemy) to pursue.
    /// </summary>
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

    /// <summary>
    /// Moves the Barra to a specific tile.
    /// </summary>
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

    /// <summary>
    /// Calculates a simple path towards the target tile.
    /// </summary>
    private List<Vector3Int> CalculatePath(Vector3Int start, Vector3Int target)
    {
        List<Vector3Int> path = new List<Vector3Int>();

        int dx = target.x - start.x;
        int dy = target.y - start.y;

        // Move horizontally first, then vertically
        for (int i = 0; i < Mathf.Abs(dx); i++)
        {
            path.Add(new Vector3Int(start.x + (dx > 0 ? 1 : -1), start.y, start.z));
            start = path[path.Count - 1];
        }
        for (int i = 0; i < Mathf.Abs(dy); i++)
        {
            path.Add(new Vector3Int(start.x, start.y + (dy > 0 ? 1 : -1), start.z));
            start = path[path.Count - 1];
        }

        return path;
    }
}
