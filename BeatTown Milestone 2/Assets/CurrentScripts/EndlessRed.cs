using UnityEngine;

public class EndlessRed : MonoBehaviour
{
    [SerializeField] public GameObject bg1;
    public GameObject bg2;

    private void OnEnable()
    {
        Hook.OnHookKillCountChanged += HandleKillCountChanged;
        Hook.OnHookSpawned += OnHookSpawned;
    }

    private void OnDisable()
    {
        Hook.OnHookKillCountChanged -= HandleKillCountChanged;
        Hook.OnHookSpawned -= OnHookSpawned;
    }

    private void OnHookSpawned(Hook hook)
    {
        // Optional: do something when hook spawns
        HandleKillCountChanged(hook.hookKillCount); // check immediately
    }

    private void HandleKillCountChanged(int killCount)
    {
        if (bg1 == null || bg2 == null)
        {
            Debug.LogError("bg1 or bg2 is not assigned in EndlessRed!");
            return;
        }

        if (killCount >= 10)
        {
            bg1.SetActive(false);
            bg2.SetActive(true);
        }
    }
}
