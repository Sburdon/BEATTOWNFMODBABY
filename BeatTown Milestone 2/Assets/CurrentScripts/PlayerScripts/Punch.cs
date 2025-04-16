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
    private Score scores;

    private void Awake()
    {
        scores = FindObjectOfType<Score>();
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
                    if (hit.collider.CompareTag("Enemy") || hit.collider.CompareTag("Goon") || hit.collider.CompareTag("Electrician") || hit.collider.CompareTag("Barra"))
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
            scores.score = scores.score + 1;

            stateMachine.ChangeState(WrestlerState.Punch);
            StartCoroutine(DelayedFishSlap());

            StartCoroutine(DelayedPunchDamage(selectedEnemy));

            playerFatigue.UseFatigue(playerFatigue.punchFatigueCost);
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
        yield return new WaitForSeconds(0.6f);

        if (enemy != null)
        {
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

            GoonMove goonMove = enemy.GetComponent<GoonMove>();
            if (goonMove != null)
            {
                goonMove.OnPunchedByPlayer();
            }

            AIMove aiMoveScript = enemy.GetComponent<AIMove>();
            if (aiMoveScript != null)
            {
                aiMoveScript.SetFollowPlayerForTurns(2);
            }

            // ✅ New: Barra taunt logic
            BarraMove barraMove = enemy.GetComponent<BarraMove>();
            if (barraMove != null)
            {
                barraMove.isTaunted = true;
                barraMove.tauntTurnsRemaining = 1;
                Debug.Log($"{enemy.name} is now taunted and will target the player for 1 turn.");
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

        if (enemyPosition.x < playerPosition.x && transform.localScale.x > 0)
        {
            FlipPlayer();
        }
        else if (enemyPosition.x > playerPosition.x && transform.localScale.x < 0)
        {
            FlipPlayer();
        }
    }

    private void FlipPlayer()
    {
        Vector3 localScale = transform.localScale;
        localScale.x *= -1;
        transform.localScale = localScale;
    }
}
