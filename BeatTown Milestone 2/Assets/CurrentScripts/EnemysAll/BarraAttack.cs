using System.Collections;
using UnityEngine;
using static StateMachine;

public class BarraAttack : MonoBehaviour
{
    public int attackDamage = 2;
    public float attackRange = 1.5f;
    private StateMachine stateMachine;


    private void Start()
    {
        stateMachine = GetComponent<StateMachine>();
    }

    /// <summary>
    /// Checks if there is a target within attack range.
    /// </summary>
    public bool CanAttack()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attackRange);
        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Player") || hit.CompareTag("Enemy"))
            {
                return true;
            }
        }
        return false;
    }

    public IEnumerator PerformAttack()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attackRange);

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
                    yield break; // Attack only one target per action
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
                    yield break; // Attack only one target per action
                }
            }
        }
        yield return null;
    }
}
