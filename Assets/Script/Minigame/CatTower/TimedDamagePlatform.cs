using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class TimedDamagePlatform : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private string playerTag = "Player";

    [Header("Damage Timing")]
    [SerializeField] private float warningStartTime = 1.0f;
    [SerializeField] private float damageTime = 2.0f;
    [SerializeField] private int damageAmount = 1;

    [Header("Behavior")]
    [SerializeField] private bool resetTimerWhenExit = true;
    [SerializeField] private bool damageOnlyOncePerStay = true;
    [SerializeField] private bool requireStandingFromAbove = true;

    [Header("Visual")]
    [SerializeField] private SpriteRenderer targetRenderer;
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite warningSprite;
    [SerializeField] private Sprite dangerSprite;

    [Header("Optional Flash")]
    [SerializeField] private bool flashDuringWarning = false;
    [SerializeField] private float flashSpeed = 8f;

    private DamageReceiver currentReceiver;
    private Collider2D currentPlayerCollider;

    private float stayTimer;
    private bool hasDamagedThisStay;

    private SpriteRenderer spriteRenderer;
    private Color defaultColor;

    private enum VisualState
    {
        Normal,
        Warning,
        Danger
    }

    private VisualState currentVisualState = VisualState.Normal;

    private void Awake()
    {
        if (targetRenderer == null)
        {
            targetRenderer = GetComponent<SpriteRenderer>();
        }

        spriteRenderer = targetRenderer;

        if (spriteRenderer != null)
        {
            defaultColor = spriteRenderer.color;
        }

        ApplyVisualState(VisualState.Normal, true);
    }

    private void Reset()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = false;
        }

        targetRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (currentReceiver == null || currentPlayerCollider == null)
        {
            ApplyVisualState(VisualState.Normal);
            return;
        }

        if (damageOnlyOncePerStay && hasDamagedThisStay)
        {
            ApplyVisualState(VisualState.Danger);
            return;
        }

        stayTimer += Time.deltaTime;

        if (stayTimer >= damageTime)
        {
            ApplyVisualState(VisualState.Danger);

            bool damaged = currentReceiver.TryTakeDamage(damageAmount);
            if (damaged)
            {
                hasDamagedThisStay = true;
            }

            if (!damageOnlyOncePerStay)
            {
                stayTimer = 0f;
            }

            return;
        }

        if (stayTimer >= warningStartTime)
        {
            ApplyVisualState(VisualState.Warning);
        }
        else
        {
            ApplyVisualState(VisualState.Normal);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryBeginStay(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (currentReceiver != null) return;
        TryBeginStay(collision);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (currentPlayerCollider == null) return;
        if (collision.collider != currentPlayerCollider) return;

        ClearCurrentTarget();
    }

    private void TryBeginStay(Collision2D collision)
    {
        if (!collision.collider.CompareTag(playerTag)) return;

        if (requireStandingFromAbove && !IsStandingFromAbove(collision))
        {
            return;
        }

        DamageReceiver receiver = collision.collider.GetComponent<DamageReceiver>();
        if (receiver == null)
        {
            receiver = collision.collider.GetComponentInParent<DamageReceiver>();
        }

        if (receiver == null) return;

        if (currentPlayerCollider != collision.collider)
        {
            currentPlayerCollider = collision.collider;
            currentReceiver = receiver;
            stayTimer = 0f;
            hasDamagedThisStay = false;
            ApplyVisualState(VisualState.Normal, true);
        }
    }

    private bool IsStandingFromAbove(Collision2D collision)
    {
        for (int i = 0; i < collision.contactCount; i++)
        {
            ContactPoint2D contact = collision.GetContact(i);

            if (contact.normal.y > 0.3f)
            {
                return true;
            }
        }

        return false;
    }

    private void ClearCurrentTarget()
    {
        currentReceiver = null;
        currentPlayerCollider = null;

        if (resetTimerWhenExit)
        {
            stayTimer = 0f;
            hasDamagedThisStay = false;
        }

        ApplyVisualState(VisualState.Normal, true);
    }

    private void ApplyVisualState(VisualState nextState, bool force = false)
    {
        if (spriteRenderer == null) return;

        if (!force && currentVisualState == nextState)
        {
            if (nextState == VisualState.Warning && flashDuringWarning)
            {
                UpdateWarningFlash();
            }
            return;
        }

        currentVisualState = nextState;

        switch (nextState)
        {
            case VisualState.Normal:
                if (normalSprite != null) spriteRenderer.sprite = normalSprite;
                spriteRenderer.color = defaultColor;
                break;

            case VisualState.Warning:
                if (warningSprite != null) spriteRenderer.sprite = warningSprite;
                spriteRenderer.color = defaultColor;
                if (flashDuringWarning)
                {
                    UpdateWarningFlash();
                }
                break;

            case VisualState.Danger:
                if (dangerSprite != null) spriteRenderer.sprite = dangerSprite;
                spriteRenderer.color = defaultColor;
                break;
        }
    }

    private void UpdateWarningFlash()
    {
        if (spriteRenderer == null) return;

        float t = (Mathf.Sin(Time.time * flashSpeed) + 1f) * 0.5f;
        float alpha = Mathf.Lerp(0.65f, 1f, t);

        Color c = defaultColor;
        c.a = alpha;
        spriteRenderer.color = c;
    }
}