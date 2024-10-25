using UnityEngine;
using UnityEngine.Tilemaps;
using static StateMachine;

public class Punch : MonoBehaviour
{
    public GameObject PPShighlight;
    public GameObject moveMentHighlight;
    public Tilemap tilemap;
    private Transform selectedTarget;
    private bool isPunching;
    private PlayerMove playerMove;
    private PlayerFatigue playerFatigue;
    public int punchDamage = 1;
    private StateMachine stateMachine;

    private void Awake()
    {
        playerMove = GetComponent<PlayerMove>();
        playerFatigue = GetComponent<PlayerFatigue>();
        stateMachine = GetComponent<StateMachine>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (isPunching)
            {
                if (selectedTarget != null)
                {
                    TryPunchEnemy();
                }
                else
                {
                    SelectEnemy();
                }
            }
        }
    }

    public void OnPunchButtonPressed()
    {
        PPShighlight.SetActive(true);
        moveMentHighlight.SetActive(false);
        isPunching = true; // Activate punching mode
        selectedEnemy = null; // Reset selected enemy
        playerMove.CurrentAction = ActionType.Punch; // Set the current action to Punch
        Debug.Log("Punch button pressed, current action: " + playerMove.CurrentAction);
        CheckEnemiesInRange();
    }

    public void CancelPunch()
    {
        isPunching = false;
        selectedTarget = null;
        playerMove.CurrentAction = ActionType.None;
        Debug.Log("Punch action canceled.");
    }

    void SelectEnemy()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

        if (hit.collider != null && (hit.collider.CompareTag("Enemy") || hit.collider.CompareTag("Barra")))
        {
            Vector3Int targetPosition = tilemap.WorldToCell(hit.collider.transform.position);
            Vector3Int playerPosition = tilemap.WorldToCell(transform.position);

            if (IsWithinPunchRange(playerPosition, targetPosition))
            {
                selectedTarget = hit.collider.transform;
                Debug.Log($"Selected target for punch: {selectedTarget.name}");
            }
            else
            {
                Debug.Log("Selected target is out of punch range.");
            }
        }
    }

    void TryPunchEnemy()
    {
        if (selectedTarget != null)
        {
            EnemyHealth targetHealth = selectedTarget.GetComponent<EnemyHealth>();
            if (targetHealth != null)
            {
                targetHealth.TakeDamage(punchDamage);
                Debug.Log($"{selectedTarget.name} has been punched and took {punchDamage} damage!");
                stateMachine.ChangeState(WrestlerState.Punch);
                playerFatigue.UseFatigue(playerFatigue.punchFatigueCost);
            }

            AIMove enemyMove = selectedTarget.GetComponent<AIMove>();
            if (enemyMove != null)
            {
                enemyMove.SetFollowPlayerTurns(2);
                Debug.Log($"{selectedTarget.name} will follow the player for 2 turns.");
            }

            else
            {
                Debug.Log("Selected enemy does not have a valid damage method.");
            }

            // Reset punch state after attempting to punch
            isPunching = false;
            selectedTarget = null;
            playerMove.CurrentAction = ActionType.None;
        }
        else
        {
            Debug.Log("No target selected to punch.");
        }
    }

    bool IsWithinPunchRange(Vector3Int playerPosition, Vector3Int targetPosition)
    {
        return (Mathf.Abs(playerPosition.x - targetPosition.x) + Mathf.Abs(playerPosition.y - targetPosition.y) == 1);
    }

    private void CheckEnemiesInRange()
    {
        Vector3Int playerCurrentPosition = tilemap.WorldToCell(transform.position);

        // Check each enemy if it is within punching range
        foreach (GameObject enemyObj in GameObject.FindGameObjectsWithTag("AI"))
        {
            Transform enemy = enemyObj.transform;
            Vector3Int enemyPosition = tilemap.WorldToCell(enemy.position);
            if (IsWithinPunchRange(playerCurrentPosition, enemyPosition))
            {
                Debug.Log($"{enemy.name} is within punch range!");
                selectedEnemy = enemy; // Automatically select the enemy in range
                break; // Exit loop after selecting the first found enemy
            }
        }
    }
}
