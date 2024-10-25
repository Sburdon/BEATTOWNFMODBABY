using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using static StateMachine;

public class Push : MonoBehaviour
{
    public GameObject SwingHighlight;
    public GameObject PPShighlight;
    public GameObject moveMentHighlight;
    public Tilemap tilemap;
    public Hook hook; // Hook will be assigned later by RespawnManager
    private Transform selectedTarget;
    private bool isPushing;
    private PlayerMove playerMove;
    private PlayerFatigue playerFatigue;
    private StateMachine stateMachine;

    void Awake()
    {
        playerMove = GetComponent<PlayerMove>();
        playerFatigue = GetComponent<PlayerFatigue>();
        stateMachine = GetComponent<StateMachine>();
    }

    public void SetHookReference(Hook hookInstance)
    {
        hook = hookInstance;
        Debug.Log("Hook instance assigned to Push script successfully.");
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (isPushing)
            {
                if (selectedTarget != null)
                {
                    TryPushTarget();
                }
                else
                {
                    SelectTarget();
                }
            }
        }
    }

    public void OnPushButtonPressed()
    {
        SwingHighlight.SetActive(false);
        PPShighlight.SetActive(true);
        moveMentHighlight.SetActive(false);
        // Cancel any movement when the push button is pressed
        playerMove.CancelMove();
        isPushing = true;
        selectedTarget = null;
        Debug.Log("Push button pressed, current action: " + playerMove.CurrentAction);
    }

    public void CancelPush()
    {
        isPushing = false;
        selectedTarget = null;
        Debug.Log("Push action canceled.");
    }

    public bool IsPushing()
    {
        return isPushing;
    }

    void SelectTarget()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

        if (hit.collider != null && (hit.collider.CompareTag("Enemy") || hit.collider.CompareTag("Barra")))
        {
            Transform target = hit.collider.transform;

            Vector3Int playerPosition = playerMove.CurrentTilePosition;
            Vector3Int targetPosition = tilemap.WorldToCell(target.position);

            int deltaX = Mathf.Abs(targetPosition.x - playerPosition.x);
            int deltaY = Mathf.Abs(targetPosition.y - playerPosition.y);

            if (deltaX + deltaY == 1)
            {
                selectedTarget = target;
                Debug.Log($"Selected target: {selectedTarget.name}");
            }
            else
            {
                Debug.Log("Target is not adjacent to the player (1 tile away in cardinal directions).");
            }
        }
    }

    void TryPushTarget()
    {
        if (selectedTarget != null)
        {
            Vector3Int playerPosition = playerMove.CurrentTilePosition;
            Vector3Int targetPosition = tilemap.WorldToCell(selectedTarget.position);

            Vector3Int direction = Vector3Int.zero;

            if (playerPosition.x < targetPosition.x)
                direction = Vector3Int.right;
            else if (playerPosition.x > targetPosition.x)
                direction = Vector3Int.left;
            else if (playerPosition.y < targetPosition.y)
                direction = Vector3Int.up;
            else if (playerPosition.y > targetPosition.y)
                direction = Vector3Int.down;

            Vector3Int furthestTile = FindFurthestTile(targetPosition, direction);

            if (furthestTile != targetPosition)
            {
                OccupiedTilesManager.Instance.RemoveOccupiedPosition(targetPosition);

                StartCoroutine(PushTargetToTile(selectedTarget, furthestTile));
                playerFatigue.UseFatigue(playerFatigue.pushFatigueCost);
                selectedTarget = null;
                isPushing = false;
            }
            else
            {
                Debug.Log("No valid tile to push to.");
            }
        }
        else
        {
            Debug.Log("No target selected for push.");
        }
    }

    Vector3Int FindFurthestTile(Vector3Int startTile, Vector3Int direction)
    {
        Vector3Int currentTile = startTile;
        while (AIUtils.IsTileValid(tilemap, OccupiedTilesManager.Instance, currentTile + direction, hook))
        {
            currentTile += direction;
        }
        return currentTile;
    }

    private IEnumerator PushTargetToTile(Transform target, Vector3Int targetTilePosition)
    {
        PPShighlight.SetActive(false);
        stateMachine.ChangeState(WrestlerState.Push);
        Vector3 startPosition = target.position;
        Vector3 endPosition = tilemap.GetCellCenterWorld(targetTilePosition);
        float travelTime = 0.5f;
        float elapsedTime = 0f;

        EnemyHealth targetHealth = target.GetComponent<EnemyHealth>();

        bool targetDied = false;
        void OnTargetDeath() { targetDied = true; }

        if (targetHealth != null)
        {
            targetHealth.OnDeath += OnTargetDeath;
        }

        while (elapsedTime < travelTime)
        {
            if (targetDied)
            {
                Debug.Log("Target died during push. Stopping movement.");
                break;
            }

            target.position = Vector3.Lerp(startPosition, endPosition, elapsedTime / travelTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        if (targetHealth != null)
        {
            targetHealth.OnDeath -= OnTargetDeath;
        }

        if (targetDied)
        {
            yield break;
        }

        target.position = endPosition;
        Debug.Log($"{target.name} has been pushed to {targetTilePosition}");

        AIMove targetMove = target.GetComponent<AIMove>();
        if (targetMove != null)
        {
            OccupiedTilesManager.Instance.RemoveOccupiedPosition(targetMove.CurrentTilePosition);
            targetMove.CurrentTilePosition = targetTilePosition;
            OccupiedTilesManager.Instance.AddOccupiedPosition(targetMove.CurrentTilePosition);
        }

        Vector3Int targetTilePos = targetTilePosition;
        Vector3Int hookTilePos = hook != null ? hook.GetHookPosition() : new Vector3Int();

        if (hook != null && targetTilePos == hookTilePos)
        {
            hook.HandleSwingOrPushIntoHook(target.gameObject);
        }
    }

    public static class AIUtils
    {
        public static bool IsAdjacent(Vector3Int origin, Vector3Int target)
        {
            int dx = Mathf.Abs(origin.x - target.x);
            int dy = Mathf.Abs(origin.y - target.y);
            return (dx + dy == 1);
        }

        public static bool IsTileValid(Tilemap tilemap, OccupiedTilesManager occupiedManager, Vector3Int tilePosition, Hook hook = null)
        {
            bool hasTile = tilemap.HasTile(tilePosition);
            bool isOccupied = occupiedManager.IsTileOccupied(tilePosition);

            if (hook != null && tilePosition == hook.GetHookPosition())
            {
                isOccupied = false;
            }

            return hasTile && !isOccupied;
        }
    }
}
