using UnityEngine;

public class AIAttack : MonoBehaviour
{
    public PlayerHealth playerHealth;
    public int attackDamage = 1;
    public float attackRange = 1f;

    private EnemyHealth enemyHealth;

    void Start()
    {
        enemyHealth = GetComponent<EnemyHealth>();
        if (playerHealth == null)
        {
            // Find the player health component if not assigned
            playerHealth = FindObjectOfType<PlayerHealth>();
        }
    }

    public void AttackIfInRange()
    {
        if (enemyHealth != null && enemyHealth.IsDead)
        {
            return;
        }

        // Calculate distance to player
        float distanceToPlayer = Vector3.Distance(transform.position, playerHealth.transform.position);

        if (distanceToPlayer <= attackRange)
        {
            // Attack the player
            playerHealth.TakeDamage(attackDamage);
            Debug.Log($"{gameObject.name} attacked the player for {attackDamage} damage.");
        }
    }
}
