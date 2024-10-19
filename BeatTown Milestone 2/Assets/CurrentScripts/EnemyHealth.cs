using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class EnemyHealth : MonoBehaviour
{
    public int maxHealth = 3; // Maximum health value
    public int health;        // Current health

    public bool IsDead { get; private set; } = false; // Flag to track if the enemy is dead

    // Event to notify when the enemy dies
    public event Action OnDeath;

    void Start()
    {
        health = maxHealth; // Initialize health
    }

    public void TakeDamage(int amount)
    {
        if (IsDead) return; // Do nothing if already dead

        health -= amount;
        Debug.Log($"{gameObject.name} took {amount} damage! Remaining health: {health}");

        if (health <= 0)
        {
            Die(); // Implement death logic
        }
    }

    public void ResetHealth()
    {
        health = maxHealth;
        IsDead = false;
        Debug.Log($"{gameObject.name} has been respawned with full health.");
    }

    public void Die()
    {
        if (IsDead) return; // Prevent multiple deaths

        Debug.Log($"{gameObject.name} has died.");
        IsDead = true;

        // Invoke the death event
        OnDeath?.Invoke();

        // Disable enemy's behavior (optional)
        AIMove aiMove = GetComponent<AIMove>();
        if (aiMove != null)
        {
            aiMove.enabled = false;
        }

        // If this is BarraAI, handle accordingly
        BarraAI barraAI = GetComponent<BarraAI>();
        if (barraAI != null)
        {
            barraAI.enabled = false;
        }

        // Start respawn coroutine
        StartCoroutine(RespawnEnemy());
    }

    private IEnumerator RespawnEnemy()
    {
        // Wait for a short duration before respawning
        yield return new WaitForSeconds(0f);

        // Respawn logic
        RespawnManager.Instance.RespawnEnemy(gameObject);
    }
}
