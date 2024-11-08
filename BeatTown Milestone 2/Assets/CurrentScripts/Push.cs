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
    public All_SFX All_SFX;
    public GameObject RealMoveHighlight;

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
            if (isPushing && playerFatigue.currentFatigue > 0)
            {
                if (selectedTarget != null)
                {
                    TryPushTarget();
                }
                else
                {
                    SelectTarget();
                }
            }else if(isPushing && playerFatigue.currentFatigue <= 0)
            {
                Debug.Log("No Fatigue to Push");
                CancelPush();
            }
        }
    }

    public void OnPushButtonPressed()
    {
        if (playerFatigue.CanPerformAction(playerFatigue.punchFatigueCost))
        {
            RealMoveHighlight.SetActive(false);
            SwingHighlight.SetActive(false);
            PPShighlight.SetActive(true);
            moveMentHighlight.SetActive(false);
            playerMove.CancelMove();
            isPushing = true;
            selectedTarget = null;
            Debug.Log("Push button pressed, current action: " + playerMove.CurrentAction);
        }
        else
        {
            Debug.Log("Not enough fatigue to push.");
        }
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
        All_SFX.PlayPush();
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

        AIMove targetAIMove = target.GetComponent<AIMove>();
        BarraMove targetBarraMove = target.GetComponent<BarraMove>();
        if (targetAIMove != null)
        {
            OccupiedTilesManager.Instance.RemoveOccupiedPosition(targetAIMove.CurrentTilePosition);
            targetAIMove.CurrentTilePosition = targetTilePosition;
            OccupiedTilesManager.Instance.AddOccupiedPosition(targetAIMove.CurrentTilePosition);
        }
        else if (targetBarraMove != null)
        {
            OccupiedTilesManager.Instance.RemoveOccupiedPosition(targetBarraMove.CurrentTilePosition);
            targetBarraMove.CurrentTilePosition = targetTilePosition;
            OccupiedTilesManager.Instance.AddOccupiedPosition(targetBarraMove.CurrentTilePosition);
        }

        Vector3Int targetTilePos = targetTilePosition;
        Vector3Int hookTilePos = hook != null ? hook.GetHookPosition() : new Vector3Int();

        if (hook != null && targetTilePos == hookTilePos)
        {
            hook.HandleSwingOrPushIntoHook(target.gameObject);
        }
    }
}
