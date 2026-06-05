using UnityEngine;
using UnityEngine.UI;

public class MinigameHeartUI : MonoBehaviour
{
    [System.Serializable]
    public class HeartSpriteEntry
    {
        [Header("Counts")]
        [Range(0, 3)] public int gold;
        [Range(0, 3)] public int red;
        [Range(0, 3)] public int black;

        [Header("Sprite")]
        public Sprite sprite;
    }

    [Header("References")]
    [SerializeField] private PlayerHeartSystem heartSystem;
    [SerializeField] private Image heartImage;

    [Header("Sprite Table")]
    [SerializeField] private HeartSpriteEntry[] spriteTable;

    [Header("Fallback")]
    [SerializeField] private bool useFallbackSpriteIfNotFound = false;
    [SerializeField] private Sprite fallbackSprite;

    private void Reset()
    {
        heartImage = GetComponent<Image>();
    }

    private void OnEnable()
    {
        if (heartSystem != null)
        {
            heartSystem.OnHeartChanged += Refresh;
        }
    }

    private void OnDisable()
    {
        if (heartSystem != null)
        {
            heartSystem.OnHeartChanged -= Refresh;
        }
    }

    private void Start()
    {
        Refresh();
    }

    public void Refresh()
    {
        if (heartSystem == null)
        {
            Debug.LogWarning("MinigameHeartUI: heartSystem が未設定です。", this);
            return;
        }

        if (heartImage == null)
        {
            Debug.LogWarning("MinigameHeartUI: heartImage が未設定です。", this);
            return;
        }

        int gold = heartSystem.GoldHearts;
        int red = heartSystem.RedHearts;
        int black = heartSystem.BlackHearts;

        Debug.Log($"[MinigameHeartUI Refresh] gold={gold}, red={red}, black={black}", this);

        Sprite found = FindSprite(gold, red, black);

        if (found != null)
        {
            heartImage.sprite = found;
            Debug.Log($"[MinigameHeartUI] sprite changed -> {found.name}", this);
            return;
        }

        Debug.LogWarning(
            $"MinigameHeartUI: 対応するHPスプライトが未登録です。(gold={gold}, red={red}, black={black})",
            this
        );

        if (useFallbackSpriteIfNotFound && fallbackSprite != null)
        {
            heartImage.sprite = fallbackSprite;
        }
    }

    private Sprite FindSprite(int gold, int red, int black)
    {
        if (spriteTable == null) return null;

        for (int i = 0; i < spriteTable.Length; i++)
        {
            HeartSpriteEntry entry = spriteTable[i];
            if (entry == null) continue;

            if (entry.gold == gold &&
                entry.red == red &&
                entry.black == black)
            {
                return entry.sprite;
            }
        }

        return null;
    }

#if UNITY_EDITOR
    [ContextMenu("Log Current Heart State")]
    private void LogCurrentHeartState()
    {
        if (heartSystem == null)
        {
            Debug.Log("heartSystem 未設定", this);
            return;
        }

        Debug.Log(
            $"Current Heart State => gold={heartSystem.GoldHearts}, red={heartSystem.RedHearts}, black={heartSystem.BlackHearts}",
            this
        );
    }
#endif
}