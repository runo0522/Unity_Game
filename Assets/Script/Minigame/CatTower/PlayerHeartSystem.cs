using System;
using UnityEngine;

public class PlayerHeartSystem : MonoBehaviour
{
    [Header("Heart Settings")]
    [SerializeField] private int maxHeartSlots = 3;
    [SerializeField] private int redHearts = 3;
    [SerializeField] private int goldHearts = 0;

    public int MaxHeartSlots => maxHeartSlots;
    public int RedHearts => redHearts;
    public int GoldHearts => goldHearts;
    public int BlackHearts => maxHeartSlots - redHearts - goldHearts;

    public bool IsGameOver { get; private set; }

    public event Action OnHeartChanged;
    public event Action OnGameOver;

    private void Awake()
    {
        Initialize();
    }

    public void Initialize()
    {
        IsGameOver = false;
        ClampState();
        NotifyHeartChanged();
    }

    public void ResetHearts()
    {
        redHearts = maxHeartSlots;
        goldHearts = 0;
        IsGameOver = false;

        ClampState();
        NotifyHeartChanged();
    }

    public void TakeDamage()
    {
        if (IsGameOver) return;

        Debug.Log($"[PlayerHeartSystem BEFORE] red={redHearts}, gold={goldHearts}, black={BlackHearts}", this);

        if (goldHearts > 0)
        {
            goldHearts--;
            redHearts++;
        }
        else
        {
            if (redHearts > 0)
            {
                redHearts--;
            }
        }

        ClampState();

        Debug.Log($"[PlayerHeartSystem AFTER] red={redHearts}, gold={goldHearts}, black={BlackHearts}", this);

        NotifyHeartChanged();

        if (redHearts <= 0 && goldHearts <= 0)
        {
            TriggerGameOver();
        }
    }

    public bool TryGainGoldHeart()
    {
        if (IsGameOver) return false;

        // これ以上黄色にできない
        if (goldHearts >= maxHeartSlots) return false;

        // 赤がないと黄色化できない
        if (redHearts <= 0) return false;

        redHearts--;
        goldHearts++;

        ClampState();
        NotifyHeartChanged();
        return true;
    }

    public bool TryUseGoldHeart()
    {
        if (IsGameOver) return false;
        if (goldHearts <= 0) return false;

        // 使用後も黄色→赤に戻る
        goldHearts--;
        redHearts++;

        ClampState();
        NotifyHeartChanged();
        return true;
    }

    public void SetHeartsForDebug(int red, int gold)
    {
        redHearts = red;
        goldHearts = gold;
        IsGameOver = false;

        ClampState();
        NotifyHeartChanged();

        if (redHearts <= 0 && goldHearts <= 0)
        {
            TriggerGameOver();
        }
    }

    private void TriggerGameOver()
    {
        if (IsGameOver) return;

        IsGameOver = true;
        OnGameOver?.Invoke();
    }

    private void ClampState()
    {
        if (maxHeartSlots < 1)
        {
            maxHeartSlots = 1;
        }

        redHearts = Mathf.Clamp(redHearts, 0, maxHeartSlots);
        goldHearts = Mathf.Clamp(goldHearts, 0, maxHeartSlots);

        int total = redHearts + goldHearts;
        if (total > maxHeartSlots)
        {
            int overflow = total - maxHeartSlots;

            // 基本的には赤を減らして調整
            redHearts = Mathf.Max(0, redHearts - overflow);
        }
    }

    private void NotifyHeartChanged()
    {
        OnHeartChanged?.Invoke();
    }

    public bool TryHealRedHeart(int amount = 1)
    {
        if (IsGameOver) return false;
        if (amount <= 0) return false;

        int beforeRed = redHearts;

        redHearts += amount;
        ClampState();

        bool healed = redHearts > beforeRed;
        if (healed)
        {
            NotifyHeartChanged();
        }

        return healed;
    }
}