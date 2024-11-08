using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;
using static StateMachine;

public class Swing : MonoBehaviour
{
    public GameObject SwingHighlight;
    public GameObject PPShighlight;
    public GameObject moveMentHighlight;
    public Tilemap tilemap;
    public Hook hook; // Hook will be assigned later by RespawnManager
    public float swingSpeed = 5f;
    private GameObject targetToSwing;
    private Vector3Int targetTilePosition;
    private bool isSwingMode = false;
    private bool isSwinging = false;
    private PlayerFatigue playerFatigue;
    public int swingFatigueCost = 2;
    private StateMachine stateMachine;
    public All_SFX All_SFX;
    public GameObject RealMoveHighlight;

    void Awake()
    {
        playerFatigue = GetComponent<PlayerFatigue>();
        stateMachine = GetComponent<StateMachine>();
    }

    public void SetHookReference(Hook hookInstance)
    {
        hook = hookInstance;
        Debug.Log("Hook instance assigned to Swing script successfully.");
    }

    public void OnSwingButtonPressed()
    {
        if (playerFatigue.CanPerformAction(playerFatigue.swingFatigueCost))
        {
            RealMoveHighlight.SetActive(false);
            PPShighlight.SetActive(true);
            SwingHighlight.SetActive(false);
            moveMentHighlight.SetActive(false);
            if (isSwinging)
            {
                Debug.Log("Already swinging.");
                return;
            }

            if (isSwingMode)
            {
                Debug.Log("Swing mode already active.");
                return;
            }

            isSwingMode = true;
            Debug.Log("Swing mode activated. Click on an adjacent enemy or Barra to swing.");
        }
        else
        {
            Debug.Log("Not enough fatigue to swing.");
        }
    }

        void Update()
    {
        if (isSwingMode && !isSwinging)
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (targetToSwing == null)
                {
                    SelectTarget();
                }
                else
                {
                    Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    Vector3Int clickedTilePosition = tilemap.WorldToCell(mouseWorldPosition);
                    Vector3Int playerTilePosition = tilemap.WorldToCell(transform.position);

                    if (AIUtils.IsAdjacent(playerTilePosition, clickedTilePosition) &&
                        clickedTilePosition != playerTilePosition &&
                        clickedTilePosition != tilemap.WorldToCell(targetToSwing.transform.position))
                    {
                        if (AIUtils.IsTileValid(tilemap, OccupiedTilesManager.Instance, clickedTilePosition, hook))
                        {
                            targetTilePosition = clickedTilePosition;
                            playerFatigue.UseFatigue(swingFatigueCost);
                            StartCoroutine(SwingTarget(targetToSwing, targetTilePosition));

                            isSwingMode = false;
                            targetToSwing = null;
                        }
                        else
                        {
                            Debug.Log("Invalid tile for swinging.");
                        }
                    }
                    else
                    {
                        Debug.Log("Selected tile is not a valid target.");
                    }
                }
            }
            else if (Input.GetMouseButtonDown(1))
            {
                Debug.Log("Swing action canceled.");
                isSwingMode = false;
                targetToSwing = null;
            }
        }
    }

    private void SelectTarget()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

        if (hit.collider != null && (hit.collider.CompareTag("Enemy") || hit.collider.CompareTag("Barra")))
        {
            Vector3Int targetPosition = tilemap.WorldToCell(hit.collider.transform.position);
            Vector3Int playerPosition = tilemap.WorldToCell(transform.position);

            if (AIUtils.IsAdjacent(playerPosition, targetPosition))
            {
                targetToSwing = hit.collider.gameObject; // Select the enemy
                Debug.Log($"Selected enemy for swing: {targetToSwing.name}");
                PPShighlight.SetActive(false);
                SwingHighlight.SetActive(true);
            }
            else
            {
                Debug.Log("Selected target is out of swing range.");
            }
        }
        else
        {
            Debug.Log("No enemy or Barra selected.");
        }
    }

    private IEnumerator SwingTarget(GameObject target, Vector3Int targetTilePosition)
    {
        isSwinging = true;

        Vector3 startPos = target.transform.position;
        Vector3 endPos = tilemap.GetCellCenterWorld(targetTilePosition);
        stateMachine.ChangeState(WrestlerState.Swing);
        All_SFX.PlaySwing();

        float elapsedTime = 0f;
        float duration = 1f / swingSpeed;

        EnemyHealth targetHealth = target.GetComponent<EnemyHealth>();
        bool targetDied = false;

        void OnTargetDeath()
        {
            targetDied = true;
        }

        if (targetHealth != null)
        {
            targetHealth.OnDeath += OnTargetDeath;
        }

        while (elapsedTime < duration)
        {
            if (targetDied)
            {
                SwingHighlight.SetActive(false);
                Debug.Log("Target died during swing. Stopping movement.");
                break;
            }

            target.transform.position = Vector3.Lerp(startPos, endPos, elapsedTime / duration);
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

        target.transform.position = endPos;

        Debug.Log($"{target.name} has been swung to {targetTilePosition}");

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
            hook.HandleSwingOrPushIntoHook(target);
        }

        isSwinging = false;
        Debug.Log("Swing action completed.");
        PPShighlight.SetActive(false);
        moveMentHighlight.SetActive(false);
        SwingHighlight.SetActive(false);
    }

    public bool IsSwinging()
    {
        return isSwinging;
    }

    public void CancelSwing()
    {
        isSwingMode = false;
        isSwinging = false;
        targetToSwing = null;
        Debug.Log("Swing action canceled.");
    }
}

// Utility Class for AI-related Functions
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
