//using UnityEngine;

//public class AIAttack : MonoBehaviour
//{
//    protected GameManager2 gameManager;
//    protected PlayerHealth playerHealth;
//    protected int attackDamage = 1;  // Damage dealt by the AI

//    void Start()
//    {
//        gameManager = FindObjectOfType<GameManager2>();
//        playerHealth = gameManager.player.GetComponent<PlayerHealth>();

//        if (playerHealth == null)
//        {
//            Debug.LogError("PlayerHealth script not found on the player.");
//        }
//    }

//    // Method to perform the attack
//    public virtual void Attack()
//    {
//        if (IsPlayerInRange())
//        {
//            playerHealth.TakeDamage(attackDamage);
//            Debug.Log("AI attacked the player for " + attackDamage + " damage.");
//        }
//        else
//        {
//            Debug.Log("AI cannot attack: Player is not in range.");
//        }
//    }

//    // Check if the player is adjacent to the AI (within 1 grid space)
//    protected bool IsPlayerInRange()
//    {
//        Vector2Int aiPos = GetComponent<AIMove>().currentGridPos;
//        Vector2Int playerPos = gameManager.playerMoveScript.currentGridPos;

//        int xDistance = Mathf.Abs(aiPos.x - playerPos.x);
//        int yDistance = Mathf.Abs(aiPos.y - playerPos.y);

//        // Player is in range if adjacent (orthogonally, not diagonally)
//        return (xDistance == 1 && yDistance == 0) || (xDistance == 0 && yDistance == 1);
//    }
//}
