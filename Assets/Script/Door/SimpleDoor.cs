using UnityEngine;
using UnityEngine.Events;

public class SimpleDoor : MonoBehaviour
{
    [Header("見た目")]
    [SerializeField] private SpriteRenderer doorRenderer;  // ドアの SpriteRenderer
    [SerializeField] private Sprite closedSprite;          // 閉じたときのスプライト
    [SerializeField] private Sprite openSprite;            // 開いたときのスプライト（なければ null でもOK）

    [Header("当たり判定")]
    [SerializeField] private Collider2D colliderWhenClosed; // 閉時 ON
    [SerializeField] private Collider2D colliderWhenOpen;   // 開時 ON（不要なら空）

    [Header("初期状態")]
    [SerializeField] private bool isOpen = false;

    public UnityEvent OnOpened;
    public UnityEvent OnClosed;

    private void Awake()
    {
        Apply();
    }

    public void Open()
    {
        if (isOpen) return;
        isOpen = true;
        Apply();
        OnOpened?.Invoke();
    }

    public void Close()
    {
        if (!isOpen) return;
        isOpen = false;
        Apply();
        OnClosed?.Invoke();
    }

    public void Toggle()
    {
        isOpen = !isOpen;
        Apply();
        if (isOpen) OnOpened?.Invoke(); else OnClosed?.Invoke();
    }

    private void Apply()
    {
        // ★ コライダー切り替え
        if (colliderWhenClosed) colliderWhenClosed.enabled = !isOpen;
        if (colliderWhenOpen)   colliderWhenOpen.enabled   =  isOpen;

        // ★ スプライト切り替え
        if (doorRenderer)
        {
            if (isOpen && openSprite != null)
                doorRenderer.sprite = openSprite;
            else if (!isOpen && closedSprite != null)
                doorRenderer.sprite = closedSprite;
        }
    }
}
