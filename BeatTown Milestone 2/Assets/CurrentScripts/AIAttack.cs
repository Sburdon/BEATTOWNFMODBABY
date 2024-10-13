using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIAttack : MonoBehaviour
{
    public PlayerMove playerMove; // Reference to the player for position checks
    public int attackRange = 1; // Attack range (e.g., 1 tile away)
    public int damage = 1; // Damage dealt by the AI

    private Vector3Int CurrentTilePosition; // AI's current position
    private Vector3Int playerTilePosition; // Player's current position

    private void Start()
    {
        // Initialize AI position and get the player's position
        UpdateAIPosition();
        UpdatePlayerPosition();
    }

    public void TakeTurn()
    {
        UpdateAIPosition();
        UpdatePlayerPosition();

        if (IsNextToPlayer())
        {
            AttackPlayer();
        }
    }

    private void UpdateAIPosition()
    {
        // Recalculate AI position on the grid
        CurrentTilePosition = playerMove.tilemap.WorldToCell(transform.position);
        Debug.Log($"{gameObject.name} position updated to {CurrentTilePosition}.");
    }

    private void UpdatePlayerPosition()
    {
        // Recalculate player's position on the grid
        playerTilePosition = playerMove.CurrentTilePosition;
        Debug.Log($"Player position updated to {playerTilePosition}.");
    }

    private bool IsNextToPlayer()
    {
        int deltaX = Mathf.Abs(CurrentTilePosition.x - playerTilePosition.x);
        int deltaY = Mathf.Abs(CurrentTilePosition.y - playerTilePosition.y);

        return (deltaX + deltaY == attackRange); // Return true if AI is within attack range
    }

    private void AttackPlayer()
    {
        Debug.Log($"{gameObject.name} attacks the player!");
        // Implement attack logic, e.g., reduce player's health
        
    }
}
