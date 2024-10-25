// Assets/CurrentScripts/EnemyHealth.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class EnemyHealth : MonoBehaviour
{
    public int maxHealth = 3; // Maximum health value
    [SerializeField]
    private int health;        // Current health

    public GameObject fullHealthBarPrefab; // Reference to the full health bar prefab
    public GameObject emptyHealthBarPrefab; // Reference to the empty health bar prefab
    private GameObject fullHealthBar; // Instance of the full health bar
    private GameObject emptyHealthBar; // Instance of the empty health bar

    public bool IsDead { get; private set; } = false; // Flag to track if the enemy is dead

    // Event to notify when the enemy dies
    public event Action OnDeath;

    // Reference to RespawnManager
    private RespawnManager respawnManager;

    // Public property to access current health
    public int CurrentHealth
    {
        get { return health; }
        private set
        {
            health = Mathf.Clamp(value, 0, maxHealth);
            UpdateHealthBar();
        }
    }

    void Start()
    {
        health = maxHealth; // Initialize health
        CurrentHealth = health; // Initialize current health

        // Instantiate both health bar prefabs
        fullHealthBar = Instantiate(fullHealthBarPrefab, transform.position + new Vector3(0, 1, 0), Quaternion.identity, transform);
        emptyHealthBar = Instantiate(emptyHealthBarPrefab, transform.position + new Vector3(0, 1, 0), Quaternion.identity, transform);

        UpdateHealthBar();

        // Find RespawnManager instance
        respawnManager = RespawnManager.Instance;
        if (respawnManager == null)
        {
            Debug.LogError("RespawnManager instance not found in the scene.");
        }
    }

    public void TakeDamage(int amount)
    {
        if (IsDead) return; // Do nothing if already dead

        CurrentHealth -= amount;
        Debug.Log($"{gameObject.name} took {amount} damage! Remaining health: {CurrentHealth}");

        if (CurrentHealth <= 0)
        {
            Die(); // Implement death logic
        }
    }

    public void ResetHealth()
    {
        CurrentHealth = maxHealth;
        IsDead = false;
        UpdateHealthBar(); // Reset health bar
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

        

        // Notify RespawnManager
        if (respawnManager != null)
        {
            respawnManager.EnemyDied(gameObject);
        }

        // Removed the RespawnEnemy coroutine call as RespawnManager handles respawning
    }

    private void UpdateHealthBar()
    {
        if (fullHealthBar != null)
        {
            // Calculate the health percentage
            float healthPercentage = (float)CurrentHealth / maxHealth;

            // Update full health bar's scale based on current health
            fullHealthBar.transform.localScale = new Vector3(healthPercentage, 0.24f, 1); // Scale x based on health

            // Position the health bars above the enemy
            Vector3 healthBarPosition = transform.position + new Vector3(0, 0.7f, 0); // Adjust Y offset as needed
            fullHealthBar.transform.position = healthBarPosition;
            emptyHealthBar.transform.position = healthBarPosition;

            // Optionally adjust empty health bar size
            emptyHealthBar.transform.localScale = new Vector3(1f, 0.24f, 1); // Set to the full size
        }
    }
}
