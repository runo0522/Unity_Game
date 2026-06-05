using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class HealPlatform : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private string playerTag = "Player";

    [Header("Heal")]
    [SerializeField] private int healAmount = 1;

    [Header("Behavior")]
    [SerializeField] private bool requireStandingFromAbove = true;
    [SerializeField] private bool healOncePerStay = true;
    [SerializeField] private bool disableAfterUse = true;

    [Header("Visual")]
    [SerializeField] private SpriteRenderer targetRenderer;
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite usedSprite;

    private Collider2D currentPlayerCollider;
    private bool healedThisStay;
    private bool isUsed;

    private void Awake()
    {
        if (targetRenderer == null)
        {
            targetRenderer = GetComponent<SpriteRenderer>();
        }

        ApplyVisual();
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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryHeal(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (!healOncePerStay) return;
        if (!healedThisStay)
        {
            TryHeal(collision);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (currentPlayerCollider == null) return;
        if (collision.collider != currentPlayerCollider) return;

        currentPlayerCollider = null;
        healedThisStay = false;
    }

    private void TryHeal(Collision2D collision)
    {
        if (isUsed && disableAfterUse) return;
        if (!collision.collider.CompareTag(playerTag)) return;

        if (requireStandingFromAbove && !IsStandingFromAbove(collision))
        {
            return;
        }

        PlayerHeartSystem heartSystem = collision.collider.GetComponent<PlayerHeartSystem>();
        if (heartSystem == null)
        {
            heartSystem = collision.collider.GetComponentInParent<PlayerHeartSystem>();
        }

        if (heartSystem == null) return;

        if (healOncePerStay && healedThisStay && currentPlayerCollider == collision.collider)
        {
            return;
        }

        bool healed = heartSystem.TryHealRedHeart(healAmount);

        currentPlayerCollider = collision.collider;
        healedThisStay = true;

        if (healed)
        {
            if (disableAfterUse)
            {
                isUsed = true;
                ApplyVisual();
            }
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

    private void ApplyVisual()
    {
        if (targetRenderer == null) return;

        if (isUsed)
        {
            if (usedSprite != null)
            {
                targetRenderer.sprite = usedSprite;
            }
        }
        else
        {
            if (normalSprite != null)
            {
                targetRenderer.sprite = normalSprite;
            }
        }
    }
}