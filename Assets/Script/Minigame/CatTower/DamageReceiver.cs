using UnityEngine;

public class DamageReceiver : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerHeartSystem heartSystem;

    [Header("Invincible Time")]
    [SerializeField] private float invincibleDuration = 0.8f;

    [Header("Debug")]
    [SerializeField] private bool debugLog = true;

    private float invincibleTimer = 0f;

    private void Awake()
    {
        if (heartSystem == null)
        {
            heartSystem = GetComponent<PlayerHeartSystem>();
            if (heartSystem == null)
            {
                heartSystem = GetComponentInParent<PlayerHeartSystem>();
            }
        }
    }

    private void Update()
    {
        if (invincibleTimer > 0f)
        {
            invincibleTimer -= Time.deltaTime;
        }
    }

    public bool TryTakeDamage(int amount = 1)
    {
        if (heartSystem == null)
        {
            if (debugLog) Debug.LogWarning("DamageReceiver: heartSystem is null.", this);
            return false;
        }

        if (heartSystem.IsGameOver)
        {
            if (debugLog) Debug.Log("DamageReceiver: blocked by game over.", this);
            return false;
        }

        if (invincibleTimer > 0f)
        {
            if (debugLog) Debug.Log($"DamageReceiver: blocked by invincibleTimer = {invincibleTimer}", this);
            return false;
        }

        if (amount <= 0)
        {
            if (debugLog) Debug.Log("DamageReceiver: invalid amount.", this);
            return false;
        }

        if (debugLog) Debug.Log($"DamageReceiver: apply damage x{amount}", this);

        for (int i = 0; i < amount; i++)
        {
            heartSystem.TakeDamage();
            if (heartSystem.IsGameOver) break;
        }

        invincibleTimer = invincibleDuration;
        return true;
    }
}