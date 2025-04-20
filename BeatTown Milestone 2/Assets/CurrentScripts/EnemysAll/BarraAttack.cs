using System.Collections;
using UnityEngine;
using static StateMachine;

public class BarraAttack : MonoBehaviour
{
    public int attackDamage = 2;
    public float attackRange = 1.5f;
    private StateMachine stateMachine;
    private BarraMove barraMove;

    private void Start()
    {
        stateMachine = GetComponent<StateMachine>();
        barraMove = GetComponent<BarraMove>();
    }

    /// <summary>
    /// Checks if there is a target within attack range.
    /// </summary>
    public bool CanAttack()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attackRange);
        foreach (Collider2D hit in hits)
        {
            if (barraMove != null && barraMove.isTaunted)
            {
                if (hit.CompareTag("Player"))
                {
                    return true;
                }
            }
            else
            {
                if (hit.CompareTag("Player") || hit.CompareTag("Enemy"))
                {
                    return true;
                }
            }
        }
        return false;
    }

    public IEnumerator PerformAttack()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attackRange);

        // If taunted, prioritize player only
        if (barraMove != null && barraMove.isTaunted)
        {
            foreach (Collider2D hit in hits)
            {
                if (hit.CompareTag("Player"))
                {
                    stateMachine.ChangeState(WrestlerState.Punch);
                    yield return new WaitForSeconds(0.6f);
                    PlayerHealth playerHealth = hit.GetComponent<PlayerHealth>();
                    if (playerHealth != null)
                    {
                        playerHealth.TakeDamage(attackDamage);
                        Debug.Log($"{gameObject.name} (taunted) attacked Player for {attackDamage} damage.");
                        yield break;
                    }
                }
            }
        }
        else
        {
            foreach (Collider2D hit in hits)
            {
                if (hit.CompareTag("Player"))
                {
                    stateMachine.ChangeState(WrestlerState.Punch);
                    yield return new WaitForSeconds(0.6f);
                    PlayerHealth playerHealth = hit.GetComponent<PlayerHealth>();
                    if (playerHealth != null)
                    {
                        playerHealth.TakeDamage(attackDamage);
                        Debug.Log($"{gameObject.name} attacked Player for {attackDamage} damage.");
                        yield break;
                    }
                }
                else if (hit.CompareTag("Enemy"))
                {
                    stateMachine.ChangeState(WrestlerState.Punch);
                    yield return new WaitForSeconds(0.6f);
                    EnemyHealth enemyHealth = hit.GetComponent<EnemyHealth>();
                    if (enemyHealth != null)
                    {
                        enemyHealth.TakeDamage(attackDamage, true);
                        Debug.Log($"{gameObject.name} attacked {enemyHealth.gameObject.name} for {attackDamage} damage.");
                        yield break;
                    }
                }
            }
        }

        yield return null;
    }
}
