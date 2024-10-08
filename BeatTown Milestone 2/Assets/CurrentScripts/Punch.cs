using UnityEngine;
using UnityEngine.Tilemaps;

public class Punch : MonoBehaviour
{
    public Tilemap tilemap;
    private Transform selectedEnemy;
    private bool isPunching;
    private PlayerMove playerMove;
    public int punchDamage = 1;

    void Start()
    {
        tilemap = tilemap ?? FindObjectOfType<Tilemap>(); // Finds the Tilemap if not set
        playerMove = playerMove ?? GetComponent<PlayerMove>(); // Ensure PlayerMove is assigned
        if (playerMove == null)
        {
            Debug.LogError("PlayerMove component missing on player prefab.");
        }
    }

    public void OnPunchButtonPressed()
    {
        isPunching = true;
        selectedEnemy = null;
        playerMove.CurrentAction = ActionType.Punch;
        Debug.Log("Punch action started.");
        CheckEnemiesInRange();
    }

    public void CancelPunch()
    {
        if (playerMove == null)
        {
            Debug.LogError("PlayerMove is not assigned!");
            return;
        }

        isPunching = false;
        selectedEnemy = null;
        playerMove.CurrentAction = ActionType.None;
        Debug.Log("Punch action canceled.");
    }

    void SelectEnemy()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

        if (hit.collider != null && hit.collider.CompareTag("Enemy"))
        {
            Vector3Int enemyPosition = tilemap.WorldToCell(hit.collider.transform.position);
            Vector3Int playerPosition = tilemap.WorldToCell(transform.position);

            if (IsWithinPunchRange(playerPosition, enemyPosition))
            {
                selectedEnemy = hit.collider.transform;
                Debug.Log($"Selected enemy: {selectedEnemy.name}");
            }
        }
    }

    void TryPunchEnemy()
    {
        if (selectedEnemy != null)
        {
            EnemyHealth enemyScript = selectedEnemy.GetComponent<EnemyHealth>();
            if (enemyScript != null)
            {
                enemyScript.TakeDamage(punchDamage);
                Debug.Log($"{selectedEnemy.name} punched!");
            }

            isPunching = false;
            selectedEnemy = null;
            playerMove.CurrentAction = ActionType.None;
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
                selectedEnemy = enemy;
                Debug.Log($"{enemy.name} is within punch range.");
                break;
            }
        }
    }
}
