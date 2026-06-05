using System;
using System.Collections.Generic;
using UnityEngine;

public class AbilityGaugeSystem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerHeartSystem heartSystem;

    [Header("Gauge Settings")]
    [SerializeField] private int currentPoint = 0;
    [SerializeField] private int pointToFill = 5;

    [Header("Per Platform Reward")]
    [SerializeField] private int pointPerPlatform = 1;

    [Header("Debug")]
    [SerializeField] private bool logGainPoint = false;
    [SerializeField] private bool logHeartConversion = false;

    private readonly HashSet<int> clearedPlatformIds = new HashSet<int>();

    public int CurrentPoint => currentPoint;
    public int PointToFill => pointToFill;
    public int PointPerPlatform => pointPerPlatform;

    public float GaugeRate
    {
        get
        {
            if (pointToFill <= 0) return 0f;
            return Mathf.Clamp01((float)currentPoint / pointToFill);
        }
    }

    public bool IsGaugeFull => currentPoint >= pointToFill;

    public event Action OnGaugeChanged;
    public event Action<int> OnPointAdded;
    public event Action OnGoldHeartCreated;

    private void Awake()
    {
        if (pointToFill < 1)
        {
            pointToFill = 1;
        }

        if (pointPerPlatform < 1)
        {
            pointPerPlatform = 1;
        }

        currentPoint = Mathf.Clamp(currentPoint, 0, pointToFill);
        NotifyGaugeChanged();
    }

    public void ResetGauge()
    {
        currentPoint = 0;
        clearedPlatformIds.Clear();
        NotifyGaugeChanged();
    }

    public void SetGaugePoint(int value)
    {
        currentPoint = Mathf.Clamp(value, 0, pointToFill);
        NotifyGaugeChanged();
    }

    public void AddPoint(int amount)
    {
        if (heartSystem != null && heartSystem.IsGameOver) return;
        if (amount <= 0) return;

        currentPoint += amount;

        if (logGainPoint)
        {
            Debug.Log($"AbilityGaugeSystem: +{amount} point  current={currentPoint}/{pointToFill}", this);
        }

        OnPointAdded?.Invoke(amount);

        TryConvertGaugeToGoldHeart();
        NotifyGaugeChanged();
    }

    public bool TryAddPointFromPlatform(int platformId)
    {
        if (heartSystem != null && heartSystem.IsGameOver) return false;

        if (clearedPlatformIds.Contains(platformId))
        {
            return false;
        }

        clearedPlatformIds.Add(platformId);
        AddPoint(pointPerPlatform);
        return true;
    }

    public bool HasPassedPlatform(int platformId)
    {
        return clearedPlatformIds.Contains(platformId);
    }

    public void ClearPassedPlatformHistory()
    {
        clearedPlatformIds.Clear();
    }

    private void TryConvertGaugeToGoldHeart()
    {
        if (heartSystem == null)
        {
            Debug.LogWarning("AbilityGaugeSystem: heartSystem が未設定です。", this);
            currentPoint = Mathf.Clamp(currentPoint, 0, pointToFill);
            return;
        }

        while (currentPoint >= pointToFill)
        {
            bool created = heartSystem.TryGainGoldHeart();

            if (created)
            {
                currentPoint -= pointToFill;
                currentPoint = Mathf.Max(0, currentPoint);

                if (logHeartConversion)
                {
                    Debug.Log("AbilityGaugeSystem: ゲージMAX → 黄色ハートを1つ獲得", this);
                }

                OnGoldHeartCreated?.Invoke();
            }
            else
            {
                // 黄色ハートをこれ以上作れないなら、ゲージはMAX表示で止める
                currentPoint = pointToFill;
                break;
            }
        }
    }

    private void NotifyGaugeChanged()
    {
        OnGaugeChanged?.Invoke();
    }

#if UNITY_EDITOR
    [ContextMenu("Debug Add 1 Point")]
    private void DebugAdd1Point()
    {
        AddPoint(1);
    }

    [ContextMenu("Debug Fill Gauge")]
    private void DebugFillGauge()
    {
        AddPoint(pointToFill);
    }

    [ContextMenu("Debug Reset Gauge")]
    private void DebugResetGauge()
    {
        ResetGauge();
    }
#endif
}