using UnityEngine;
using UnityEngine.UI;

public class AbilityGaugeUI : MonoBehaviour
{
    [System.Serializable]
    public class GaugeSpriteEntry
    {
        [Header("Gauge Step")]
        [Min(0)] public int point;

        [Header("Sprite")]
        public Sprite sprite;
    }

    [Header("References")]
    [SerializeField] private AbilityGaugeSystem gaugeSystem;
    [SerializeField] private Image gaugeImage;

    [Header("Sprite Table")]
    [SerializeField] private GaugeSpriteEntry[] spriteTable;

    [Header("Fallback")]
    [SerializeField] private bool useNearestLowerStepIfNotFound = true;
    [SerializeField] private bool useFallbackSpriteIfNotFound = false;
    [SerializeField] private Sprite fallbackSprite;

    private void Reset()
    {
        gaugeImage = GetComponent<Image>();
    }

    private void OnEnable()
    {
        if (gaugeSystem != null)
        {
            gaugeSystem.OnGaugeChanged += Refresh;
        }
    }

    private void OnDisable()
    {
        if (gaugeSystem != null)
        {
            gaugeSystem.OnGaugeChanged -= Refresh;
        }
    }

    private void Start()
    {
        Refresh();
    }

    public void Refresh()
    {
        if (gaugeSystem == null)
        {
            Debug.LogWarning("AbilityGaugeUI: gaugeSystem が未設定です。", this);
            return;
        }

        if (gaugeImage == null)
        {
            Debug.LogWarning("AbilityGaugeUI: gaugeImage が未設定です。", this);
            return;
        }

        int currentPoint = gaugeSystem.CurrentPoint;
        Sprite found = FindExactSprite(currentPoint);

        if (found == null && useNearestLowerStepIfNotFound)
        {
            found = FindNearestLowerSprite(currentPoint);
        }

        if (found != null)
        {
            gaugeImage.sprite = found;
            return;
        }

        Debug.LogWarning(
            $"AbilityGaugeUI: 対応するゲージスプライトが未登録です。(currentPoint={currentPoint})",
            this
        );

        if (useFallbackSpriteIfNotFound && fallbackSprite != null)
        {
            gaugeImage.sprite = fallbackSprite;
        }
    }

    private Sprite FindExactSprite(int point)
    {
        if (spriteTable == null) return null;

        for (int i = 0; i < spriteTable.Length; i++)
        {
            GaugeSpriteEntry entry = spriteTable[i];
            if (entry == null) continue;

            if (entry.point == point)
            {
                return entry.sprite;
            }
        }

        return null;
    }

    private Sprite FindNearestLowerSprite(int point)
    {
        if (spriteTable == null || spriteTable.Length == 0) return null;

        Sprite bestSprite = null;
        int bestPoint = int.MinValue;

        for (int i = 0; i < spriteTable.Length; i++)
        {
            GaugeSpriteEntry entry = spriteTable[i];
            if (entry == null || entry.sprite == null) continue;

            if (entry.point <= point && entry.point > bestPoint)
            {
                bestPoint = entry.point;
                bestSprite = entry.sprite;
            }
        }

        return bestSprite;
    }

#if UNITY_EDITOR
    [ContextMenu("Log Current Gauge State")]
    private void LogCurrentGaugeState()
    {
        if (gaugeSystem == null)
        {
            Debug.Log("gaugeSystem 未設定", this);
            return;
        }

        Debug.Log(
            $"Current Gauge State => point={gaugeSystem.CurrentPoint}/{gaugeSystem.PointToFill}",
            this
        );
    }
#endif
}