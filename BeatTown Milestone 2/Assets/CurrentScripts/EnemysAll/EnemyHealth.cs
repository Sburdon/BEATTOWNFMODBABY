using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // For UI Slider

public class EnemyHealth : MonoBehaviour
{
    public int maxHealth = 3; // Maximum health value
    [SerializeField]
    private int health;        // Current health

    public GameObject healthSliderPrefab; // Reference to the health slider prefab
    private Slider healthSlider;          // Instance of the slider

    private bool isHovered = false;  // Track if the enemy is being hovered over

    public bool IsDead { get; private set; } = false; // Flag to track if the enemy is dead

    public event System.Action OnDeath;

    private RespawnManager respawnManager;
    private TempTurnBase tempTurnBase;

    public int CurrentHealth
    {
        get { return health; }
        private set
        {
            health = Mathf.Clamp(value, 0, maxHealth);
            UpdateHealthSlider(); // Update the health slider when health changes
        }
    }

    void Start()
    {
        health = maxHealth;
        CurrentHealth = health;

        respawnManager = RespawnManager.Instance;
        tempTurnBase = FindObjectOfType<TempTurnBase>();

        if (respawnManager == null) Debug.LogError("RespawnManager instance not found.");
        if (tempTurnBase == null) Debug.LogError("TempTurnBase instance not found.");

        // Instantiate the slider from the prefab and disable it initially
        if (healthSliderPrefab != null)
        {
            GameObject sliderObj = Instantiate(healthSliderPrefab, transform.position, Quaternion.identity, GameObject.Find("Canvas").transform);
            healthSlider = sliderObj.GetComponent<Slider>();
            healthSlider.gameObject.SetActive(false); // Hide at the start
        }
        else
        {
            Debug.LogError("Health Slider Prefab is not assigned in the Inspector!");
        }
    }

    void Update()
    {
        HandleHoverDetection();
    }

    public void HandleHoverDetection()
    {
        Vector3 mousePosition = Input.mousePosition;
        Vector2 worldMousePosition = Camera.main.ScreenToWorldPoint(mousePosition);
        Collider2D hit = Physics2D.OverlapPoint(worldMousePosition);

        if (hit != null && hit.gameObject == gameObject && !isHovered)
        {
            isHovered = true;
            ShowHealthSlider(true);
        }
        else if ((hit == null || hit.gameObject != gameObject) && isHovered)
        {
            isHovered = false;
            ShowHealthSlider(false);
        }

        // Set the slider's position to the fixed coordinates
        if (healthSlider != null && healthSlider.gameObject.activeSelf)
        {
            RectTransform sliderRect = healthSlider.GetComponent<RectTransform>();
            sliderRect.anchoredPosition = new Vector2(879f, -94.00001f);
        }
    }

    public void ShowHealthSlider(bool show)
    {
        if (healthSlider != null)
        {
            healthSlider.gameObject.SetActive(show);
            UpdateHealthSlider();
        }
    }

    public void TakeDamage(int amount)
    {
        if (IsDead) return;

        CurrentHealth -= amount;
        Debug.Log($"{gameObject.name} took {amount} damage! Remaining health: {CurrentHealth}");

        if (CurrentHealth <= 0)
        {
            Die();
        }
    }

    public void ResetHealth()
    {
        CurrentHealth = maxHealth;
        IsDead = false;
        Debug.Log($"{gameObject.name} has been respawned with full health.");
    }

    public void Die()
    {
        if (IsDead) return;

        Debug.Log($"{gameObject.name} has died.");
        IsDead = true;

        OnDeath?.Invoke();

        if (tempTurnBase != null)
        {
            if (TryGetComponent(out AIMove aiMoveComponent))
            {
                tempTurnBase.RemoveAIUnit(aiMoveComponent);
            }
            else if (TryGetComponent(out BarraMove barraMoveComponent))
            {
                tempTurnBase.RemoveBarraUnit(barraMoveComponent);
            }
        }

        if (respawnManager != null)
        {
            respawnManager.EnemyDied(gameObject);
        }

        gameObject.SetActive(false);
    }

    private void UpdateHealthSlider()
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = CurrentHealth;
        }
    }

    private void OnDestroy()
    {
        if (healthSlider != null)
        {
            Destroy(healthSlider.gameObject); // Clean up the slider when the enemy is destroyed
        }
    }
}
