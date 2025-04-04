using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;
using static StateMachine;

public class Punch : MonoBehaviour
{
    public TempTurnBase tempTurnBase;
    public GameObject SwingHighlight;
    public GameObject PPShighlight;
    public GameObject moveMentHighlight;
    public Tilemap tilemap;
    private Transform selectedEnemy;
    private bool isPunching;
    private PlayerMove playerMove;
    private PlayerFatigue playerFatigue;
    public int punchDamage = 1;
    private StateMachine stateMachine;
    public All_SFX All_SFX;
    public GameObject RealMoveHighlight;
    public GameObject jumpHighlight;

    private void Awake()
    {
        playerMove = GetComponent<PlayerMove>();
        playerFatigue = GetComponent<PlayerFatigue>();
        stateMachine = GetComponent<StateMachine>();
        tempTurnBase = FindObjectOfType<TempTurnBase>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (isPunching)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

                if (hit.collider != null)
                {
                    if (hit.collider != null && (hit.collider.CompareTag("Enemy") || hit.collider.CompareTag("Goon") || hit.collider.CompareTag("Electrician") || hit.collider.CompareTag("Barra")))
                    {
                        Vector3Int enemyPosition = tilemap.WorldToCell(hit.collider.transform.position);
                        Vector3Int playerPosition = tilemap.WorldToCell(transform.position);

                        if (IsWithinPunchRange(playerPosition, enemyPosition))
                        {
                            selectedEnemy = hit.collider.transform;
                            FlipPlayerIfNeeded(enemyPosition);
                            TryPunchEnemy();
                        }
                        else
                        {
                            Debug.Log("Selected enemy is out of punch range.");
                        }
                    }
                }
            }
        }
    }

    public void OnPunchButtonPressed()
    {
        if (playerFatigue.CanPerformAction(playerFatigue.punchFatigueCost) && !playerFatigue.lockedMovement)
        {
            tempTurnBase.ResetAllColliders();

            RealMoveHighlight.SetActive(false);
            SwingHighlight.SetActive(false);
            PPShighlight.SetActive(true);
            moveMentHighlight.SetActive(false);
            jumpHighlight.SetActive(false);

            isPunching = true;
            selectedEnemy = null;
            playerMove.CurrentAction = ActionType.Punch;
            Debug.Log("Punch button pressed, current action: " + playerMove.CurrentAction);
            CheckEnemiesInRange();
        }
        else
        {
            Debug.Log("Not enough fatigue to punch.");
        }
    }

    public void CancelPunch()
    {
        isPunching = false;
        selectedEnemy = null;
        playerMove.CurrentAction = ActionType.None;
        Debug.Log("Punch action canceled.");
    }
    IEnumerator DelayedFishSlap()
    {
        yield return new WaitForSeconds(0.6f);
        All_SFX.PlayFishSlap();
    }

    void TryPunchEnemy()
    {
        if (selectedEnemy != null)
        {
            // Trigger animation first
            stateMachine.ChangeState(WrestlerState.Punch);

            //All_SFX.PlayFishSlap();
            StartCoroutine(DelayedFishSlap());

            // Start the coroutine to delay damage application
            StartCoroutine(DelayedPunchDamage(selectedEnemy));

            // Consume fatigue immediately
            playerFatigue.UseFatigue(playerFatigue.punchFatigueCost);

            // Disable highlights and reset state
            PPShighlight.SetActive(false);
            isPunching = false;
            playerMove.CurrentAction = ActionType.None;
            
        }
        else
        {
            Debug.Log("No enemy selected to punch.");
        }
    }

    IEnumerator DelayedPunchDamage(Transform enemy)
    {
        yield return new WaitForSeconds(0.6f); // Adjust delay to match animation timing

        if (enemy != null)
        {
            // Check if the enemy has EnemyHealth (for general enemies)
            EnemyHealth enemyScript = enemy.GetComponent<EnemyHealth>();
            GoonHealth goonHealth = enemy.GetComponent<GoonHealth>();

            if (enemyScript != null)
            {
                enemyScript.TakeDamage(punchDamage, true);
                Debug.Log($"{enemy.name} has been punched and took {punchDamage} damage!");
            }
            else if (goonHealth != null)
            {
                goonHealth.TakeDamage(punchDamage);
                Debug.Log($"{enemy.name} (Goon) has been punched and took {punchDamage} damage!");
            }
            else
            {
                Debug.Log("Selected enemy does not have a valid damage method.");
            }

            // Ensure Goon punch telegraph updates
            GoonMove goonMove = enemy.GetComponent<GoonMove>();
            if (goonMove != null)
            {
                goonMove.OnPunchedByPlayer();
            }

            // If the enemy is a regular AI, have them chase the player
            AIMove aiMoveScript = enemy.GetComponent<AIMove>();
            if (aiMoveScript != null)
            {
                aiMoveScript.SetFollowPlayerForTurns(3);
            }
        }
    }


    bool IsWithinPunchRange(Vector3Int playerPosition, Vector3Int enemyPosition)
    {
        return (Mathf.Abs(playerPosition.x - enemyPosition.x) + Mathf.Abs(playerPosition.y - enemyPosition.y) == 1);
    }

    private void CheckEnemiesInRange()
    {
        Vector3Int playerCurrentPosition = tilemap.WorldToCell(transform.position);
        foreach (GameObject enemyObj in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            Transform enemy = enemyObj.transform;
            Vector3Int enemyPosition = tilemap.WorldToCell(enemy.position);
            if (IsWithinPunchRange(playerCurrentPosition, enemyPosition))
            {
                Debug.Log($"{enemy.name} is within punch range!");
                break;
            }
        }
    }

    private void FlipPlayerIfNeeded(Vector3Int enemyPosition)
    {
        Vector3Int playerPosition = tilemap.WorldToCell(transform.position);

        if (enemyPosition.x < playerPosition.x && transform.localScale.x > 0) // Enemy is to the left
        {
            FlipPlayer();
        }
        else if (enemyPosition.x > playerPosition.x && transform.localScale.x < 0) // Enemy is to the right
        {
            FlipPlayer();
        }
    }

    private void FlipPlayer()
    {
        Vector3 localScale = transform.localScale;
        localScale.x *= -1; // Flip the player horizontally
        transform.localScale = localScale;
    }
}
