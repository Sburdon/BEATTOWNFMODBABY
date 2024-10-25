// Assets/CurrentScripts/AIAttack.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    public float attackRange = 1.5f;
    public int attackDamage = 1;
    public PlayerHealth playerHealth; // Reference to the player's health script

    void Update()
    {
        // Optionally, you can call AttackIfInRange here or elsewhere as needed
        // For example:
        // AttackIfInRange();
    }

    /// <summary>
    /// Checks if any valid targets are within attack range and attacks them.
    /// </summary>
    virtual public void AttackIfInRange()
    {
        // Detect all colliders within attack range
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, attackRange);

        foreach (var hitCollider in hitColliders)
        {
            // Prevent attacking itself
            if (hitCollider.gameObject == this.gameObject)
                continue;

            // Exclude attacking other BarraAI units
            if (hitCollider.CompareTag("BarraAI"))
                continue;

            // Attack the player
            if (hitCollider.CompareTag("Player"))
            {
                PlayerHealth ph = hitCollider.GetComponent<PlayerHealth>();
                if (ph != null && !ph.IsDead)
                {
                    ph.TakeDamage(attackDamage);
                    Debug.Log($"{gameObject.name} attacked the player for {attackDamage} damage.");
                }
            }
            // Attack other standard enemies
            else if (hitCollider.CompareTag("Enemy"))
            {
                EnemyHealth eh = hitCollider.GetComponent<EnemyHealth>();
                if (eh != null && !eh.IsDead)
                {
                    eh.TakeDamage(attackDamage);
                    Debug.Log($"{gameObject.name} attacked {eh.gameObject.name} for {attackDamage} damage.");
                }
            }
        }
    }

    /// <summary>
    /// Visualizes the attack range in the Unity Editor.
    /// </summary>
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
